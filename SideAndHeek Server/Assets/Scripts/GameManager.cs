using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public List<Transform> spawnpoints = new List<Transform>();
    private int spawnpointIndexCounter = 1;

    public bool gameStarted = false;

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
        LoadSpawnpointsForScene(true);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    private void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        spawnpoints = LoadSpawnpointsForScene(_scene.name.Contains("Lobby"));
        if (_scene.name.Contains("Lobby"))
        {
            spawnpointIndexCounter = 0;
        } else
        {
            spawnpointIndexCounter = 0; //change to 1 and add code for seeker to spawn separatley at spawnpoints[0]
            gameStarted = true;
        }

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                Transform _spawnpoint = GetNextSpawnpoint();

                client.player.TeleportPlayer(_spawnpoint); 
            }
        }
    }

    public Transform GetNextSpawnpoint()
    {
        if (spawnpoints.Count != 0)
        {
            if (spawnpoints.Count <= spawnpointIndexCounter)
            {
                spawnpointIndexCounter = 0;
            }

            Transform ret = spawnpoints[spawnpointIndexCounter];
            spawnpointIndexCounter++;

            return ret;
        }

        throw new System.Exception("ERROR: spawnpointIndexCounter is larger of spawnpoints list size");
    }

    private List<Transform> LoadSpawnpointsForScene(bool isLobby)
    {
        string spawnpointTag = "Spawnpoints";
        if (isLobby)
        {
            spawnpointTag = "Lobby" + spawnpointTag;
        } else
        {
            spawnpointTag = "Map" + spawnpointTag;
        }

        List<Transform> newSpawnpoints = new List<Transform>();
        GameObject spawnpointParent = GameObject.FindGameObjectWithTag(spawnpointTag);
        if (spawnpointParent)
        {
            for (int i = 0; i < spawnpointParent.transform.childCount; i++)
            {
                newSpawnpoints.Add(spawnpointParent.transform.GetChild(i));
            }
        }

        return newSpawnpoints;
    }

    public void TryStartGame(int _fromClient)
    {
        bool areAllPlayersReady = AreAllPlayersReady();
        if (areAllPlayersReady)
        {
            LoadScene("Map_Legacy");
            ServerSend.ChangeScene("Map_Legacy");

            int _randPlayerId = Server.clients.ElementAt(Random.Range(0, Server.GetPlayerCount() - 1)).Value.id;

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    PlayerType _playerType = PlayerType.Default;
                    if (client.id == _randPlayerId)
                    {
                        _playerType = PlayerType.Hunter;
                    }
                    else
                    {
                        _playerType = PlayerType.Hider;
                    }

                    ServerSend.SetPlayerType(client.id, _playerType);
                }
            }
        } else
        {
            ServerSend.SetPlayerType(_fromClient);
        }
    }

    private bool AreAllPlayersReady()
    {
        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                if (!client.player.isReady)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
