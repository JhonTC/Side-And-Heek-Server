using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new PickupLevel", menuName = "Data/Pickup Level")]
public class PickupLevel : ScriptableObject
{
    public Level difficulty;
    public Color color;
}

public enum Level
{
    Common,
    Rare,
    Legendary
}
