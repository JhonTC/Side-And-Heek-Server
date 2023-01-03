using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_Deathmatch : GameMode
{
    public GR_Deathmatch gameRules;
    private string spectatorSceneName = "Lobby";

    public override void Init()
    {
        sceneName = "Map_1";
    }

    public override GameRules GetGameRules()
    {
        return gameRules;
    }

    public override void SetGameRules(GameRules _gameRules)
    {
        GR_Deathmatch newGameRules = _gameRules as GR_Deathmatch;

        if (newGameRules != null)
        {
            gameRules = newGameRules;
        }
    }

    public override void GameStart()
    {
        foreach (Player player in Player.list.Values)
        {
            player.TeleportPlayer(LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(false));
        }

        ServerSend.GameStarted();
        GameModeUtils.StartGameTimer(GameManager.instance.GameOver, gameRules.gameLength);
    }

    public override void GameOver()
    {
        CalculateWinners();
    }

    public override void AddGameStartMessageValues(ref Message message)
    {
        message.AddInt(gameRules.gameLength);
    }

    public override void AddGameOverMessageValues(ref Message message)
    {

    }

    public override bool CheckForGameOver()
    {
        bool isGameOver = false;

        if (GameManager.instance.gameStarted)
        {
            int defaultCount = 0;

            foreach (Player player in Player.list.Values)
            {
                if (player.playerType == PlayerType.Default)
                {
                    defaultCount++;
                }
            }

            isGameOver = defaultCount == 1;
        }

        return isGameOver;
    }

    private void CalculateWinners()
    {
        //todo: find remaining player

        foreach (Player player in Player.list.Values)
        {
            if (player.playerType == PlayerType.Default)
            {
                Debug.Log($"Game Over, {player.Username} Wins!");
                break;
            }
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
    }

    public override void OnPlayerHitFallDetector(Player player)
    {
        player.SetPlayerType(PlayerType.Spectator);
        player.TeleportPlayer(LevelManager.GetLevelManagerForScene(spectatorSceneName).GetNextSpawnpoint(true));
    }

    public override void OnPlayerTypeSet(Player player, PlayerType playerType, bool isFirstHunter)
    {

    }
}
