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
    public GameRules gameRules;

    //[SerializeField] private int specialSpawnDelay = 20;
    private int specialSpawnCount = 0;

    public string activeSceneName = "Lobby";
    public string activeHunterSceneName = "Lobby";

    //public int maxPlayDuration = 240;
    public int currentTime = 0;

    //public float hunterSpeedMultiplier = 1f;

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
        }

        foreach (Player player in Player.list.Values)
        {
            if (player.playerType != PlayerType.Hunter)
            {
                player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(!gameStarted && player.isHost));
            }
            else
            {
                StartCoroutine(SpawnSpecial(player, gameRules.hidingTime));
            }
        }
    }

    private void OnLevelFinishedUnloading(Scene _scene)
    {
        activeSceneName = "Lobby";
        activeHunterSceneName = activeSceneName;

        PickupHandler.ResetPickupLog();

        foreach (Player player in Player.list.Values)
        {
            player.activePickup = null;
        }
    }

    private IEnumerator SpawnSpecial(Player _player, int _delay = 60)
    {
        specialSpawnCount = _delay;
        while (specialSpawnCount > 0 && gameStarted)
        {
            yield return new WaitForSeconds(1.0f);
            
            ServerSend.SetSpecialCountdown(_player.Id, specialSpawnCount, specialSpawnCount > 1);

            specialSpawnCount--;
        }

        if (gameStarted)
        {
            activeHunterSceneName = activeSceneName;

            _player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeHunterSceneName).GetNextSpawnpoint(true));

            ServerSend.GameStarted(gameRules.gameLength);
            StartCoroutine(GameTimeCountdown(gameRules.gameLength));
        }
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
        if (gameStarted)
        {
            int hunterCount = 0;
            int hiderCount = 0;

            foreach (Player player in Player.list.Values)
            {
                if (player.playerType == PlayerType.Hider)
                {
                    hiderCount++;
                }
                if (player.playerType == PlayerType.Hunter)
                {
                    hunterCount++;
                }
            }

            bool isGameOver = hiderCount == 0 || hunterCount == 0;

            if (isGameOver)
            {
                bool isHunterVictory = true;
                if (hunterCount == 0)
                {
                    isHunterVictory = false;
                }

                GameOver(isHunterVictory);
            }
        }
    }

    public void GameOver(bool _isHunterVictory)
    {
        ServerSend.GameOver(_isHunterVictory);

        gameStarted = false;

        foreach (Player player in Player.list.Values)
        {
            player.TeleportPlayer(LevelManager.GetLevelManagerForScene("Lobby").GetNextSpawnpoint(!gameStarted && player.isHost));
            player.SetReady(false, false);
            player.SetPlayerType(PlayerType.Default, false, false);
        }

        PickupHandler.ClearAllActivePickups();
        ItemHandler.ClearAllActiveItems();

        SceneManager.UnloadSceneAsync("Map_1");
        ServerSend.UnloadScene("Map_1");

        tryStartGameActive = false;

        Debug.Log($"Game Over, {(_isHunterVictory ? "Hunters" : "Hiders")} Win!");
    }

    public void GameRulesChanged(GameRules _gameRules)
    {
        gameRules = _gameRules;
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
