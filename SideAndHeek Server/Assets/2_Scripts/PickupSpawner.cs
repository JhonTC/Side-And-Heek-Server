using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public static Dictionary<int, PickupSpawner> spawners = new Dictionary<int, PickupSpawner>();
    public static Dictionary<TaskCode, int> tasksLog = new Dictionary<TaskCode, int>();
    public static Dictionary<ItemCode, int> itemsLog = new Dictionary<ItemCode, int>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasPickup = false;

    public PickupType pickupType = PickupType.NULL;
    public TaskDetails activeTaskDetails;
    public ItemDetails activeItemDetails;

    public int maxSpawnCount = 0;

    private void Start()
    {
        hasPickup = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);

        BasePickup pickup = null;
        if (pickupType == PickupType.Task)
        {
            if (activeTaskDetails != null)
            {
                pickup = activeTaskDetails.task;
            }
        } 
        else if (pickupType == PickupType.Item)
        {
            if (activeItemDetails != null)
            {
                pickup = activeItemDetails.item;
            }
        }

        ServerSend.CreatePickupSpawner(spawnerId, transform.position, pickupType, hasPickup, pickup);

        StartCoroutine(SpawnPickup());
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

    private IEnumerator SpawnPickup(int spawnDelay = 10)
    {
        yield return new WaitForSeconds(spawnDelay);

        if (!HaveAllPickupsOfTypeBeenSpawned(pickupType))
        {
            if (pickupType == PickupType.Task)
            {
                activeTaskDetails = GameManager.instance.collection.GetRandomTask();

                while (!CanTaskCodeBeUsed(activeTaskDetails))
                {
                    activeTaskDetails = GameManager.instance.collection.GetRandomTask();
                }

                if (tasksLog.ContainsKey(activeTaskDetails.task.taskCode))
                {
                    tasksLog[activeTaskDetails.task.taskCode]++;
                }
                else
                {
                    tasksLog.Add(activeTaskDetails.task.taskCode, 1);
                }

                hasPickup = true;

                ServerSend.PickupSpawned(spawnerId, activeTaskDetails.task);
            }
            else if (pickupType == PickupType.Item)
            {
                activeItemDetails = GameManager.instance.collection.GetRandomItem();

                while (!CanItemCodeBeUsed(activeItemDetails))
                {
                    activeItemDetails = GameManager.instance.collection.GetRandomItem();
                }

                if (itemsLog.ContainsKey(activeItemDetails.item.itemCode))
                {
                    itemsLog[activeItemDetails.item.itemCode]++;
                }
                else
                {
                    itemsLog.Add(activeItemDetails.item.itemCode, 1);
                }

                hasPickup = true;

                ServerSend.PickupSpawned(spawnerId, activeItemDetails.item);
            }
        }
    }

    private bool CanTaskCodeBeUsed(TaskDetails taskDetails)
    {
        if (tasksLog.ContainsKey(taskDetails.task.taskCode))
        {
            if (tasksLog[taskDetails.task.taskCode] >= taskDetails.numberOfUses)
            {
                return false;
            } 
        }
            
        return true;
    }

    private bool CanItemCodeBeUsed(ItemDetails itemDetails)
    {
        if (itemsLog.ContainsKey(itemDetails.item.itemCode))
        {
            if (itemsLog[itemDetails.item.itemCode] >= itemDetails.numberOfUses)
            {
                return false;
            }
        }
        
        return true;
    }

    private bool HaveAllPickupsOfTypeBeenSpawned(PickupType pickupType)
    {
        if (pickupType == PickupType.Task)
        {
            if (tasksLog.Count < GameManager.instance.collection.taskDetails.Count)
            {
                return false;
            }

            foreach (TaskCode taskCode in tasksLog.Keys)
            {
                if (tasksLog.ContainsKey(taskCode))
                {
                    if (tasksLog[taskCode] < GameManager.instance.collection.GetTaskByCode(taskCode).numberOfUses)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        else if (pickupType == PickupType.Item)
        {
            if (itemsLog.Count < GameManager.instance.collection.itemDetails.Count)
            {
                return false;
            }

            foreach (ItemCode itemCode in itemsLog.Keys)
            {
                if (itemsLog.ContainsKey(itemCode))
                {
                    if (itemsLog[itemCode] < GameManager.instance.collection.GetItemByCode(itemCode).numberOfUses)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        } 

        return true;
    }

    public void PickupPickedUp(int _byPlayer)
    {
        hasPickup = false;

        int code = 0;
        BasePickup pickup = null;
        if (pickupType == PickupType.Task)
        {
            if (activeTaskDetails != null)
            {
                pickup = activeTaskDetails.task;
                code = (int)activeTaskDetails.task.taskCode;
            }
        }
        else if (pickupType == PickupType.Item)
        {
            if (activeItemDetails != null)
            {
                pickup = activeItemDetails.item;
                code = (int)activeItemDetails.item.itemCode;
            }
        }

        Server.clients[_byPlayer].player.PickupPickedUp(pickup);
        ServerSend.PickupPickedUp(spawnerId, _byPlayer, pickupType, code);

        StartCoroutine(SpawnPickup());
    }
}
