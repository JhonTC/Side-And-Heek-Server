using Riptide;
using UnityEngine;

public class ServerSend
{
    public static void Welcome(ushort _toClient, string _msg, GameRules _gameRules, Color[] _hiderColours)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.welcome);
        message.AddString(_msg);
        message.AddUShort(_toClient);
        message.AddGameRules(_gameRules);
        message.AddInt(_hiderColours.Length);
        for (int i = 0; i < _hiderColours.Length; i++)
        {
            message.AddColour(_hiderColours[i]);
        }

        NetworkManager.Instance.Server.Send(message, _toClient);
    }

    /*public static void SpawnPlayer(int _toClient, Player _player, bool _isHost)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.Id);
            _packet.Write(_player.Username);
            _packet.Write(_player.isReady);
            _packet.Write(_isHost);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_player.activeColour);

            SendTCPData(_toClient, _packet);
        }
    }*/

    public static void PlayerPositions(Player _player)
    {
        if (_player.isBodyActive)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerPosition);
            message.AddUShort(_player.Id);
            message.AddVector3(_player.movementController.root.position);
            message.AddVector3(_player.movementController.rightFootCollider.foot.position);
            message.AddVector3(_player.movementController.leftFootCollider.foot.position);
            message.AddVector3(_player.movementController.rightLeg.position);
            message.AddVector3(_player.movementController.leftLeg.position);

            message.AddQuaternion(_player.movementController.rightFootCollider.foot.rotation);
            message.AddQuaternion(_player.movementController.leftFootCollider.foot.rotation);
            message.AddQuaternion(_player.movementController.rightLeg.rotation);
            message.AddQuaternion(_player.movementController.leftLeg.rotation);

            NetworkManager.Instance.Server.SendToAll(message);
        }
    }

    public static void PlayerRotations(Player _player)
    {
        if (_player.isBodyActive)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerRotation);
            message.AddUShort(_player.Id);
            message.AddQuaternion(_player.movementController.root.rotation);

            NetworkManager.Instance.Server.SendToAll(message);
        }
    }

    public static void PlayerState(Player _player)
    {
        if (_player.isBodyActive)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerState);
            message.AddUShort(_player.Id);
            message.AddBool(_player.movementController.largeGroundCollider.isGrounded);
            message.AddBool(_player.headCollided);
            message.AddFloat(_player.movementController.root.velocity.magnitude);
            message.AddFloat(_player.inputSpeed);
            message.AddBool(_player.footCollided);

            message.AddBool(_player.movementController.isJumping);
            message.AddBool(_player.movementController.isFlopping);
            message.AddBool(_player.movementController.isSneaking);

            NetworkManager.Instance.Server.SendToAll(message);

            if (_player.headCollided)
            {
                _player.headCollided = false;
            }

            if (_player.footCollided)
            {
                _player.footCollided = false;
            }
        }
    }

    public static void CreatePickupSpawner(ushort _spawnerId, Vector3 _position)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.createItemSpawner);
        message.AddUShort(_spawnerId);
        message.AddVector3(_position);
        message.AddBool(false);

        NetworkManager.Instance.Server.SendToAll(message);
    }
    public static void CreatePickupSpawner(ushort _spawnerId, Vector3 _position, ushort _playerId, Pickup _pickup = null)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.createItemSpawner);
        message.AddUShort(_spawnerId);
        message.AddVector3(_position);
        message.AddBool(_pickup != null);
        if (_pickup != null)
        {
            message.AddUShort(_pickup.objectId);
            message.AddInt((int)_pickup.activeItemDetails.pickupSO.pickupCode);
        }

        NetworkManager.Instance.Server.Send(message, _playerId);
    }

    public static void PickupSpawned(ushort _pickupId, bool _bySpawner, ushort _id, PickupSO _pickup, Vector3 _position, Quaternion _rotation)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.pickupSpawned);
        message.AddUShort(_pickupId);
        message.AddBool(_bySpawner);
        message.AddUShort(_id);
        message.AddVector3(_position);
        message.AddQuaternion(_rotation);
        message.AddInt((int)_pickup.pickupCode);
        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void ItemSpawned(ushort _pickupId, ushort _id, PickupSO _pickup, Vector3 _position, Quaternion _rotation)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.itemSpawned);
        message.AddUShort(_pickupId);
        message.AddUShort(_id);
        message.AddVector3(_position);
        message.AddQuaternion(_rotation);
        message.AddInt((int)_pickup.pickupCode);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void ItemTransform(ushort _id, Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.itemTransform);
        message.AddUShort(_id);
        message.AddVector3(_position);
        message.AddQuaternion(_rotation);
        message.AddVector3(_scale);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void ItemUseComplete(ushort _playerId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.itemUseComplete);
        message.AddUShort(_playerId);

        NetworkManager.Instance.Server.Send(message, _playerId);
    }

    public static void PickupPickedUp(ushort _pickupId, ushort _byPlayer, int _code)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.pickupPickedUp);
        message.AddUShort(_pickupId);
        message.AddUShort(_byPlayer);
        message.AddInt(_code);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void PlayerReadyToggled(ushort _playerId, bool _isReady)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerReadyToggled);
        message.AddUShort(_playerId);
        message.AddBool(_isReady);

        NetworkManager.Instance.Server.SendToAll(message, _playerId);
    }

    public static void PlayerReadyReset(ushort _playerId, bool _isReady)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerReadyToggled);
        message.AddUShort(_playerId);
        message.AddBool(_isReady);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void ChangeScene(string _sceneToLoad)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.changeScene);
        message.AddString(_sceneToLoad);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void UnloadScene(string _sceneToUnload)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.unloadScene);
        message.AddString(_sceneToUnload);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void SetPlayerType(Player _player) { SetPlayerType(_player.Id, _player.playerType, true); }
    public static void SetPlayerType(ushort _playerId, PlayerType _playerType, bool _sendToAll)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.setPlayerType);
        message.AddUShort(_playerId);
        message.AddInt((int)_playerType);

        if (_sendToAll)
        {
            NetworkManager.Instance.Server.SendToAll(message);
        }
        else
        {
            NetworkManager.Instance.Server.Send(message, _playerId);
        }
        
        SetPlayerMaterialType(_playerId, MaterialType.Default);
    }

    public static void SetSpecialCountdown(ushort _specialId, int _countdownValue, bool _isCountdownActive)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.setSpecialCountdown);
        message.AddUShort(_specialId);
        message.AddInt(_countdownValue);
        message.AddBool(_isCountdownActive);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void SetPlayerColour(ushort _playerId, Color _colour, bool _isSeekerColour, bool _isSpecialColour = false)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.setPlayerColour);
        message.AddUShort(_playerId);
        message.AddColour(_colour);
        message.AddBool(_isSeekerColour);
        message.AddBool(_isSpecialColour);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void SetPlayerMaterialType(ushort _playerId, MaterialType _materialType)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.setPlayerMaterialType);
        message.AddUShort(_playerId);
        message.AddInt((int)_materialType);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void SendErrorResponse(ErrorResponseCode _responseCode)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.sendErrorResponseCode);
        message.AddInt((int)_responseCode);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void GameStarted(int gameDuration)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.gameStart);
        message.AddInt(gameDuration);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void GameOver(bool _isHunterVictory)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.gameOver);
        message.AddBool(_isHunterVictory);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void PlayerTeleported(ushort _playerId, Vector3 _position)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerTeleported);
        message.AddUShort(_playerId);
        message.AddVector3(_position);

        NetworkManager.Instance.Server.Send(message, _playerId);
    }

    public static void GameRulesChanged(ushort _playerId, GameRules _gameRules)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.gameRulesChanged);
        message.AddUShort(_playerId);
        message.AddGameRules(_gameRules);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public static void SetPlayerHost(Player _player)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.setPlayerHost);
        message.AddUShort(_player.Id);
        message.AddBool(_player.isHost);

        NetworkManager.Instance.Server.SendToAll(message);
    }
}
