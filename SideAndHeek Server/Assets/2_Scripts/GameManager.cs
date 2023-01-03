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
    public GameMode gameMode;
    public GameType gameType;

    public string activeSceneName = "Lobby";

    public int currentTime = 0;

    private bool tryStartGameActive = false;

    public Color defaultColour;

    public Color[] hiderColours;
    public Color hunterColour;

    public Dictionary<Color, bool> chosenHiderColours = new Dictionary<Color, bool>();

    private List<Player> lastMainHunterPlayers = new List<Player>();

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
            chosenHiderColours.Add(colour, false);
        }
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
                Player randomPlayer = GetRandomPlayerExcludingLastHunters();
                lastMainHunterPlayers.Clear();

                foreach (Player player in Player.list.Values)
                {
                    if (player != null)
                    {
                        PlayerType _playerType = PlayerType.Default;
                        if (player.Id == randomPlayer.Id)
                        {
                            _playerType = PlayerType.Hunter;
                            lastMainHunterPlayers.Add(player);
                        }
                        else
                        {
                            _playerType = PlayerType.Hider;
                        }
                        player.SetPlayerType(_playerType, true);
                    }
                }

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

    private Player GetRandomPlayerExcludingLastHunters()
    {
        List<Player> randomPlayers = new List<Player>();
        foreach (Player player in Player.list.Values)
        {
            if (!lastMainHunterPlayers.Contains(player))
            {
                randomPlayers.Add(player);
            }
        }

        return randomPlayers[Random.Range(0, randomPlayers.Count)];
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

        PickupHandler.ClearAllActivePickups();
        ItemHandler.ClearAllActiveItems();

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
        //GAMEMODE CHANGES?
    }

    public bool ClaimHiderColour(Color previousColour, Color newColour)
    {
        if (chosenHiderColours.ContainsKey(newColour))
        {
            if (!chosenHiderColours[newColour])
            {
                if (chosenHiderColours.ContainsKey(previousColour))
                {
                    if (chosenHiderColours[previousColour])
                    {
                        chosenHiderColours[previousColour] = false;
                    }
                }

                return chosenHiderColours[newColour] = true;
            }
        }

        return false;
    }

    public void UnclaimHiderColour(Color colour)
    {
        if (chosenHiderColours.ContainsKey(colour))
        {
            if (chosenHiderColours[colour])
            {
                chosenHiderColours[colour] = false;
            }
        }
    }

    public Color GetNextAvaliableColour()
    {
        foreach (Color colour in chosenHiderColours.Keys)
        {
            if (!chosenHiderColours[colour])
            {
                chosenHiderColours[colour] = true;
                return colour;
            }
        }

        throw new System.Exception("ERROR: No colours are left to choose from");
    }
}
