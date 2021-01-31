using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<TaskDetails, int> tasksLog = new Dictionary<TaskDetails, int>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasItem = false;

    public TaskDetails activeTaskDetails;

    private void Start()
    {
        hasItem = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);
        
        ServerSend.CreateItemSpawner(spawnerId, transform.position, activeTaskDetails);

        if (!HaveAllTasksBeenSpawned())
        {
            StartCoroutine(SpawnItem());
        }
    }

    private void OnDestroy()
    {
        if (spawners.ContainsKey(spawnerId))
        {
            spawners.Remove(spawnerId);
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (hasItem && other.CompareTag("BodyCollider"))
        {
            Player _player = other.GetComponentInParent<Player>();
            if (_player.AttemptPickupItem())
            {
                ItemPickedUp(_player.id);
            }
        }
    }*/

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(10f);

        activeTaskDetails = GameManager.instance.collection.GetRandomTask();

        while (!CanTaskCodeBeUsed(activeTaskDetails))
        {
            activeTaskDetails = GameManager.instance.collection.GetRandomTask();
        }

        if (tasksLog.ContainsKey(activeTaskDetails))
        {
            tasksLog[activeTaskDetails]++;
        }
        else
        {
            tasksLog.Add(activeTaskDetails, 1);
        }

        hasItem = true;

        ServerSend.ItemSpawned(spawnerId, activeTaskDetails.task);
    }

    private bool CanTaskCodeBeUsed(TaskDetails taskDetails)
    {
        if (tasksLog.ContainsKey(taskDetails))
        {
            if (tasksLog[taskDetails] < taskDetails.numberOfUses)
            {
                return true;
            } else
            {
                return false;
            }
        } else
        {
            return true;
        }
    }

    private bool HaveAllTasksBeenSpawned()
    {
        if (tasksLog.Count == 0)
        {
            return false;
        }

        foreach (TaskDetails taskDetails in tasksLog.Keys)
        {
            if (tasksLog.ContainsKey(taskDetails))
            {
                if (tasksLog[taskDetails] < taskDetails.numberOfUses)
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }

        return true;
    }

    public void ItemPickedUp(int _byPlayer)
    {
        hasItem = false;

        Server.clients[_byPlayer].player.ItemPickedUp(activeTaskDetails.task);
        ServerSend.ItemPickedUp(spawnerId, _byPlayer, activeTaskDetails.task.taskCode);

        if (!HaveAllTasksBeenSpawned())
        {
            StartCoroutine(SpawnItem());
        }
    }
}
