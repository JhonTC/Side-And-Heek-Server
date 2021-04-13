using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new pickupSO", menuName = "Data/PickupSO")]
public class PickupSO : ScriptableObject
{
    public string pickupName;
    public string pickupContent;
    public Sprite pickupIcon;
    public PickupLevel pickupLevel;

    public PlayerType userType;

    public PickupCode pickupCode;
    public float power;
    public float duration;
}

public enum PickupCode
{
    NULL,
    SuperFlop,
    SuperJump,
    JellyBomb,
    SuperSpeed_3,
    SuperSpeed_6,
    SuperSpeed_9,
    Invisibility,
    Teleport,
    Morph,
    BearTrap
}
