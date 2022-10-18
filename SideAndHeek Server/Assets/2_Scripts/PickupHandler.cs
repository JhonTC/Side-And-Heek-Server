using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupHandler
{
    public static Dictionary<ushort, Pickup> pickups = new Dictionary<ushort, Pickup>();

    public static Dictionary<PickupCode, int> pickupLog = new Dictionary<PickupCode, int>();

    private delegate BasePickup PickupHandlerDelegate(PickupSO _pickupSO, Player _player);

    private static Dictionary<PickupCode, PickupHandlerDelegate> pickupHandlers;

    public PickupHandler()
    {
        InitialisePickupData();
    }

    private void InitialisePickupData()
    {
        pickupHandlers = new Dictionary<PickupCode, PickupHandlerDelegate>()
        {
            { PickupCode.NULL,  NullPickup },
            { PickupCode.SuperFlop, SuperFlop },
            { PickupCode.SuperJump, SuperJump },
            { PickupCode.JellyBomb, JellyBomb },
            { PickupCode.SuperSpeed_3, SuperSpeed },
            { PickupCode.SuperSpeed_6, SuperSpeed },
            { PickupCode.SuperSpeed_9, SuperSpeed },
            { PickupCode.Invisibility, Invisibility },
            { PickupCode.Teleport, Teleport },
            { PickupCode.Morph, Morph }
        };
        Debug.Log("Initialised packets.");
    }

    public Pickup SpawnPickup(ushort _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        Pickup pickup = null;

        //if (!pickups.ContainsKey(pickupId))
        //{
            pickup = NetworkObjectsManager.instance.SpawnPickup(_position, _rotation);
            pickup.Init(_spawner, _creatorId, _code);
            pickups.Add(pickup.objectId, pickup);

            ServerSend.PickupSpawned(pickup.objectId, true, pickup.creatorId, pickup.activeItemDetails.pickupSO, _position, _rotation);

        //}

        return pickup;
    }

    public bool CanPickupCodeBeUsed(PickupDetails pickupDetails)
    {
        if (pickupLog.ContainsKey(pickupDetails.pickupSO.pickupCode))
        {
            if (pickupLog[pickupDetails.pickupSO.pickupCode] >= pickupDetails.numberOfUses)
            {
                return false;
            }
        }

        return true;
    }

    public bool HaveAllPickupsBeenSpawned()
    {
        if (pickupLog.Count < GameManager.instance.collection.pickupDetails.Count)
        {
            return false;
        }

        foreach (PickupCode pickupCode in pickupLog.Keys)
        {
            if (pickupLog.ContainsKey(pickupCode))
            {
                if (pickupLog[pickupCode] < GameManager.instance.collection.GetPickupByCode(pickupCode).numberOfUses)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }


    public BasePickup HandlePickup(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = pickupHandlers[_pickupSO.pickupCode](_pickupSO, _player);
        return ret;
    }

    private BasePickup NullPickup(PickupSO _pickupSO, Player _player)
    {
        return null;
    }
    private BasePickup SuperFlop(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new SuperFlop(_pickupSO, _player);
        return ret;
    }
    private BasePickup SuperJump(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new SuperJump(_pickupSO, _player);
        return ret;
    }
    private BasePickup JellyBomb(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new JellyBombItem(_pickupSO, _player);
        return ret;
    }
    private BasePickup SuperSpeed(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new SuperSpeed(_pickupSO, _player);
        return ret;
    }
    private BasePickup Invisibility(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new Invisibility(_pickupSO, _player);
        return ret;
    }
    private BasePickup Teleport(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new TeleportItem(_pickupSO, _player);
        return ret;
    }
    private BasePickup Morph(PickupSO _pickupSO, Player _player)
    {
        BasePickup ret = new Morph(_pickupSO, _player);
        return ret;
    }
}
