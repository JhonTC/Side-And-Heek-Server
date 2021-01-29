using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new task",menuName = "Data/Task")]
public class TaskSO : ScriptableObject
{
    public TaskCode taskCode;

    public string taskName;
    public string taskContent;
    public Sprite taskIcon;
    public TaskDifficaulty taskDifficulty;
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
