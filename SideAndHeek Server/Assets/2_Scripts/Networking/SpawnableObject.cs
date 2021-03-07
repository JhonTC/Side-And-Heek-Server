using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : MonoBehaviour
{
    public int pickupId;
    public int creatorId;
    public PickupType pickupType = PickupType.NULL;

    public TaskDetails activeTaskDetails;
    public ItemDetails activeItemDetails;

    public void Init(int _creatorId, PickupType _pickupType, int _code, bool _sendMovement)
    {
        pickupId = PickupManager.pickups.Count + 1;
        creatorId = _creatorId;

        pickupType = _pickupType;

        if (pickupType == PickupType.Task)
        {
            activeTaskDetails = GameManager.instance.collection.GetTaskByCode((TaskCode)_code);
        }
        else if (pickupType == PickupType.Item)
        {
            activeItemDetails = GameManager.instance.collection.GetItemByCode((ItemCode)_code);
        }
    }
}
