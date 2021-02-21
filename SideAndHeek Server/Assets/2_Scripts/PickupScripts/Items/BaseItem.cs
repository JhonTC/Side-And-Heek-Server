using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseItem
{
    public ItemPickup item;

    public BaseItem(ItemPickup _item)
    {
        item = _item;
    }

    public virtual void ItemUsed() { }
}

public class SuperFlop : BaseItem
{
    Player owner;

    public SuperFlop(ItemPickup _item, Player _owner) : base(_item)
    {
        owner = _owner;
    }

    public override void ItemUsed()
    {
        owner.movementController.flopForceMultiplier = item.power;
        owner.movementController.OnFlopKey(false);
    }
}

public class SuperJump : BaseItem
{
    Player owner;

    public SuperJump(ItemPickup _item, Player _owner) : base(_item)
    {
        owner = _owner;
    }

    public override void ItemUsed()
    {
        owner.movementController.jumpForceMultiplier = item.power;
        owner.movementController.OnJump();
    }
}