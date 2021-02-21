using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitialiseServerData();
        //ErrorResponseHandler.InitialiseErrorResponseData();

        string strHostName = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(strHostName);
        IPAddress address = IPAddress.Any;

        /*Debug.Log(hostEntry.AddressList.Length);
        for (int i = 0; i < hostEntry.AddressList.Length; i++)
        {
            Debug.Log(hostEntry.AddressList[i]);
        }*/
        if (hostEntry.AddressList.Length > 1)
        {
            Debug.Log($"Host Name: {strHostName}, Host Address: {hostEntry.AddressList[1]}");
            address = hostEntry.AddressList[1];
        }

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPRecieveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");
        
        if (!GameManager.instance.gameStarted)
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }
            
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        else if (GameManager.instance.gameStarted)
        {
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Game in progress!");
        }
    }

    private static void UDPRecieveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPRecieveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _e)
        {
            Debug.Log($"Error receiveing UDP data: {_e}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _e)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_e}");
        }
    }

    private static void InitialiseServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i, i == 1));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.playerReady, ServerHandle.PlayerReady },
            { (int)ClientPackets.tryStartGame, ServerHandle.TryStartGame },
            { (int)ClientPackets.setPlayerColour, ServerHandle.SetPlayerColour },
            { (int)ClientPackets.pickupSelected, ServerHandle.PickupSelected },
            { (int)ClientPackets.itemUsed, ServerHandle.ItemUsed },
            { (int)ClientPackets.gameRulesChanged, ServerHandle.GameRulesChanged }
        };
        Debug.Log("Initialised packets");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

    public static int GetPlayerCount()
    {
        int i = 0;
        foreach (Client client in clients.Values)
        {
            if (client.player != null)
            {
                i++;
            }
        }

        return i;
    }
}
