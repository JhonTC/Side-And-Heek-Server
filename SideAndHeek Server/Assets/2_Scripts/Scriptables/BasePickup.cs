using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasePickup : ScriptableObject
{
    public string pickupName;
    public string pickupContent;
    public Sprite pickupIcon;
    public PickupLevel pickupLevel;

    public PickupType pickupType;
}

public enum PickupType
{
    NULL,
    Task,
    Item
}
