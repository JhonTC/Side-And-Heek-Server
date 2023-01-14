using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_Deathmatch : GameMode
{
    private GR_Deathmatch CustomGameRules => gameRules as GR_Deathmatch;

    private string spectatorSceneName = "Lobby";
    public Transform shrinkingArea;

    public override void Init()
    {
        sceneName = "Map_2";
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
        GameObject objectSA = GameObject.FindGameObjectWithTag("ShrinkingArea");
        if (objectSA)
        {
            shrinkingArea = objectSA.transform;
        }

        foreach (Player player in Player.list.Values)
        {
            player.TeleportPlayer(LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(false));
        }

        ServerSend.GameStarted();
        GameModeUtils.StartGameTimer(GameManager.instance.GameOver, CustomGameRules.gameLength);
    }

    public override void FixedUpdate()
    {
        if (shrinkingArea.localScale.x >= 0.4f)
        {
            shrinkingArea.localScale -= new Vector3(CustomGameRules.shrinkSpeed, 0, CustomGameRules.shrinkSpeed) * Time.fixedDeltaTime;
        }

        //NEED TO SEND NEW SCALE TO CLIENTS
    }

    public override void GameOver()
    {
        CalculateWinners();
    }

    public override void AddGameStartMessageValues(ref Message message)
    {
        message.AddInt(CustomGameRules.gameLength);
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

            isGameOver = defaultCount <= 1;
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

        if (CheckForGameOver())
        {
            GameManager.instance.GameOver();
        }
    }

    public override void OnPlayerTypeSet(Player player, PlayerType playerType, bool isFirstHunter)
    {

    }
}
