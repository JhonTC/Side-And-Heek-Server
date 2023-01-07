using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject: MonoBehaviour
{
    public ushort objectId;
    public bool sendTransform = false;
    public NetworkedObjectType networkedObjectType;

    public virtual void Init(ushort _objectId, bool _sendTransform, NetworkedObjectType _networkedObjectType)
    {
        objectId = _objectId;
        sendTransform = _sendTransform;
        networkedObjectType = _networkedObjectType;
    }

    protected virtual void FixedUpdate()
    {
        if (sendTransform)
        {
            ServerSend.NetworkObjectTransform(objectId, transform.position, transform.rotation, transform.localScale);
        }
    }
}
