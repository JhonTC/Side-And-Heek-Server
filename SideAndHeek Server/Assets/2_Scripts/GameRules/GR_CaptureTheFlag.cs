using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GR_CaptureTheFlag : GameRules
{
    [Range(2, 10)]
    public int numberOfTeams;
    [Range(60, 360)]
    public int gameLength;
    [Range(1, 20)]
    public int maxScore;
    public GameEndType gameEndType;
    public CatchType catchType;

    public GR_CaptureTheFlag(GameType gameType)
    {
        this.gameType = gameType;
    }

    public override Message AddMessageValues(Message message)
    {
        base.AddMessageValues(message);

        message.AddInt(numberOfTeams);
        message.AddInt(gameLength);
        message.AddInt(maxScore);
        message.AddInt((int)gameEndType);
        message.AddInt((int)catchType);
        message.AddBool(continuousFlop);

        return message;
    }

    public override void ReadMessageValues(Message message)
    {
        numberOfTeams = message.GetInt();
        gameLength = message.GetInt();
        maxScore = message.GetInt();
        gameEndType = (GameEndType)message.GetInt();
        catchType = (CatchType)message.GetInt();
        continuousFlop = message.GetBool();
    }

    public override Dictionary<string, object> GetListOfValues() //Used in GameManagerEditor
    {
        Dictionary<string, object> retList = new Dictionary<string, object>();

        retList.Add("Number Of Teams", numberOfTeams);
        retList.Add("Game Length", gameLength);
        retList.Add("Max Score", maxScore);
        retList.Add("Game End Type", gameEndType);
        retList.Add("Catch Type", catchType);
        retList.Add("Continuous Flop", continuousFlop);

        return retList;
    }
}
