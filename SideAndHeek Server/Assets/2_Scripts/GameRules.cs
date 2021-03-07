using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameRules
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

    public bool continuousFlop;


    public GameRules(int _gameLength = 180, 
        int _numberOfHunters = 1, CatchType _catchType = CatchType.OnTouch, int _hidingTime = 20, 
        SpeedBoostType _speedBoostType = SpeedBoostType.FirstHunter, float _speedMultiplier = 1.1f,
        HiderFallRespawnType _fallRespawnType = HiderFallRespawnType.Hider, FallRespawnLocation _fallRespawnLocation = FallRespawnLocation.Centre,
        bool _continuousFlop = false)
    {
        gameLength = _gameLength;

        numberOfHunters = _numberOfHunters;
        catchType = _catchType;
        hidingTime = _hidingTime;

        speedBoostType = _speedBoostType;
        speedMultiplier = _speedMultiplier; // Mathf.Clamp(_speedMultiplier, minSpeedMultiplier, maxSpeedMultiplier);

        fallRespawnType = _fallRespawnType;
        fallRespawnLocation = _fallRespawnLocation;

        continuousFlop = _continuousFlop;
    }
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
