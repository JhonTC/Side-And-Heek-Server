using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NetworkedObjectType
{
    Static,
    Pickup,
    Item
}

public class NetworkObjectsManager : MonoBehaviour
{
    public static NetworkObjectsManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

        pickupHandler = new PickupHandler(); //move to GameManager?
        itemHandler = new ItemHandler();     //move to GameManager?
    }

    [Serializable]
    public struct ItemDetails
    {
        public SpawnableObject prefab;
        public PickupType pickupType;
    }

    public static Dictionary<ushort, NetworkObject> networkObjects = new Dictionary<ushort, NetworkObject>();
    private static ushort currentObjectId = 0;

    [SerializeField] private Pickup pickupPrefab;
    [SerializeField] private List<ItemDetails> itemPrefabs;

    [HideInInspector] public PickupHandler pickupHandler;
    [HideInInspector] public ItemHandler itemHandler;

    public NetworkObject SpawnObject(NetworkedObjectType networkObjectType, Vector3 position, Quaternion rotation, bool sendTransform, PickupType pickupType = PickupType.NULL)
    {
        NetworkObject prefab = GetPrefabForType(networkObjectType, pickupType);
        if (prefab == null) return null;

        NetworkObject newObject = Instantiate(prefab, position, rotation);
        newObject.Init(currentObjectId, sendTransform, networkObjectType);

        RegisterNetworkedObject(newObject);

        currentObjectId++;

        return newObject;
    }

    public void DestroyObject(NetworkObject networkObject)
    {
        if (networkObjects.ContainsKey(networkObject.objectId))
        {
            UnregisterNetworkedObject(networkObject);
        }

        ServerSend.NetworkObjectDestroyed(networkObject.objectId);
        Destroy(networkObject.gameObject);
    }

    public void RegisterNetworkedObject(NetworkObject networkObject)
    {
        if (!networkObjects.ContainsKey(networkObject.objectId))
        {
            networkObjects.Add(networkObject.objectId, networkObject);
        } else
        {
            Debug.LogWarning($"NetworkObject with id: {networkObject.objectId} already exists");
        }
    }

    public void UnregisterNetworkedObject(NetworkObject networkObject)
    {
        if (networkObjects.ContainsKey(networkObject.objectId))
        {
            networkObjects.Remove(networkObject.objectId);
        }
        else
        {
            Debug.LogWarning($"NetworkObject with id: {networkObject.objectId} doesn't exist");
        }
    }

    public void ClearAllSpawnedNetworkObjects()
    {
        var networkObjectsToDelete = networkObjects.Where(T => T.Value.networkedObjectType != NetworkedObjectType.Static).ToArray();

        foreach (var networkObject in networkObjectsToDelete)
        {
            if (networkObject.Value.networkedObjectType != NetworkedObjectType.Static)
            {
                DestroyObject(networkObject.Value);
            }
        }
    }

    private NetworkObject GetPrefabForType(NetworkedObjectType networkObjectType, PickupType pickupType = PickupType.NULL)
    {
        switch (networkObjectType)
        {
            case NetworkedObjectType.Pickup:
                return pickupPrefab;
            case NetworkedObjectType.Item:
                return GetSpawnableObjectForPickupCode(pickupType);
        }

        return null;
    }

    private NetworkObject GetSpawnableObjectForPickupCode(PickupType _pickupCode)
    {
        foreach (ItemDetails itemDetails in itemPrefabs)
        {
            if (itemDetails.pickupType == _pickupCode)
            {
                return itemDetails.prefab;
            }
        }

        throw new Exception($"No spawnable Object with code {_pickupCode}");
    }

    public void PerformSecondsCountdown(int duration, Action callback) //todo: move to static class?
    {
        StartCoroutine(PerformCountdown(duration, 1, callback));
    }

    IEnumerator PerformCountdown(float duration, float increment, Action callback) //todo: move to static class?
    {
        float countdownValue = 0;
        while (countdownValue < duration)
        {
            yield return new WaitForSeconds(increment);

            countdownValue += increment;
        }

        callback?.Invoke();
    }
}
