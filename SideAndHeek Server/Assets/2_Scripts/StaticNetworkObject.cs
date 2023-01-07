using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticNetworkObject : NetworkObject
{
    void Start()
    {
        NetworkObjectsManager.instance.RegisterNetworkedObject(this);
    }
}
