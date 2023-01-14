using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GM_HideAndSeek : GameMode
{
    private GR_HideAndSeek CustomGameRules => gameRules as GR_HideAndSeek;

    private int specialSpawnCount = 0;
    private bool isHunterVictory = false;
    private string activeHunterSceneName = "Lobby";

    public override void Init()
    {
        sceneName = "Map_1";
    }

    public override void SetGameRules(GameRules _gameRules)
    {
        GR_HideAndSeek newGameRules = _gameRules as GR_HideAndSeek;

        if (newGameRules != null)
        {
            gameRules = newGameRules;
        }
    }

    private Player GetRandomPlayerAvoidingLastHunters()
    {
        List<Player> randomPlayers = Player.list.Values.Where(p => !GameManager.instance.lastMainHunterPlayers.Contains(p)).ToList();

        if (randomPlayers.Count <= 0)
        {
            randomPlayers = Player.list.Values.ToList();
        }

        return randomPlayers[Random.Range(0, randomPlayers.Count)];
    }

    public override void TryGameStartSuccess()
    {
        Player randomPlayer = GetRandomPlayerAvoidingLastHunters();
        GameManager.instance.lastMainHunterPlayers.Clear();

        foreach (Player player in Player.list.Values)
        {
            if (player != null)
            {
                PlayerType _playerType = PlayerType.Default;
                if (player.Id == randomPlayer.Id)
                {
                    _playerType = PlayerType.Hunter;
                    GameManager.instance.lastMainHunterPlayers.Add(player);
                }
                else
                {
                    _playerType = PlayerType.Hider;
                }
                player.SetPlayerType(_playerType, true);
            }
        }
    }

    public override void GameStart()
    {
        foreach (Player player in Player.list.Values)
        {
            if (player.playerType != PlayerType.Hunter)
            {
                player.TeleportPlayer(LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(!GameManager.instance.gameStarted && player.isHost));
            }
            else
            {
                GameManager.instance.StartCoroutine(SpawnSpecial(player, CustomGameRules.hidingTime));
            }
        }
    }

    public override void GameOver()
    {
        CalculateWinners();
        activeHunterSceneName = "Lobby";
        Debug.Log($"Game Over, {(isHunterVictory ? "Hunters" : "Hiders")} Win!");
    }

    public override void AddGameStartMessageValues(ref Message message)
    {
        message.AddInt(CustomGameRules.gameLength);
    }

    public override void AddGameOverMessageValues(ref Message message)
    {
        message.AddBool(isHunterVictory);
    }

    public override bool CheckForGameOver()
    {
        bool isGameOver = false;

        if (GameManager.instance.gameStarted)
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

            isGameOver = hiderCount == 0 || hunterCount == 0;
        }

        return isGameOver;
    }

    private void CalculateWinners()
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

        isHunterVictory = true;
        if (hunterCount == 0)
        {
            isHunterVictory = false;
        }
    }

    public override void OnPlayerCollision(Player player, Player other)
    {
        if (other.playerType == player.playerType)
        {
            if (player.movementController.canKnockOutOthers)
            {
                if (other.isBodyActive)
                {
                    other.movementController.OnCollisionWithOther(3f);
                }
            }
        }
        else if (player.playerType == PlayerType.Hunter)
        {
            if (!player.activePlayerCollisionIds.Contains(other.Id))
            {
                player.activePlayerCollisionIds.Add(other.Id);

                if (CustomGameRules.catchType == CatchType.OnTouch || (CustomGameRules.catchType == CatchType.OnFlop && player.movementController.canKnockOutOthers))
                {
                    if (other.isBodyActive)
                    {
                        other.movementController.OnCollisionWithOther(3f);
                        other.SetPlayerType(PlayerType.Hunter);

                        if (CheckForGameOver())
                        {
                            GameManager.instance.GameOver();
                        }
                    }
                }
            }
        }
    }

    public override void OnPlayerHitFallDetector(Player player)
    {
        string activeSceneName = GameManager.instance.activeSceneName;
        if (player.playerType == PlayerType.Hunter)
        {
            activeSceneName = activeHunterSceneName;
        }
        else
        {
            if (CustomGameRules.fallRespawnType == HiderFallRespawnType.Hunter)
            {
                player.SetPlayerType(PlayerType.Hunter);
            }
        }

        player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(CustomGameRules.fallRespawnLocation == FallRespawnLocation.Centre));
    }

    public override void OnPlayerTypeSet(Player player, PlayerType playerType, bool isFirstHunter)
    {
        float speedMultiplier = 1;

        if (playerType == PlayerType.Hunter)
        {
            switch (CustomGameRules.speedBoostType)
            {
                case SpeedBoostType.FirstHunter:
                    if (isFirstHunter)
                    {
                        speedMultiplier = CustomGameRules.speedMultiplier;
                    }
                    break;
                case SpeedBoostType.AllHunters:
                    speedMultiplier = CustomGameRules.speedMultiplier;
                    break;
            }
        }

        player.movementController.forwardForceMultipler = speedMultiplier;
    }

    private IEnumerator SpawnSpecial(Player _player, int _delay = 60)
    {
        specialSpawnCount = _delay;
        while (specialSpawnCount > 0 && GameManager.instance.gameStarted)
        {
            yield return new WaitForSeconds(1.0f);

            ServerSend.SetSpecialCountdown(_player.Id, specialSpawnCount, specialSpawnCount > 1);

            specialSpawnCount--;
        }

        if (GameManager.instance.gameStarted)
        {
            activeHunterSceneName = GameManager.instance.activeSceneName;

            _player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeHunterSceneName).GetNextSpawnpoint(true));

            ServerSend.GameStarted();
            GameModeUtils.StartGameTimer(GameManager.instance.GameOver, CustomGameRules.gameLength);
        }
    }
}
