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

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            
            SendTCPDataToAll(_packet);
        }
    }

    public static void CreateItemSpawner(int _playerId, int _spawnerId, Vector3 _position, TaskDetails taskDetails)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_position);

            if (taskDetails == null)
            {
                _packet.Write((int)TaskCode.NULL_TASK);
                _packet.Write("");
                _packet.Write("");
                _packet.Write(Color.white);
            }
            else if (taskDetails.task == null)
            {
                _packet.Write((int)TaskCode.NULL_TASK);
                _packet.Write("");
                _packet.Write("");
                _packet.Write(Color.white);
            }
            else
            {
                _packet.Write((int)taskDetails.task.taskCode);
                _packet.Write(taskDetails.task.taskName);
                _packet.Write(taskDetails.task.taskContent);
                _packet.Write(taskDetails.task.taskDifficulty.color);
            }

            SendTCPData(_playerId, _packet);
        }
    }
    public static void CreateItemSpawner(int _spawnerId, Vector3 _position, TaskDetails taskDetails)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_position);

            if (taskDetails == null)
            {
                _packet.Write((int)TaskCode.NULL_TASK);
                _packet.Write("");
                _packet.Write("");
                _packet.Write(Color.white);
            }
            else if (taskDetails.task == null)
            {
                _packet.Write((int)TaskCode.NULL_TASK);
                _packet.Write("");
                _packet.Write("");
                _packet.Write(Color.white);
            } 
            else
            {
                _packet.Write((int)taskDetails.task.taskCode);
                _packet.Write(taskDetails.task.taskName);
                _packet.Write(taskDetails.task.taskContent);
                _packet.Write(taskDetails.task.taskDifficulty.color);
            }

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemSpawned(int _spawnerId, TaskSO task)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);

            _packet.Write((int)task.taskCode);
            _packet.Write(task.taskName);
            _packet.Write(task.taskContent);
            _packet.Write(task.taskDifficulty.color);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemPickedUp(int _spawnerId, int _byPlayer, TaskCode _code)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);
            _packet.Write((int)_code);

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

    public static void SetPlayerColour(int _playerId, Color _colour, bool _isSeekerColour)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setPlayerColour))
        {
            _packet.Write(_playerId);
            _packet.Write(_colour);
            _packet.Write(_isSeekerColour);

            SendTCPDataToAll(_playerId, _packet);
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

    public static void TaskProgressed(int _playerId, TaskCode _code, float progression)
    {
        using (Packet _packet = new Packet((int)ServerPackets.taskProgressed))
        {
            _packet.Write(_playerId);
            _packet.Write((int)_code);
            _packet.Write(progression);

            SendTCPData(_playerId, _packet);
        }
    }

    public static void TaskComplete(int _playerId, TaskCode _code)
    {
        using (Packet _packet = new Packet((int)ServerPackets.taskComplete))
        {
            _packet.Write(_playerId);
            _packet.Write((int)_code);

            SendTCPData(_playerId, _packet);
        }
    }

    #endregion
}
