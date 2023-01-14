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

    public GameplayRecorder gameplayRecorder;

    public PickupCollection collection;
    public GameMode gameMode;
    public GameType gameType;

    public string activeSceneName = "Lobby";

    public int currentTime = 0;

    private bool tryStartGameActive = false;

    public Color defaultColour;

    public Color[] hiderColours;
    public Color hunterColour;

    public Dictionary<Color, bool> chosenDefaultColours = new Dictionary<Color, bool>();

    [HideInInspector] public List<Player> lastMainHunterPlayers = new List<Player>(); //todo:Move somewhere?? - cant go to gamemode as it gets cleared when updated

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
        //gameRules = new GameRules();
        gameType = GameType.HideAndSeek;
        gameMode = GameMode.CreateGameModeFromType(gameType);
        gameMode.SetGameRules(GameRules.CreateGameRulesFromType(gameType)); //todo: replace with default mode?

        foreach (Color colour in hiderColours)
        {
            chosenDefaultColours.Add(colour, false);
        }

        gameplayRecorder = new GameplayRecorder();
    }

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            gameMode.FixedUpdate();
        }

        gameplayRecorder.FixedUpdate();
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
            gameMode.GameStart();
        }
    }

    private void OnLevelFinishedUnloading(Scene _scene)
    {
        activeSceneName = "Lobby";

        PickupHandler.ResetPickupLog();

        foreach (Player player in Player.list.Values)
        {
            player.activePickup = null;
        }
    }

    public void TryStartGame(int _fromClient)
    {
        if (!tryStartGameActive)
        {
            if (AreAllPlayersReady())
            {
                gameMode.TryGameStartSuccess();

                tryStartGameActive = true;

                LevelManager.GetLevelManagerForScene(activeSceneName).LoadScene(gameMode.sceneName, LevelType.Map);
                ServerSend.ChangeScene(gameMode.sceneName);
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
        foreach (Player player in Player.list.Values)
        {
            if (!player.isReady)
            {
                return false;
            }
        }

        return true;
    }

    public void CheckForGameOver()
    {
        if (gameMode.CheckForGameOver())
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        gameStarted = false;
        gameMode.GameOver();
        ServerSend.GameOver();

        foreach (Player player in Player.list.Values)
        {
            player.TeleportPlayer(LevelManager.GetLevelManagerForScene("Lobby").GetNextSpawnpoint(!gameStarted && player.isHost));
            player.SetReady(false, false);
            player.SetPlayerType(PlayerType.Default, false, false);
        }

        NetworkObjectsManager.instance.ClearAllSpawnedNetworkObjects();

        SceneManager.UnloadSceneAsync(gameMode.sceneName);
        ServerSend.UnloadScene(gameMode.sceneName);

        tryStartGameActive = false;
    }

    public void GameTypeChanged(GameType _gameType)
    {
        gameType = _gameType;
        gameMode = GameMode.CreateGameModeFromType(gameType);
    }

    public void GameRulesChanged(GameRules _gameRules)
    {
        gameMode.SetGameRules(_gameRules);
    }

    public bool ClaimHiderColour(Color previousColour, Color newColour)
    {
        if (chosenDefaultColours.ContainsKey(newColour))
        {
            if (!chosenDefaultColours[newColour])
            {
                if (chosenDefaultColours.ContainsKey(previousColour))
                {
                    if (chosenDefaultColours[previousColour])
                    {
                        chosenDefaultColours[previousColour] = false;
                    }
                }

                return chosenDefaultColours[newColour] = true;
            }
        }

        return false;
    }

    public void UnclaimHiderColour(Color colour)
    {
        if (chosenDefaultColours.ContainsKey(colour))
        {
            if (chosenDefaultColours[colour])
            {
                chosenDefaultColours[colour] = false;
            }
        }
    }

    public Color GetNextAvaliableColour()
    {
        foreach (Color colour in chosenDefaultColours.Keys)
        {
            if (!chosenDefaultColours[colour])
            {
                chosenDefaultColours[colour] = true;
                return colour;
            }
        }

        throw new System.Exception("ERROR: No colours are left to choose from");
    }

    public void OnPlayerLeft(ushort playerId)
    {
        bool isLeavingPlayerHost = Player.list[playerId].isHost;

        if (gameStarted)
        {
            gameMode.OnPlayerLeft(Player.list[playerId]);
        }

        UnclaimHiderColour(Player.list[playerId].activeColour);

        Player.list[playerId].DespawnPlayer();
        Destroy(Player.list[playerId].gameObject);
        Player.list.Remove(playerId);

        CheckForGameOver();

        if (Player.list.Count > 0)
        {
            if (isLeavingPlayerHost)
            {
                Player.AppointNewHost();
            }
        }
        else
        {
            //Application.Quit();
            Debug.LogWarning("Last Player left, server should close");
        }
    }
}
