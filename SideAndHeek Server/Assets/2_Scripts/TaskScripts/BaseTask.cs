using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseTask
{
    public TaskSO task;
    public float progress;

    public BaseTask(TaskSO _task)
    {
        task = _task;
        progress = 0f;
    }

    public virtual void UpdateProgress() { }

}
