using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseTask
{
    public TaskPickup task;
    public float progress;

    public BaseTask(TaskPickup _task)
    {
        task = _task;
        progress = 0f;
    }

    public virtual void UpdateTask() { }
}

public class EasyTestTask : BaseTask
{
    Player owner;
    int initialFlopCount = 0;
    int lastFlopCount;
    public EasyTestTask(TaskPickup task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateTask()
    {
        if (owner.movementController.flopCount != lastFlopCount)
        {
            Debug.Log("PROGRESS IS HAPPENING");

            int progress = owner.movementController.flopCount - initialFlopCount;
            float percentage = Mathf.Clamp01(progress / task.maxProgress);

            owner.TaskProgressed(task.taskCode, percentage);

            if (owner.movementController.flopCount - initialFlopCount >= task.maxProgress)
            {
                owner.TaskComplete(this);
            }

            lastFlopCount = owner.movementController.flopCount;
        }
    }
}

public class NormalTestTask : BaseTask
{
    Player owner;
    int initialFlopCount = 0;
    int lastFlopCount;
    public NormalTestTask(TaskPickup task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateTask()
    {
        if (owner.movementController.flopCount != lastFlopCount)
        {
            Debug.Log("PROGRESS IS HAPPENING");

            int progress = owner.movementController.flopCount - initialFlopCount;
            float percentage = Mathf.Clamp01(progress / task.maxProgress);

            owner.TaskProgressed(task.taskCode, percentage);

            if (owner.movementController.flopCount - initialFlopCount >= task.maxProgress)
            {
                owner.TaskComplete(this);
            }

            lastFlopCount = owner.movementController.flopCount;
        }
    }
}

public class HardTestTask : BaseTask
{
    Player owner;
    int initialFlopCount = 0;
    int lastFlopCount;
    public HardTestTask(TaskPickup task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateTask()
    {
        if (owner.movementController.flopCount != lastFlopCount)
        {
            Debug.Log("PROGRESS IS HAPPENING");

            int progress = owner.movementController.flopCount - initialFlopCount;
            float percentage = Mathf.Clamp01(progress / task.maxProgress);

            owner.TaskProgressed(task.taskCode, percentage);

            if (owner.movementController.flopCount - initialFlopCount >= task.maxProgress)
            {
                owner.TaskComplete(this);
            }

            lastFlopCount = owner.movementController.flopCount;
        }
    }
}

public class DanTask : BaseTask
{
    Player owner;
    int initialFlopCount = 0;
    int lastFlopCount;
    public DanTask(TaskPickup task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateTask()
    {
        if (owner.movementController.flopCount != lastFlopCount)
        {
            int progress = owner.movementController.flopCount - initialFlopCount;
            float percentage = Mathf.Clamp01(progress / task.maxProgress);

            owner.TaskProgressed(task.taskCode, percentage);

            if (owner.movementController.flopCount - initialFlopCount >= task.maxProgress)
            {
                owner.TaskComplete(this);
            }

            lastFlopCount = owner.movementController.flopCount;
        }
    }
}
