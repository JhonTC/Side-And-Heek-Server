using System.Collections.Generic;
using UnityEngine;

public class ItemHandler
{
    public static ushort currentItemId = 0;

    public static Dictionary<ushort, SpawnableObject> items = new Dictionary<ushort, SpawnableObject>();

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

        Debug.Log("Initialised packets.");
    }

    public SpawnableObject SpawnItem(ushort _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        SpawnableObject item = null;

        if (!items.ContainsKey(currentItemId))
        {
            item = NetworkObjectsManager.instance.SpawnItem((PickupCode)_code, _position, _rotation);
            item.Init(currentItemId, _creatorId, _code, true);
            /*switch ((PickupCode)_code)
            {
                case PickupCode.JellyBomb:
                    JellyBomb jellybomb = item as JellyBomb;
                    jellybomb.Init(currentItemId, _creatorId, _code, ); //TODO: cleanup to pass params rather than having specific calls
                    break;
                default:
                    item.Init(currentItemId, _creatorId, _code, true);
                    break;
            }*/

            items.Add(item.objectId, item);

            ServerSend.ItemSpawned(item.objectId, item.creatorId, item.activeItemDetails.pickupSO, _position, _rotation);

            currentItemId++;
            //TODO: above will cause issues on server being live for a long time - after many pickup spawns value will be out of range
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

    public static void ClearAllActiveItems()
    {
        foreach (SpawnableObject spawnable in items.Values)
        {
            Object.Destroy(spawnable.gameObject);
        }
        items.Clear();
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
