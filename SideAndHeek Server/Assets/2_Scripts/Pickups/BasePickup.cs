using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasePickup
{
    public PickupSO pickupSO;

    public BasePickup(PickupSO _pickupSO)
    {
        pickupSO = _pickupSO;
    }

    public virtual void PickupUsed() { }
}

public class SuperFlop : BasePickup
{
    Player owner;

    public SuperFlop(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        owner.movementController.flopForceMultiplier = pickupSO.power;
        owner.movementController.OnFlopKey(false);
        owner.ItemUseComplete();
    }
}

public class SuperJump : BasePickup
{
    Player owner;

    public SuperJump(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        owner.movementController.jumpForceMultiplier = pickupSO.power;
        owner.movementController.OnJump();
        owner.ItemUseComplete();
    }
}

public class JellyBombItem : BasePickup
{
    Player owner;

    public JellyBombItem(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        NetworkObjectsManager.instance.itemHandler.SpawnItem(owner.id, (int)pickupSO.pickupCode, owner.movementController.transform.position, owner.movementController.transform.rotation);
    }
}

public class SuperSpeed : BasePickup
{
    Player owner;

    public SuperSpeed(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        owner.movementController.forwardForceMultipler = pickupSO.power;

        NetworkObjectsManager.instance.PerformSecondsCountdown((int)pickupSO.duration, OnComplete);
    }

    public void OnComplete()
    {
        owner.movementController.forwardForceMultipler = owner.movementController.maxForwardForceMultipler;
        owner.ItemUseComplete();
    }
}

public class Invisibility : BasePickup
{
    Player owner;

    public Invisibility(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        ServerSend.SetPlayerMaterialType(owner.id, MaterialType.Invisible);

        NetworkObjectsManager.instance.PerformSecondsCountdown((int)pickupSO.duration, OnComplete);
    }

    public void OnComplete()
    {
        ServerSend.SetPlayerMaterialType(owner.id, MaterialType.Default);
        owner.ItemUseComplete();
    }
}

public class TeleportItem : BasePickup
{
    Player owner;

    public TeleportItem(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        //NetworkObjectsManager.instance.itemHandler.SpawnItem(owner.id, (int)pickupSO.pickupCode, owner.movementController.transform.position, owner.movementController.transform.rotation);
    }
}

public class Morph : BasePickup
{
    Player owner;
    Color playerOGColour;

    public Morph(PickupSO _pickupSO, Player _owner) : base(_pickupSO)
    {
        owner = _owner;
    }

    public override void PickupUsed()
    {
        playerOGColour = owner.activeColour;

        ServerSend.SetPlayerColour(owner.id, GameManager.instance.hunterColour, false, true);

        NetworkObjectsManager.instance.PerformSecondsCountdown((int)pickupSO.duration, OnComplete);
    }

    public void OnComplete()
    {
        ServerSend.SetPlayerColour(owner.id, playerOGColour, false, true);
        owner.ItemUseComplete();
    }
}