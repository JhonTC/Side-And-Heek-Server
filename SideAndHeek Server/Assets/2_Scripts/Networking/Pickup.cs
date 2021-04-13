using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : SpawnableObject
{
    public PickupSpawner spawner;

    public void Init(int _pickupId, PickupSpawner _spawner, int _creatorId, int _code)
    {
        base.Init(_pickupId, _creatorId, _code, false);

        spawner = _spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeItemDetails != null && other.CompareTag("BodyCollider"))
        {
            Player _player = other.GetComponentInParent<Player>();
            if (_player.playerType == activeItemDetails.pickupSO.userType || activeItemDetails.pickupSO.userType == PlayerType.Default)
            {
                if (_player.AttemptPickupItem())
                {
                    PickupPickedUp(_player.id);
                }
            }
        }
    }

    public void PickupPickedUp(int _byPlayer)
    {
        int code = 0;
        PickupSO pickup = null;
        if (activeItemDetails != null)
        {
            pickup = activeItemDetails.pickupSO;
            code = (int)activeItemDetails.pickupSO.pickupCode;
        }

        Server.clients[_byPlayer].player.PickupPickedUp(pickup);
        ServerSend.PickupPickedUp(objectId, _byPlayer, code);

        if (PickupHandler.pickups.ContainsKey(objectId))
        {
            PickupHandler.pickups.Remove(objectId);
        }

        if (spawner != null)
        {
            spawner.PickupPickedUp();
        }

        Destroy(gameObject);
    }
}
