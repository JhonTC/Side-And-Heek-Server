using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : SpawnableObject
{
    public PickupSpawner spawner;

    public void Init(PickupSpawner _spawner, int _creatorId, PickupType _pickupType, int _code, bool _sendMovement)
    {
        base.Init(_creatorId, _pickupType, _code, true);

        spawner = _spawner;
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

    public void PickupPickedUp(int _byPlayer)
    {
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
        ServerSend.PickupPickedUp(pickupId, _byPlayer, pickupType, code);

        if (PickupManager.pickups.ContainsKey(pickupId))
        {
            PickupManager.pickups.Remove(pickupId);
        }

        if (spawner != null)
        {
            spawner.PickupPickedUp();
        }

        Destroy(gameObject);
    }
}
