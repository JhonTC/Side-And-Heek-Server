using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GR_HideAndSeek : GameRules
{
    [Range(60, 360)]
    public int gameLength;

    [HideInInspector] public int numberOfHunters;                     //* - requires more hunter spawns
    public CatchType catchType;
    [Range(0, 60)]
    public int hidingTime;

    public SpeedBoostType speedBoostType;
    [Range(0.8f, 1.2f)]
    public float speedMultiplier;

    public HiderFallRespawnType fallRespawnType;
    public FallRespawnLocation fallRespawnLocation;

    public GR_HideAndSeek(GameType gameType)
    {
        this.gameType = gameType;
    }

    public override Message AddMessageValues(Message message)
    {
        base.AddMessageValues(message);

        message.AddInt(gameLength);
        message.AddInt(numberOfHunters);
        message.AddInt((int)catchType);
        message.AddInt(hidingTime);
        message.AddInt((int)speedBoostType);
        message.AddFloat(speedMultiplier);
        message.AddInt((int)fallRespawnType);
        message.AddInt((int)fallRespawnLocation);
        message.AddBool(continuousFlop);

        return message;
    }

    public override void ReadMessageValues(Message message) 
    {
        gameLength = message.GetInt();
        numberOfHunters = message.GetInt();
        catchType = (CatchType)message.GetInt();
        hidingTime = message.GetInt();
        speedBoostType = (SpeedBoostType)message.GetInt();
        speedMultiplier = message.GetFloat();
        fallRespawnType = (HiderFallRespawnType)message.GetInt();
        fallRespawnLocation = (FallRespawnLocation)message.GetInt();
        continuousFlop = message.GetBool();
    }

    public override Dictionary<string, object> GetListOfValues() //Used in GameManagerEditor
    {
        Dictionary<string, object> retList = new Dictionary<string, object>();

        retList.Add("Game Length", gameLength);
        retList.Add("Number Of Hunters", numberOfHunters);
        retList.Add("Catch Type", catchType);
        retList.Add("Hiding Time", hidingTime);
        retList.Add("Speed Boost Type", speedBoostType);
        retList.Add("Speed Multiplier", speedMultiplier);
        retList.Add("Fall Respawn Type", fallRespawnType);
        retList.Add("Fall Respawn Location", fallRespawnLocation);
        retList.Add("Continuous Flop", continuousFlop);

        return retList;
    }
}
