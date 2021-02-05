using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager instance;

    private delegate BaseTask TaskHandler(TaskPickup _task, Player _player);
    private delegate BaseItem ItemHandler(ItemPickup _item, Player _player);

    private static Dictionary<TaskCode, TaskHandler> taskHandlers;
    private static Dictionary<ItemCode, ItemHandler> itemHandlers;

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
            { ItemCode.SuperJump, SuperJump }
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
}
