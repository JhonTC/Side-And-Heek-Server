using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Team
{
    public ushort id;
    public List<Player> members = new List<Player>();
    public int score = 0;

    public Team(ushort _id)
    {
        id = _id;
    }

    public void AddMember(Player player)
    {
        if (!members.Contains(player))
        {
            members.Add(player);
        }
    }

    public void RemoveMember(Player player)
    {
        if (members.Contains(player))
        {
            members.Remove(player);
        }
    }

    public void ChangeScore(int delta)
    {
        score += delta;
    }
}

public class GM_CaptureTheFlag : GameMode
{
    private GR_CaptureTheFlag CustomGameRules => gameRules as GR_CaptureTheFlag;

    public Dictionary<ushort, Team> teams = new Dictionary<ushort, Team>();
    private ushort currentTeamId = 0;

    public Dictionary<string, ushort> disconnectedPlayerTeams; //replace string with UnityID

    public override void Init()
    {
        sceneName = "Map_3";
    }

    public override void SetGameRules(GameRules _gameRules)
    {
        GR_CaptureTheFlag newGameRules = _gameRules as GR_CaptureTheFlag;

        if (newGameRules != null)
        {
            gameRules = newGameRules;
        }
    }

    public override void TryGameStartSuccess()
    {

    }

    public override void GameStart()
    {
        //todo:split players into teams relative to CustomGameRules.numberOfTeams
        int numberOfTeams = CustomGameRules.numberOfTeams;
        if (Player.list.Count < CustomGameRules.numberOfTeams)
        {
            numberOfTeams = Player.list.Count;
        }

        for (int i = 0; i < CustomGameRules.numberOfTeams; i++)
        {
            teams.Add(currentTeamId, new Team(currentTeamId));

            currentTeamId++;
        }

        List<Team> tempTeams = teams.Values.ToList();
        int currentTeamIndex = 0;
        foreach (Player player in Player.list.Values)
        {
            tempTeams[currentTeamIndex].AddMember(player);

            currentTeamIndex++;
            if (currentTeamIndex >= tempTeams.Count)
            {
                currentTeamIndex = 0;
            }
        }

        LevelManager levelManager = LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName);

        foreach (Team team in teams.Values)
        {
            if (team.members.Count > 0)
            {
                Transform spawnpoint = levelManager.GetNextSpawnpoint(false);

                Color teamColour = team.members[Random.Range(0, team.members.Count - 1)].activeColour;

                foreach (Player player in team.members)
                {
                    player.TeleportPlayer(spawnpoint);
                    player.activeColour = teamColour;
                    ServerSend.SetPlayerColour(player.Id, teamColour, false);
                }
            }
        }

        ServerSend.GameStarted();

        if (CustomGameRules.gameEndType == GameEndType.Time)
        GameModeUtils.StartGameTimer(GameManager.instance.GameOver, CustomGameRules.gameLength);
    }

    public override void GameOver()
    {
        CalculateWinners();

        teams.Clear();
        currentTeamId = 0;
    }

    public override bool CheckForGameOver()
    {
        if (GameManager.instance.gameStarted)
        {
            if (CustomGameRules.gameEndType == GameEndType.Score)
            {
                foreach (Team team in teams.Values)
                {
                    if (team.score >= CustomGameRules.maxScore)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void CalculateWinners()
    {
        ushort highestKey = 0;
        bool isFirst = true;
        foreach (Team team in teams.Values)
        {
            if (isFirst || team.score >= teams[highestKey].score)
            {
                highestKey = team.id;

                if (!isFirst)
                {
                    isFirst = false;
                }
            }
        }

        Debug.Log($"Game Over, Team {highestKey} Win!");
    }

    public override void AddGameStartMessageValues(ref Message message)
    {

    }
    public override void AddGameOverMessageValues(ref Message message)
    {

    }

    public override void OnPlayerCollision(Player player, Player other)
    {
        //if player is holding flag, make the drop it
    }

    public override void OnPlayerTypeSet(Player player, PlayerType playerType, bool isFirstHunter)
    {

    }

    public override void OnPlayerHitFallDetector(Player player)
    {
        //respawn at team spawnpoint
    }

    public override void OnSceneLoaded()
    {

    }

    public override void OnTeamScore(ushort teamId)
    {
        if (teams.ContainsKey(teamId))
        {
            teams[teamId].ChangeScore(1);
        }
    }

    public override void OnPlayerLeft(Player player)
    {
        foreach (Team team in teams.Values)
        {
            if (team.members.Contains(player))
            {
                team.RemoveMember(player);
                disconnectedPlayerTeams.Add(player.Username, team.id); //todo ADD PLAYER JOINED DETECTION
            }
        }
    }
}
