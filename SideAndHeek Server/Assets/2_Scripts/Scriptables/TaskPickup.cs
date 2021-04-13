using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new task",menuName = "Data/Task")]
public class TaskPickup : PickupSO
{
    public TaskCode taskCode;

    public float maxProgress;
}

public enum TaskCode
{
    NULL_TASK,
    TestTaskEasy,
    TestTaskNormal,
    TestTaskHard,
    TestTaskExtreme
}
