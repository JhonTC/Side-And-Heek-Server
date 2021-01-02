﻿using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exeptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exeptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exeptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exeptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.isReady);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void PlayerPositions(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.controller.root.position);
            _packet.Write(_player.controller.rightFootCollider.foot.position);
            _packet.Write(_player.controller.leftFootCollider.foot.position);
            _packet.Write(_player.controller.rightLeg.position);
            _packet.Write(_player.controller.leftLeg.position);

            _packet.Write(_player.controller.rightFootCollider.foot.rotation);
            _packet.Write(_player.controller.leftFootCollider.foot.rotation);
            _packet.Write(_player.controller.rightLeg.rotation);
            _packet.Write(_player.controller.leftLeg.rotation);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerRotations(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.controller.root.rotation);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            
            SendTCPDataToAll(_packet);
        }
    }

    public static void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_position);
            _packet.Write(_hasItem);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerReadyToggled(int _playerId, bool _isReady)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerReadyToggled))
        {
            _packet.Write(_playerId);
            _packet.Write(_isReady);

            SendTCPDataToAll(_playerId, _packet);
        }
    }

    public static void ChangeScene(string _sceneToLoad)
    {
        using (Packet _packet = new Packet((int)ServerPackets.changeScene))
        {
            _packet.Write(_sceneToLoad);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SetPlayerType(int _playerId) { SetPlayerType(_playerId, PlayerType.Default, false); }
    public static void SetPlayerType(Player _player) { SetPlayerType(_player.id, _player.playerType, true); }
    public static void SetPlayerType(int _playerId, PlayerType _playerType, bool _sendToAll)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setPlayerType))
        {
            _packet.Write(_playerId);
            _packet.Write((int)_playerType);

            if (_sendToAll)
            {
                SendTCPDataToAll(_packet);
            } else
            {
                SendTCPData(_playerId, _packet);
            }
        }
    }

    public static void SetSpecialCountdown(int _specialId, int _countdownValue, bool _isCountdownActive)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setSpecialCountdown))
        {
            _packet.Write(_specialId);
            _packet.Write(_countdownValue);
            _packet.Write(_isCountdownActive);
            
            SendTCPDataToAll(_packet);
        }
    }

    #endregion
}
