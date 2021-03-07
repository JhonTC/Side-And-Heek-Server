using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public static Dictionary<int, PickupSpawner> spawners = new Dictionary<int, PickupSpawner>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasPickup = false;
    public PickupType pickupType;
    
    public int maxSpawnCount = 0;

    public Pickup activePickup;

    private void Start()
    {
        hasPickup = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);

        ServerSend.CreatePickupSpawner(spawnerId, transform.position, pickupType);

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

        if (!PickupManager.instance.HaveAllPickupsOfTypeBeenSpawned(pickupType))
        {
            if (pickupType == PickupType.Task)
            {
                TaskDetails activeTaskDetails = GameManager.instance.collection.GetRandomTask();

                while (!PickupManager.instance.CanTaskCodeBeUsed(activeTaskDetails))
                {
                    activeTaskDetails = GameManager.instance.collection.GetRandomTask();
                }

                if (PickupManager.tasksLog.ContainsKey(activeTaskDetails.task.taskCode))
                {
                    PickupManager.tasksLog[activeTaskDetails.task.taskCode]++;
                }
                else
                {
                    PickupManager.tasksLog.Add(activeTaskDetails.task.taskCode, 1);
                }

                hasPickup = true;

                PickupSpawned((int)activeTaskDetails.task.taskCode);
            }
            else if (pickupType == PickupType.Item)
            {
                ItemDetails activeItemDetails = GameManager.instance.collection.GetRandomItem();

                while (!PickupManager.instance.CanItemCodeBeUsed(activeItemDetails))
                {
                    activeItemDetails = GameManager.instance.collection.GetRandomItem();
                }

                if (PickupManager.itemsLog.ContainsKey(activeItemDetails.item.itemCode))
                {
                    PickupManager.itemsLog[activeItemDetails.item.itemCode]++;
                }
                else
                {
                    PickupManager.itemsLog.Add(activeItemDetails.item.itemCode, 1);
                }

                hasPickup = true;

                PickupSpawned((int)activeItemDetails.item.itemCode);
            }
        }
    }

    public void PickupSpawned(int code)
    {
        activePickup = PickupManager.instance.SpawnPickup(pickupType, spawnerId, code, transform.position, transform.rotation, this);
    }

    public void PickupPickedUp()
    {
        hasPickup = false;
        activePickup = null;

        StartCoroutine(SpawnPickup());
    }
}
