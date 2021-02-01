using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    private delegate BaseTask TaskHandler(TaskSO _task, Player _player);
    private static Dictionary<TaskCode, TaskHandler> taskHandlers;

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
    }

    public BaseTask HandleTask(TaskSO _task, Player _player)
    {
        return taskHandlers[_task.taskCode](_task, _player);
    }

    private BaseTask NullTask(TaskSO _task, Player _player) {
        return null;
    }
    private BaseTask TestTaskEasy(TaskSO _task, Player _player)
    {
        return new EasyTestTask(_task, _player);
    }
    private BaseTask TestTaskNormal(TaskSO _task, Player _player)
    {
        return new NormalTestTask(_task, _player);
    }
    private BaseTask TestTaskHard(TaskSO _task, Player _player)
    {
        return new HardTestTask(_task, _player);
    }
    private BaseTask TestTaskExtreme(TaskSO _task, Player _player)
    {
        return new DanTask(_task, _player);
    }
}
