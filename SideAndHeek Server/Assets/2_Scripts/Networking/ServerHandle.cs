using UnityEngine;

public class ServerHandle
{
    public static void WelcomeRecieved(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\"(ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        float inputSpeed = _packet.ReadFloat();

        bool[] _otherInputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _otherInputs.Length; i++)
        {
            _otherInputs[i] = _packet.ReadBool();
        }

        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(inputSpeed, _otherInputs, _rotation);
    }

    public static void PlayerReady(int _fromClient, Packet _packet)
    {
        bool _isReady = _packet.ReadBool();

        Server.clients[_fromClient].player.SetReady(_isReady);
    }

    public static void SetPlayerColour(int _fromClient, Packet _packet)
    {
        Color _colour = _packet.ReadColour();
        bool _isSeekerColour = _packet.ReadBool();

        ServerSend.SetPlayerColour(_fromClient, _colour, _isSeekerColour);
    }

    public static void TryStartGame(int _fromClient, Packet _packet)
    {
        GameManager.instance.TryStartGame(_fromClient);
    }
}
