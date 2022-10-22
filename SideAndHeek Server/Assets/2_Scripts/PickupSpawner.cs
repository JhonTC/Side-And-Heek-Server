using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public static Dictionary<ushort, PickupSpawner> spawners = new Dictionary<ushort, PickupSpawner>();

    [ReadOnly] public ushort id;
    public bool hasPickup = false;

    public Pickup activePickup;

    private void Start()
    {
        hasPickup = false;

        id = (ushort)spawners.Count;
        spawners.Add(id, this);

        if (NetworkManager.Instance.Server != null)
        {
            ServerSend.CreatePickupSpawner(id, transform.position);
        }

        StartCoroutine(SpawnPickup());
    }

    private void OnDestroy()
    {
        if (spawners.ContainsKey(id))
        {
            spawners.Remove(id);
        }
    }

    private IEnumerator SpawnPickup(int spawnDelay = 10)
    {
        yield return new WaitForSeconds(spawnDelay);

        if (!PickupHandler.HaveAllPickupsBeenSpawned())
        {
            PickupDetails activePickupDetails = GameManager.instance.collection.GetRandomItem();

            while (!PickupHandler.CanPickupCodeBeUsed(activePickupDetails))
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
        activePickup = NetworkObjectsManager.instance.pickupHandler.SpawnPickup(id, code, transform.position, transform.rotation, this);
    }

    public void PickupPickedUp()
    {
        hasPickup = false;
        activePickup = null;

        StartCoroutine(SpawnPickup());
    }
}
