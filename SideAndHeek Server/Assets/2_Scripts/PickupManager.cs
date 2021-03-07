using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager instance;

    public int currentPickupId = 0;
    public static Dictionary<int, Pickup> pickups = new Dictionary<int, Pickup>();

    public static Dictionary<TaskCode, int> tasksLog = new Dictionary<TaskCode, int>();
    public static Dictionary<ItemCode, int> itemsLog = new Dictionary<ItemCode, int>();

    private delegate BaseTask TaskHandler(TaskPickup _task, Player _player);
    private delegate BaseItem ItemHandler(ItemPickup _item, Player _player);

    private static Dictionary<TaskCode, TaskHandler> taskHandlers;
    private static Dictionary<ItemCode, ItemHandler> itemHandlers;

    [SerializeField] private Pickup pickupPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        InitialiseTaskData();
        InitialiseItemData();
    }

    public Pickup SpawnPickup(PickupType _pickupType, int _creatorId, int _code, Vector3 _position, Quaternion _rotation, PickupSpawner _spawner = null)
    {
        Pickup pickup = null;

        int pickupId = currentPickupId + 1;
        if (!pickups.ContainsKey(pickupId))
        {
            pickup = Instantiate(pickupPrefab, _position, _rotation, transform);
            pickup.Init(_spawner, _creatorId, _pickupType, _code, true);
            pickups.Add(pickup.pickupId, pickup);

            if (pickup.pickupType == PickupType.Task)
            {
                ServerSend.PickupSpawned(pickup.pickupId, true, pickup.creatorId, pickup.activeTaskDetails.task, _position, _rotation);
            }
            else
            {
                ServerSend.PickupSpawned(pickup.pickupId, true, pickup.creatorId, pickup.activeItemDetails.item, _position, _rotation);
            }

            currentPickupId++;
        }

        return pickup;
    }

    public bool CanTaskCodeBeUsed(TaskDetails taskDetails)
    {
        if (tasksLog.ContainsKey(taskDetails.task.taskCode))
        {
            if (tasksLog[taskDetails.task.taskCode] >= taskDetails.numberOfUses)
            {
                return false;
            }
        }

        return true;
    }

    public bool CanItemCodeBeUsed(ItemDetails itemDetails)
    {
        if (itemsLog.ContainsKey(itemDetails.item.itemCode))
        {
            if (itemsLog[itemDetails.item.itemCode] >= itemDetails.numberOfUses)
            {
                return false;
            }
        }

        return true;
    }

    public bool HaveAllPickupsOfTypeBeenSpawned(PickupType pickupType)
    {
        if (pickupType == PickupType.Task)
        {
            if (tasksLog.Count < GameManager.instance.collection.taskDetails.Count)
            {
                return false;
            }

            foreach (TaskCode taskCode in tasksLog.Keys)
            {
                if (tasksLog.ContainsKey(taskCode))
                {
                    if (tasksLog[taskCode] < GameManager.instance.collection.GetTaskByCode(taskCode).numberOfUses)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        else if (pickupType == PickupType.Item)
        {
            if (itemsLog.Count < GameManager.instance.collection.itemDetails.Count)
            {
                return false;
            }

            foreach (ItemCode itemCode in itemsLog.Keys)
            {
                if (itemsLog.ContainsKey(itemCode))
                {
                    if (itemsLog[itemCode] < GameManager.instance.collection.GetItemByCode(itemCode).numberOfUses)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }



    private void InitialiseTaskData()
    {
        taskHandlers = new Dictionary<TaskCode, TaskHandler>()
        {
            { TaskCode.NULL_TASK,  NullTask },
            { TaskCode.TestTaskEasy, TestTaskEasy },
            { TaskCode.TestTaskNormal, TestTaskNormal },
            { TaskCode.TestTaskHard, TestTaskHard },
            { TaskCode.TestTaskExtreme, TestTaskExtreme }
        };
        Debug.Log("Initialised packets.");
    }

    private void InitialiseItemData()
    {
        itemHandlers = new Dictionary<ItemCode, ItemHandler>()
        {
            { ItemCode.NULL_ITEM,  NullItem },
            { ItemCode.SuperFlop, SuperFlop },
            { ItemCode.SuperJump, SuperJump },
            { ItemCode.JellyBomb, JellyBomb }
        };
        Debug.Log("Initialised packets.");
    }

    public BaseTask HandleTask(TaskPickup _task, Player _player)
    {
        return taskHandlers[_task.taskCode](_task, _player);
    }
    public BaseItem HandleItem(ItemPickup _item, Player _player)
    {
        BaseItem ret = itemHandlers[_item.itemCode](_item, _player);
        return ret;
    }


    // -----TASKS-----

    private BaseTask NullTask(TaskPickup _task, Player _player) 
    {
        return null;
    }
    private BaseTask TestTaskEasy(TaskPickup _task, Player _player)
    {
        return new EasyTestTask(_task, _player);
    }
    private BaseTask TestTaskNormal(TaskPickup _task, Player _player)
    {
        return new NormalTestTask(_task, _player);
    }
    private BaseTask TestTaskHard(TaskPickup _task, Player _player)
    {
        return new HardTestTask(_task, _player);
    }
    private BaseTask TestTaskExtreme(TaskPickup _task, Player _player)
    {
        return new DanTask(_task, _player);
    }


    // -----Items-----

    private BaseItem NullItem(ItemPickup _item, Player _player)
    {
        return null;
    }
    private BaseItem SuperFlop(ItemPickup _item, Player _player)
    {
        BaseItem ret = new SuperFlop(_item, _player);
        return ret;
    }
    private BaseItem SuperJump(ItemPickup _item, Player _player)
    {
        BaseItem ret = new SuperJump(_item, _player);
        return ret;
    }
    private BaseItem JellyBomb(ItemPickup _item, Player _player)
    {
        BaseItem ret = new JellyBombItem(_item, _player);
        return ret;
    }
}
