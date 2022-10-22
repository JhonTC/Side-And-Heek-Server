using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : SpawnableObject
{
    public PickupSpawner spawner;

    public void Init(PickupSpawner _spawner, ushort _objectId, ushort _creatorId, int _code)
    {
        base.Init(_objectId, _creatorId, _code, false);

        spawner = _spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeItemDetails != null && other.CompareTag("BodyCollider"))
        {
            Player _player = other.GetComponentInParent<Player>();

            if (_player.playerType == activeItemDetails.pickupSO.userType || activeItemDetails.pickupSO.userType == PlayerType.Default || _player.playerType == PlayerType.Default)
            {
                if (_player.AttemptPickupItem())
                {
                    PickupPickedUp(_player.Id);
                }
            }
        }
    }

    public void PickupPickedUp(ushort _byPlayer)
    {
        int code = 0;
        PickupSO pickup = null;
        if (activeItemDetails != null)
        {
            pickup = activeItemDetails.pickupSO;
            code = (int)activeItemDetails.pickupSO.pickupCode;
        }

        Player.list[_byPlayer].PickupPickedUp(pickup);
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
