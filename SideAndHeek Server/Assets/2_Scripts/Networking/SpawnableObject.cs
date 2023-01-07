using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : NetworkObject
{
    [ReadOnly] public ushort creatorId;

    public PickupDetails activeItemDetails;

    public virtual void Init(ushort _creatorId, int _code)
    {
        creatorId = _creatorId;

        activeItemDetails = GameManager.instance.collection.GetPickupByCode((PickupType)_code);
    }
}
