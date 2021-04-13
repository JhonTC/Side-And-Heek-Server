using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public static Dictionary<int, PickupSpawner> spawners = new Dictionary<int, PickupSpawner>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasPickup = false;

    public Pickup activePickup;

    private void Start()
    {
        hasPickup = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);

        ServerSend.CreatePickupSpawner(spawnerId, transform.position);

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

        if (!NetworkObjectsManager.instance.pickupHandler.HaveAllPickupsBeenSpawned())
        {
            PickupDetails activePickupDetails = GameManager.instance.collection.GetRandomItem();

            while (!NetworkObjectsManager.instance.pickupHandler.CanPickupCodeBeUsed(activePickupDetails))
            {
                activePickupDetails = GameManager.instance.collection.GetRandomItem();
            }

            if (PickupHandler.pickupLog.ContainsKey(activePickupDetails.pickupSO.pickupCode))
            {
                PickupHandler.pickupLog[activePickupDetails.pickupSO.pickupCode]++;
            }
            else
            {
                PickupHandler.pickupLog.Add(activePickupDetails.pickupSO.pickupCode, 1);
            }

            hasPickup = true;

            PickupSpawned((int)activePickupDetails.pickupSO.pickupCode);
        }
    }

    public void PickupSpawned(int code)
    {
        activePickup = NetworkObjectsManager.instance.pickupHandler.SpawnPickup(spawnerId, code, transform.position, transform.rotation, this);
    }

    public void PickupPickedUp()
    {
        hasPickup = false;
        activePickup = null;

        StartCoroutine(SpawnPickup());
    }
}
