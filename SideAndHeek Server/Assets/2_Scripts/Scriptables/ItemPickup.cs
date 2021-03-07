using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new item", menuName = "Data/Item")]
public class ItemPickup : BasePickup
{
    public ItemCode itemCode;

    public float power;
}

public enum ItemCode
{
    NULL_ITEM,
    SuperFlop,
    SuperJump,
    JellyBomb
}
