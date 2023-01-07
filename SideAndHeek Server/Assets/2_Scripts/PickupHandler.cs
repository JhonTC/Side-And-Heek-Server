using System.Collections.Generic;
using UnityEngine;

public class PickupHandler //TODO: Make into Singleton
{
    public static Dictionary<PickupType, int> pickupLog = new Dictionary<PickupType, int>();

    private delegate BasePickup PickupHandlerDelegate(PickupSO _pickupSO, Player _player);

    private static Dictionary<PickupType, PickupHandlerDelegate> pickupHandlers;

    public PickupHandler()
    {
        InitialisePickupData();
    }

    private void InitialisePickupData()
    {
        pickupHandlers = new Dictionary<PickupType, PickupHandlerDelegate>()
        {
            { PickupType.NULL,  NullPickup },
            { PickupType.SuperFlop, SuperFlop },
            { PickupType.SuperJump, SuperJump },
            { PickupType.JellyBomb, JellyBomb },
            { PickupType.SuperSpeed_3, SuperSpeed },
            { PickupType.SuperSpeed_6, SuperSpeed },
            { PickupType.SuperSpeed_9, SuperSpeed },
            { PickupType.Invisibility, Invisibility },
            { PickupType.Teleport, Teleport },
            { PickupType.Morph, Morph }
        };
        Debug.Log("Initialised packets.");
    }

    public Pickup SpawnPickup(ushort _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        Pickup pickup = NetworkObjectsManager.instance.SpawnObject(NetworkedObjectType.Pickup, _position, _rotation, false) as Pickup;
        if (pickup != null)
        {
            pickup.Init(_spawner, _creatorId, _code);

            ServerSend.PickupSpawned(pickup.objectId, true, pickup.creatorId, pickup.activeItemDetails.pickupSO, _position, _rotation);
        }

        return pickup;
    }

    public static bool CanPickupCodeBeUsed(PickupDetails pickupDetails)
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

    public static bool HaveAllPickupsBeenSpawned()
    {
        if (pickupLog.Count < GameManager.instance.collection.pickupDetails.Count)
        {
            return false;
        }

        foreach (PickupType pickupCode in pickupLog.Keys)
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

    public static void ResetPickupLog()
    {
        pickupLog.Clear();
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
