using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameRules
{
    public int gameLength;                          

    [HideInInspector] public int numberOfHunters;                     //* - requires more hunter spawns
    public CatchType catchType;                     
    public int hidingTime;                          

    public SpeedBoostType speedBoostType;           

    public float speedMultiplier;                   
    //private float maxSpeedMultiplier = 1.25f;
    //private float minSpeedMultiplier = 0.75f;

    public GameRules(int _gameLength = 180, int _numberOfHunters = 1, CatchType _catchType = CatchType.OnTouch, int _hidingTime = 20, SpeedBoostType _speedBoostType = SpeedBoostType.FirstHunter, float _speedMultiplier = 1.1f)
    {
        gameLength = _gameLength;

        numberOfHunters = _numberOfHunters;
        catchType = _catchType;
        hidingTime = _hidingTime;

        speedBoostType = _speedBoostType;
        speedMultiplier = _speedMultiplier; // Mathf.Clamp(_speedMultiplier, minSpeedMultiplier, maxSpeedMultiplier);
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
