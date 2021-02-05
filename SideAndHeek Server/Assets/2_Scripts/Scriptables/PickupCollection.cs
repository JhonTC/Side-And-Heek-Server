using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new pickup collection", menuName = "Data/Pickup Collection")]
public class PickupCollection : ScriptableObject
{
    public List<TaskDetails> taskDetails;
    public List<ItemDetails> itemDetails;

    public TaskDetails GetTaskByCode(TaskCode code)
    {
        foreach (TaskDetails taskDetails in taskDetails)
        {
            TaskPickup taskPickup = taskDetails.task as TaskPickup;

            if (taskPickup.taskCode == code)
            {
                return taskDetails;
            }
        }

        throw new System.Exception($"ERROR: No task exists with code {code}.");
    }

    public ItemDetails GetItemByCode(ItemCode code)
    {
        foreach (ItemDetails itemDetails in itemDetails)
        {
            ItemPickup itemPickup = itemDetails.item as ItemPickup;

            if (itemPickup.itemCode == code)
            {
                return itemDetails;
            }
        }

        throw new System.Exception($"ERROR: No task exists with code {code}.");
    }

    public TaskDetails GetRandomTask()
    {
        return taskDetails[Random.Range(0, taskDetails.Count)];
    }

    public ItemDetails GetRandomItem()
    {
        return itemDetails[Random.Range(0, itemDetails.Count)];
    }
}

[System.Serializable]
public class TaskDetails
{
    public TaskPickup task;
    public int numberOfUses;
}

[System.Serializable]
public class ItemDetails
{
    public ItemPickup item;
    public int numberOfUses;
}




