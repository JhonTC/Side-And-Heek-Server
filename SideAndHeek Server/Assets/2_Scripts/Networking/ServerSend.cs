using UnityEngine;

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
    public static void Welcome(int _toClient, string _msg, GameRules _gameRules, Color[] _hiderColours)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            _packet.Write(_gameRules);
            _packet.Write(_hiderColours.Length);
            for (int i = 0; i < _hiderColours.Length; i++)
            {
                _packet.Write(_hiderColours[i]);
            }

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SpawnPlayer(int _toClient, Player _player, bool _isHost)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.isReady);
            _packet.Write(_isHost);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_player.activeColour);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void PlayerPositions(Player _player)
    {
        if (_player.isBodyActive)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.movementController.root.position);
                _packet.Write(_player.movementController.rightFootCollider.foot.position);
                _packet.Write(_player.movementController.leftFootCollider.foot.position);
                _packet.Write(_player.movementController.rightLeg.position);
                _packet.Write(_player.movementController.leftLeg.position);

                _packet.Write(_player.movementController.rightFootCollider.foot.rotation);
                _packet.Write(_player.movementController.leftFootCollider.foot.rotation);
                _packet.Write(_player.movementController.rightLeg.rotation);
                _packet.Write(_player.movementController.leftLeg.rotation);

                SendUDPDataToAll(_packet);
            }
        }
    }

    public static void PlayerRotations(Player _player)
    {
        if (_player.isBodyActive)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.movementController.root.rotation);

                SendUDPDataToAll(_packet);
            }
        }
    }

    public static void PlayerState(Player _player)
    {
        if (_player.isBodyActive)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerState))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.movementController.largeGroundCollider.isGrounded);
                _packet.Write(_player.inputSpeed);

                _packet.Write(_player.movementController.isJumping);
                _packet.Write(_player.movementController.isFlopping);
                _packet.Write(_player.movementController.isSneaking);

                SendUDPDataToAll(_packet);
            }
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

    public static void CreatePickupSpawner(int _spawnerId, Vector3 _position, int _playerId = -1)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_position);

            if (_playerId == -1)
            {
                SendTCPDataToAll(_packet);
            }
            else
            {
                SendTCPData(_playerId, _packet);
            }
        }
    }

    public static void PickupSpawned(int _pickupId, bool _bySpawner, int _id, PickupSO _pickup, Vector3 _position, Quaternion _rotation)
    {
        using (Packet _packet = new Packet((int)ServerPackets.pickupSpawned))
        {
            _packet.Write(_pickupId);
            _packet.Write(_bySpawner);
            _packet.Write(_id);
            _packet.Write(_position);
            _packet.Write(_rotation);
            _packet.Write((int)_pickup.pickupCode);

            SendTCPDataToAll(_packet);
        }
    }
    public static void ItemSpawned(int _pickupId, int _id, PickupSO _pickup, Vector3 _position, Quaternion _rotation)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_pickupId);
            _packet.Write(_id);
            _packet.Write(_position);
            _packet.Write(_rotation);
            _packet.Write((int)_pickup.pickupCode);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemTransform(int _id, Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemTransform))
        {
            _packet.Write(_id);
            _packet.Write(_position);
            _packet.Write(_rotation);
            _packet.Write(_scale);

            SendUDPDataToAll(_packet);
        }
    }

    public static void ItemUseComplete(int _clientId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemUseComplete))
        {
            _packet.Write(_clientId);

            SendTCPData(_clientId, _packet);
        }
    }

    public static void PickupPickedUp(int _pickupId, int _byPlayer, int _code)
    {
        using (Packet _packet = new Packet((int)ServerPackets.pickupPickedUp))
        {
            _packet.Write(_pickupId);
            _packet.Write(_byPlayer);
            _packet.Write(_code);

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

    public static void PlayerReadyReset(int _playerId, bool _isReady)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerReadyToggled))
        {
            _packet.Write(_playerId);
            _packet.Write(_isReady);

            SendTCPDataToAll(_packet);
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

    public static void UnloadScene(string _sceneToUnload)
    {
        using (Packet _packet = new Packet((int)ServerPackets.unloadScene))
        {
            _packet.Write(_sceneToUnload);

            SendTCPDataToAll(_packet);
        }
    }

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
        
        SetPlayerMaterialType(_playerId, MaterialType.Default);
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

    public static void SetPlayerColour(int _playerId, Color _colour, bool _isSeekerColour, bool _isSpecialColour = false)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setPlayerColour))
        {
            _packet.Write(_playerId);
            _packet.Write(_colour);
            _packet.Write(_isSeekerColour);
            _packet.Write(_isSpecialColour);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SetPlayerMaterialType(int _playerId, MaterialType _materialType)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setPlayerMaterialType))
        {
            _packet.Write(_playerId);
            _packet.Write((int)_materialType);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SendErrorResponse(ErrorResponseCode _responseCode)
    {
        using (Packet _packet = new Packet((int)ServerPackets.sendErrorResponseCode))
        {
            _packet.Write((int)_responseCode);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GameStarted(int gameDuration)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameStart))
        {
            _packet.Write(gameDuration);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GameOver(bool _isHunterVictory)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameOver))
        {
            _packet.Write(_isHunterVictory);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerTeleported(int _playerId, Vector3 _position)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerTeleported))
        {
            _packet.Write(_playerId);
            _packet.Write(_position);

            SendTCPData(_playerId, _packet);
        }
    }

    public static void GameRulesChanged(int _playerId, GameRules gameRules)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameRulesChanged))
        {
            _packet.Write(_playerId);
            _packet.Write(gameRules);

            SendTCPDataToAll(_playerId, _packet);
        }
    }

    #endregion
}
