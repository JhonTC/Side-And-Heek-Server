using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameRules
{
    public GameType gameType;
    public string friendlyName;

    public bool continuousFlop;

    public virtual Message AddMessageValues(Message message)
    {
        message.AddInt((int)gameType);

        return message;
    }

    public virtual void ReadMessageValues(Message message) { }

    public static GameRules CreateGameRulesFromType(GameType _gameType)
    {
        GameRules gameRules = null;
        switch (_gameType)
        {
            case GameType.HideAndSeek:
                gameRules = new GR_HideAndSeek(_gameType);
                break;
            case GameType.Deathmatch:
                gameRules = new GR_Deathmatch(_gameType);
                break;
        }

        return gameRules;
    }

    public virtual Dictionary<string, object> GetListOfValues()
    {
        return null;
    }
}

public enum GameType
{
    HideAndSeek,
    Deathmatch
}

public enum CatchType
{
    OnFlop,
    OnTouch
}

public enum SpeedBoostType
{
    FirstHunter,
    AllHunters,
    None
}

public enum FallRespawnLocation
{
    Centre,
    Random
}

public enum HiderFallRespawnType
{
    Hider,
    Hunter,
    Spectator
}
