﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : MonoBehaviour
{
    public ushort objectId;
    public ushort creatorId;
    public bool sendMovement;

    public PickupDetails activeItemDetails;

    public void Init(ushort _creatorId, int _code, bool _sendMovement)
    {
        creatorId = _creatorId;
        sendMovement = _sendMovement;

        activeItemDetails = GameManager.instance.collection.GetPickupByCode((PickupCode)_code);
    }

    protected virtual void FixedUpdate()
    {
        if (sendMovement)
        {
            ServerSend.ItemTransform(objectId, transform.position, transform.rotation, transform.localScale);
        }
    }
}
