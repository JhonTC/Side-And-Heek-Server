using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GR_Deathmatch : GameRules
{
    [Range(10, 360)]
    public int gameLength;
    [Range(0, 10)]
    public int playerLives;

    public GR_Deathmatch(GameType gameType)
    {
        this.gameType = gameType;
    }

    public override Message AddMessageValues(Message message)
    {
        base.AddMessageValues(message);

        message.AddInt(gameLength);
        message.AddInt(playerLives);
        message.AddBool(continuousFlop);

        return message;
    }

    public override void ReadMessageValues(Message message)
    {
        gameLength = message.GetInt();
        playerLives = message.GetInt();
        continuousFlop = message.GetBool();
    }

    public override Dictionary<string, object> GetListOfValues()
    {
        Dictionary<string, object> retList = new Dictionary<string, object>();

        retList.Add("Game Length", gameLength);
        retList.Add("Player Lives", playerLives);
        retList.Add("Continuous Flop", continuousFlop);

        return retList;
    }
}
