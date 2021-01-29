using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public Player playerPrefab;

    [SerializeField] private int maxPlayers = 10;

    [SerializeField] private List<Client> clients;

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
        
        Server.Start(maxPlayers, 42069);

        clients = new List<Client>();
        foreach (Client client in Server.clients.Values)
        {
            clients.Add(client);
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer(Vector3 _position)
    {
        return Instantiate(playerPrefab, _position, Quaternion.identity);
    }
}
