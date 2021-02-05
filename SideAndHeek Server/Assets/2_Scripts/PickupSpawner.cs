using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public static Dictionary<int, PickupSpawner> spawners = new Dictionary<int, PickupSpawner>();
    public static Dictionary<TaskDetails, int> tasksLog = new Dictionary<TaskDetails, int>();
    public static Dictionary<ItemDetails, int> itemsLog = new Dictionary<ItemDetails, int>();
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

        if (!HaveAllPickupsOfTypeBeenSpawned(pickupType))
        {
            StartCoroutine(SpawnPickup());
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

    private IEnumerator SpawnPickup(int spawnDelay = 10)
    {
        yield return new WaitForSeconds(spawnDelay);

         //(PickupType)Random.Range(1, 3);

        if (pickupType == PickupType.Task)
        {
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

            if (itemsLog.ContainsKey(activeItemDetails))
            {
                itemsLog[activeItemDetails]++;
            }
            else
            {
                itemsLog.Add(activeItemDetails, 1);
            }

            hasPickup = true;

            ServerSend.PickupSpawned(spawnerId, activeItemDetails.item);
        }
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

    private bool CanItemCodeBeUsed(ItemDetails itemDetails)
    {
        if (itemsLog.ContainsKey(itemDetails))
        {
            if (itemsLog[itemDetails] < itemDetails.numberOfUses)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    private bool HaveAllPickupsOfTypeBeenSpawned(PickupType pickupType)
    {
        if (pickupType == PickupType.Task)
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
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        else if (pickupType == PickupType.Item)
        {
            if (itemsLog.Count == 0)
            {
                return false;
            }

            foreach (ItemDetails itemDetails in itemsLog.Keys)
            {
                if (itemsLog.ContainsKey(itemDetails))
                {
                    if (itemsLog[itemDetails] < itemDetails.numberOfUses)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
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
        ServerSend.ItemPickedUp(spawnerId, _byPlayer, pickupType, code);

        if (!HaveAllPickupsOfTypeBeenSpawned(pickupType))
        {
            StartCoroutine(SpawnPickup());
        }
    }
}
