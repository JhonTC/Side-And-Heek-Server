using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool gameStarted = false;
    
    private LevelManager levelManager;
    
    [SerializeField] private int specialSpawnDelay = 20;
    private int specialSpawnCount = 0;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        if (LevelManager.instance.levelType == LevelType.Map)
        {
            // maybe delay this...
            gameStarted = true;
        }

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                if (client.player.playerType != PlayerType.Hunter)
                {
                    client.player.TeleportPlayer(LevelManager.instance.GetNextSpawnpoint(!gameStarted && client.isHost));
                } else
                {
                    StartCoroutine(SpawnSpecial(client.player, specialSpawnDelay));
                }
            }
        }
    }

    private IEnumerator SpawnSpecial(Player _player, int _delay = 60)
    {
        specialSpawnCount = _delay;
        while (specialSpawnCount > 0)
        {
            yield return new WaitForSeconds(1.0f);
            
            ServerSend.SetSpecialCountdown(_player.id, specialSpawnCount, specialSpawnCount > 1);

            specialSpawnCount--;
        }

        _player.TeleportPlayer(LevelManager.instance.GetNextSpawnpoint(true));
    }

    public void TryStartGame(int _fromClient)
    {
        bool areAllPlayersReady = AreAllPlayersReady();
        if (areAllPlayersReady)
        {
            int _randPlayerId = Server.clients.ElementAt(Random.Range(0, Server.GetPlayerCount() - 1)).Value.id;

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    PlayerType _playerType = PlayerType.Default;
                    if (client.player.id == _randPlayerId)
                    {
                        _playerType = PlayerType.Hunter;
                    }
                    else
                    {
                        _playerType = PlayerType.Hider;
                    }
                    client.player.playerType = _playerType;

                    ServerSend.SetPlayerType(client.player);
                }
            }
            
            LevelManager.instance.LoadScene("Map_Legacy", LevelType.Map);
            ServerSend.ChangeScene("Map_Legacy");
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
