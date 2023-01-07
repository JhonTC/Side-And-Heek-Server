using System.Collections.Generic;
using UnityEngine;

public class ItemHandler
{
    public static Dictionary<ushort, SpawnableObject> items = new Dictionary<ushort, SpawnableObject>();

    public static Dictionary<PickupType, int> itemLog = new Dictionary<PickupType, int>();

    private delegate BasePickup ItemHandlerDelegate(PickupSO _pickupSO, Player _player);

    private static Dictionary<PickupType, ItemHandlerDelegate> itemHandlers;
    //private static Dictionary<PickupCode, Type> itemHondlers;

    public ItemHandler()
    {
        InitialiseItemData();
    }

    private void InitialiseItemData()
    {
        itemHandlers = new Dictionary<PickupType, ItemHandlerDelegate>()
        {
            { PickupType.NULL,  NullItem },
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

    public SpawnableObject SpawnItem(ushort _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        SpawnableObject item = NetworkObjectsManager.instance.SpawnObject(NetworkedObjectType.Item, _position, _rotation, true, (PickupType)_code) as SpawnableObject;
        if (item != null)
        {
            item.Init(_creatorId, _code);

            ServerSend.ItemSpawned(item.objectId, item.creatorId, item.activeItemDetails.pickupSO, _position, _rotation);
        }

        return item;
    }

    public bool CanPickupCodeBeUsed(PickupDetails pickupDetails)
    {
        if (itemLog.ContainsKey(pickupDetails.pickupSO.pickupCode))
        {
            if (itemLog[pickupDetails.pickupSO.pickupCode] >= pickupDetails.numberOfUses)
            {
                return false;
            }
        }

        return true;
    }

    public bool HaveAllPickupsBeenSpawned()
    {
        if (itemLog.Count < GameManager.instance.collection.pickupDetails.Count)
        {
            return false;
        }

        foreach (PickupType pickupCode in itemLog.Keys)
        {
            if (itemLog.ContainsKey(pickupCode))
            {
                if (itemLog[pickupCode] < GameManager.instance.collection.GetPickupByCode(pickupCode).numberOfUses)
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
        BasePickup ret = itemHandlers[_pickupSO.pickupCode](_pickupSO, _player);
        return ret;
    }

    private BasePickup NullItem(PickupSO _pickupSO, Player _player)
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
