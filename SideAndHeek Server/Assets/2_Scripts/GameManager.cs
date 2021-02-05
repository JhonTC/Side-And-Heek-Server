using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool gameStarted = false;
    
    private LevelManager levelManager;

    public PickupCollection collection;

    [SerializeField] private int specialSpawnDelay = 20;
    private int specialSpawnCount = 0;

    public string activeSceneName = "Lobby";

    public int maxPlayDuration = 240;
    public int currentTime = 0;

    public float hunterSpeedMultiplier = 1f;

    private bool tryStartGameActive = false;

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
        SceneManager.sceneUnloaded += OnLevelFinishedUnloading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        SceneManager.sceneUnloaded -= OnLevelFinishedUnloading;
    }

    private void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        activeSceneName = _scene.name;
        SceneManager.SetActiveScene(_scene);

        if (LevelManager.GetLevelManagerForScene(activeSceneName).levelType == LevelType.Map)
        {
            gameStarted = true;
        }

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                if (client.player.playerType != PlayerType.Hunter)
                {
                    client.player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(!gameStarted && client.isHost));
                } 
                else
                {
                    StartCoroutine(SpawnSpecial(client.player, specialSpawnDelay));
                }
            }
        }
    }

    private void OnLevelFinishedUnloading(Scene _scene)
    {
        activeSceneName = "Lobby";
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

        _player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(true));

        tryStartGameActive = false;
        ServerSend.GameStarted(maxPlayDuration);
        StartCoroutine(GameTimeCountdown(maxPlayDuration));
    }

    private IEnumerator GameTimeCountdown(int _delay = 240)
    {
        currentTime = _delay;
        while (currentTime > 0 && gameStarted)
        {
            yield return new WaitForSeconds(1.0f);

            currentTime--;
        }

        if (gameStarted)
        {
            GameOver(false);
        }
    }

    public void TryStartGame(int _fromClient)
    {
        if (!tryStartGameActive)
        {
            bool areAllPlayersReady = AreAllPlayersReady();
            if (areAllPlayersReady)
            {
                int _randPlayerId = Server.clients.ElementAt(Random.Range(0, Server.GetPlayerCount())).Value.id;

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
                        client.player.SetPlayerType(_playerType);
                    }
                }

                tryStartGameActive = true;

                LevelManager.GetLevelManagerForScene(activeSceneName).LoadScene("Map_1", LevelType.Map);
                ServerSend.ChangeScene("Map_1");
            }
            else
            {
                ServerSend.SendErrorResponse(ErrorResponseCode.NotAllPlayersReady); 
                tryStartGameActive = false;
            }
        } else
        {
            ServerSend.SendErrorResponse(ErrorResponseCode.NotAllPlayersReady); //TODO:Change to different message(game already trying to start)
            tryStartGameActive = false;
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

    public void CheckForGameOver()
    {
        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                if (client.player.playerType == PlayerType.Hider)
                {
                    return;
                }
            }
        }

        GameOver(true);
    }

    public void GameOver(bool _isHunterVictory)
    {
        ServerSend.GameOver(_isHunterVictory);

        gameStarted = false;

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            { 
                client.player.playerType = PlayerType.Default;
                ServerSend.SetPlayerType(client.player);
                ServerSend.PlayerReadyReset(client.id, false);

                client.player.TeleportPlayer(LevelManager.GetLevelManagerForScene("Lobby").GetNextSpawnpoint(!gameStarted && client.isHost));
            }
        }

        SceneManager.UnloadSceneAsync("Map_1");
        ServerSend.UnloadScene("Map_1");

        Debug.Log("Game Over, Hunters Win!");
    }
}
