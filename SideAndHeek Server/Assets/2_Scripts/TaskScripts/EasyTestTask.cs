using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyTestTask : BaseTask
{
    Player owner;
    int initialFlopCount = 0;
    int lastFlopCount;
    public EasyTestTask(TaskSO task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateProgress()
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
    public NormalTestTask(TaskSO task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateProgress()
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
    public HardTestTask(TaskSO task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateProgress()
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
    public DanTask(TaskSO task, Player _owner) : base(task)
    {
        owner = _owner;
        initialFlopCount = owner.movementController.flopCount;
        lastFlopCount = initialFlopCount;
    }

    public override void UpdateProgress()
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
