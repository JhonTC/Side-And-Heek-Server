using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameMode
{
    public string sceneName;
    protected GameRules gameRules;

    public virtual void Init()
    {

    }

    public static GameMode CreateGameModeFromType(GameType _gameType)
    {
        GameMode gameMode = null;
        switch (_gameType)
        {
            case GameType.HideAndSeek:
                gameMode = new GM_HideAndSeek();
                break;
            case GameType.Deathmatch:
                gameMode = new GM_Deathmatch();
                break;
            case GameType.CaptureTheFlag:
                gameMode = new GM_CaptureTheFlag();
                break;
        }
        gameMode.Init();

        return gameMode;
    }

    public GameRules GetGameRules()
    {
        return gameRules;
    }

    public virtual void SetGameRules(GameRules gameRules)
    {

    }

    public virtual void TryGameStartSuccess()
    {

    }

    public virtual void GameStart()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void GameOver()
    {

    }

    public virtual bool CheckForGameOver()
    {
        return false;
    }

    public virtual void AddGameStartMessageValues(ref Message message)
    {

    }
    public virtual void AddGameOverMessageValues(ref Message message)
    {

    }

    public virtual void OnPlayerCollision(Player player, Player other)
    {

    }

    public virtual void OnPlayerTypeSet(Player player, PlayerType playerType, bool isFirstHunter)
    {

    }

    public virtual void OnPlayerHitFallDetector(Player player)
    {

    }

    public virtual void OnSceneLoaded()
    {

    }

    public virtual void OnTeamScore(ushort teamId)
    {
        
    }

    public virtual void OnPlayerLeft(Player player)
    {

    }
}