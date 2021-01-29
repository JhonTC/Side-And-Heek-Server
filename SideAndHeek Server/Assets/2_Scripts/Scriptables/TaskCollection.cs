using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new task collection", menuName = "Data/Task Collection")]
public class TaskCollection : ScriptableObject
{
    public List<TaskDetails> details;

    public TaskDetails GetTaskByCode(TaskCode code)
    {
        foreach (TaskDetails taskDetails in details)
        {
            if (taskDetails.task.taskCode == code)
            {
                return taskDetails;
            }
        }

        throw new System.Exception($"ERROR: No task exists with code {code}.");
    }

    public TaskDetails GetRandomTask()
    {
        return details[Random.Range(0, details.Count)];
    }
}

[System.Serializable]
public class TaskDetails
{
    public TaskSO task;
    public int numberOfUses;
}
