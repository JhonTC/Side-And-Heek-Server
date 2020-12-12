using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public Player playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        Server.Start(10, 42069);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer(Transform _transform)
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    }
}
