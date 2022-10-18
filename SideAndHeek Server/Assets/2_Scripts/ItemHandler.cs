using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler
{
    public int currentItemId = 0;

    public static Dictionary<int, SpawnableObject> items = new Dictionary<int, SpawnableObject>();

    public static Dictionary<PickupCode, int> itemLog = new Dictionary<PickupCode, int>();

    private delegate BasePickup ItemHandlerDelegate(PickupSO _pickupSO, Player _player);

    private static Dictionary<PickupCode, ItemHandlerDelegate> itemHandlers;
    //private static Dictionary<PickupCode, Type> itemHondlers;

    public ItemHandler()
    {
        InitialiseItemData();
    }

    private void InitialiseItemData()
    {
        itemHandlers = new Dictionary<PickupCode, ItemHandlerDelegate>()
        {
            { PickupCode.NULL,  NullItem },
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


        /*itemHondlers = new Dictionary<PickupCode, Type>()
        {
            { PickupCode.NULL,  null },
            { PickupCode.SuperFlop, typeof(SuperFlop) },
            { PickupCode.SuperJump, typeof(SuperJump) },
            { PickupCode.JellyBomb, typeof(JellyBombItem) }
        };*/
        Debug.Log("Initialised packets.");
    }

    /*public BasePickup HondlePickup(PickupSO _pickupSO, Player _player)
    {
        if (itemHondlers[_pickupSO.pickupCode] != null)
        {
            object[] parameters = { _pickupSO, _player };
            BasePickup ret = Activator.CreateInstance(itemHondlers[_pickupSO.pickupCode], parameters) as BasePickup;
            return ret;
        }
        
        return null;
    }*/

    public SpawnableObject SpawnItem(ushort _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        SpawnableObject item = null;

        int pickupId = currentItemId + 1;
        if (!items.ContainsKey(pickupId))
        {
            item = NetworkObjectsManager.instance.SpawnItem((PickupCode)_code, _position, _rotation);
            switch((PickupCode)_code)
            {
                case PickupCode.JellyBomb:
                    JellyBomb jellybomb = item as JellyBomb;
                    jellybomb.Init(_creatorId, _code, Player.list[_creatorId].movementController.root.transform.forward, 80);
                    break;
                default:
                    item.Init(_creatorId, _code, true);
                    break;
            }

            items.Add(item.objectId, item);

            ServerSend.ItemSpawned(item.objectId, item.creatorId, item.activeItemDetails.pickupSO, _position, _rotation);

            currentItemId++;
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

        foreach (PickupCode pickupCode in itemLog.Keys)
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
