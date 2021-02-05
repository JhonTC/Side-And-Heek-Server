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

    public void SendIntoGame(string _playerName)
    {
        Transform _transform = LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(Server.GetPlayerCount() == 0);
        player = NetworkManager.instance.InstantiatePlayer(_transform.position);
        player.Initialize(id, _playerName, _transform);

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

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        foreach (PickupSpawner _pickupSpawner in PickupSpawner.spawners.Values)
        {
            BasePickup pickup = null;
            if (_pickupSpawner.pickupType == PickupType.Task)
            {
                if (_pickupSpawner.activeTaskDetails != null)
                {
                    pickup = _pickupSpawner.activeTaskDetails.task;
                }
            }
            else if (_pickupSpawner.pickupType == PickupType.Item)
            {
                if (_pickupSpawner.activeItemDetails != null)
                {
                    pickup = _pickupSpawner.activeItemDetails.item;
                }
            }

            ServerSend.CreatePickupSpawner(_pickupSpawner.spawnerId, _pickupSpawner.transform.position, _pickupSpawner.pickupType, _pickupSpawner.hasPickup, pickup, id);
        }
    }

    public void Disconnect()
    {
        if (/*GameManager.instance.gameStarted*/true) {

            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has Disconnected.");

            isConnected = false;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(player.gameObject);
                player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnected(id);
        } else
        {
            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has Disconnected.");

            isConnected = false;

            /*ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(player.gameObject);
                player = null;
            });*/

            tcp.Disconnect();
            udp.Disconnect();

            //ServerSend.PlayerDisconnected(id);
        }
    }
}
