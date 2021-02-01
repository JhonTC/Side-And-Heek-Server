using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

[System.Serializable]
public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public Player player;
    public bool isHost;
    public string uniqueUserCode;

    public bool isConnected = false;

    public TCP tcp;
    public UDP udp;

    public Client(int _clientId, bool _isHost)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
        isHost = _isHost;
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Welcome to the server");

            Server.clients[id].isConnected = true;
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _e)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {_e}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _e)
            {
                Debug.Log($"Error receiving TCP data: {_e}");
                Server.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string _playerName, string _uniqueUserCode)
    {
        uniqueUserCode = _uniqueUserCode;

        Player disconnectedPlayer = GetDisconnectedPlayerWithUUC(uniqueUserCode);
        int oldPlayerId = -1;
        if (disconnectedPlayer != null)
        {
            player = disconnectedPlayer;
            oldPlayerId = player.id;
        }
        else
        {
            Transform _transform = LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(Server.GetPlayerCount() == 0);
            player = NetworkManager.instance.InstantiatePlayer(_transform.position);
        }
        player.Initialize(id, _playerName, disconnectedPlayer == null);

        if (GameManager.instance.gameStarted)
        {
            ServerSend.ChangeScene(id, GameManager.instance.activeSceneName);
        }

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        if (disconnectedPlayer != null)
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        ServerSend.UpdatePlayerDetails(_client.id, oldPlayerId, id, _playerName);
                    }
                    else
                    {
                        ServerSend.SpawnPlayer(id, player);
                    }
                }
            }
        } else
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player);
                }
            }
        }

        foreach (ItemSpawner _itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.activeTaskDetails);
        }
    }

    private Player GetDisconnectedPlayerWithUUC(string _uniqueUserCode)
    {
        foreach (string key in Server.disconnectedPlayers.Keys)
        {
            if (key == _uniqueUserCode)
            {
                return Server.disconnectedPlayers[key];
            }
        }

        return null;
    }

    public void Disconnect()
    {
        if (/*GameManager.instance.gameStarted*/false) {

            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has Disconnected.");

            isConnected = false;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(player.gameObject);
                player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnected(id, true);
        } else
        {
            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has Disconnected.");

            isConnected = false;

            if (!Server.disconnectedPlayers.ContainsKey(uniqueUserCode))
            {
                Server.disconnectedPlayers.Add(uniqueUserCode, player);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnected(id, false);
            //TODO: send disconnection to clients so all users get a message
        }

        if (Server.clients.ContainsKey(id))
        {
            Debug.Log($"Removed client with id {id}.");
            Server.clients.Remove(id);
        }
    }
}
