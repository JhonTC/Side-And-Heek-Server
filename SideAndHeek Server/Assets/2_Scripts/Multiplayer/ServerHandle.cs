using Riptide;
using System;
using UnityEngine;

public class ServerHandle
{
    [MessageHandler((ushort)ClientToServerId.name)]
    public static void Name(ushort fromClientId, Message message)
    {
        ServerSend.Welcome(fromClientId, "Welcome", GameManager.instance.gameMode.GetGameRules(), GameManager.instance.hiderColours);
        Player.Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.playerMovement)]
    public static void SetInputs(ushort fromClientId, Message message)
    {
        float inputSpeed = message.GetFloat();
        bool[] _otherInputs = message.GetBools(3);
        Quaternion _rotation = message.GetQuaternion();

        Player.list[fromClientId].SetInput(inputSpeed, _otherInputs, _rotation);
    }

    [MessageHandler((ushort)ClientToServerId.playerReady)]
    public static void PlayerReady(ushort fromClientId, Message message)
    {
        bool _isReady = message.GetBool();

        Player.list[fromClientId].SetReady(_isReady);
    }

    [MessageHandler((ushort)ClientToServerId.setPlayerColour)]
    public static void SetPlayerColour(ushort fromClientId, Message message)
    {
        Color _newColour = message.GetColour(); 
        bool _isSeekerColour = message.GetBool();

        bool isColourChangeAllowed = true;
        if (!GameManager.instance.gameStarted && !_isSeekerColour)
        {
            Color previousColour = Player.list[fromClientId].activeColour;
            isColourChangeAllowed = GameManager.instance.ClaimHiderColour(previousColour, _newColour);
        }

        if (isColourChangeAllowed)
        {
            Player.list[fromClientId].activeColour = _newColour;
            ServerSend.SetPlayerColour(fromClientId, _newColour, _isSeekerColour);
        } else
        {
            //todo: send error response - colour already chosen
        }
    }

    [MessageHandler((ushort)ClientToServerId.tryStartGame)]
    public static void TryStartGame(ushort fromClientId, Message message)
    {
        GameManager.instance.TryStartGame(fromClientId);
    }

    [MessageHandler((ushort)ClientToServerId.pickupSelected)]
    public static void PickupSelected(ushort fromClientId, Message message)
    {
        ushort _pickupId = message.GetUShort();

        if (PickupHandler.pickups.ContainsKey(_pickupId))
        {
            PickupHandler.pickups[_pickupId].PickupPickedUp(fromClientId);
        } else
        {
            Debug.Log($"ERROR: No pickup with id {_pickupId}");
        }
    }

    [MessageHandler((ushort)ClientToServerId.itemUsed)]
    public static void ItemUsed(ushort fromClientId, Message message)
    {
        if (message.GetBool())
        {
            Player.list[fromClientId].shootDirection = message.GetVector3();
        }

        Player.list[fromClientId].PickupUsed();
    }

    [MessageHandler((ushort)ClientToServerId.gameRulesChanged)]
    public static void GameRulesChanged(ushort fromClientId, Message message)
    {
        GameType gameType = (GameType)message.GetInt();
        if (GameManager.instance.gameType != gameType)
        {
            GameManager.instance.GameTypeChanged(gameType); //todo: this shouldnt be here
        }

        GameRules gameRules = message.GetGameRules();

        GameManager.instance.GameRulesChanged(gameRules);
        ServerSend.GameRulesChanged(fromClientId, gameRules);

        Debug.Log($"Game Rules Changed by player with id {fromClientId}");
    }
}
