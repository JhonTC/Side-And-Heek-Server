using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static Dictionary<string, LevelManager> levelManagers = new Dictionary<string, LevelManager>();

    public Transform specialSpawnpoint;
    public Transform[] spawnpoints;

    public LevelType levelType;
    
    private int spawnpointIndexCounter = 0;

    public string sceneName;

    private void Awake()
    {
        Debug.Log(SceneManager.GetActiveScene().name);
        levelManagers.Add(sceneName, this);
    }

    private void OnDestroy()
    {
        levelManagers.Remove(sceneName);
    }

    public static LevelManager GetLevelManagerForScene(string _sceneName)
    {
        if (levelManagers.ContainsKey(_sceneName))
        {
            return levelManagers[_sceneName];
        }

        return null;
    }

    public void LoadScene(string _sceneName, LevelType type)
    {
        if (SceneManager.GetActiveScene().name != _sceneName)
        {
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);

            this.enabled = false;
        }
    }

    public Transform GetNextSpawnpoint(bool useSpecialSpawn)
    {
        if (useSpecialSpawn)
        {
            return specialSpawnpoint;
        }

        if (spawnpoints.Length != 0)
        {
            if (spawnpoints.Length <= spawnpointIndexCounter)
            {
                spawnpointIndexCounter = 0;
            }

            Transform ret = spawnpoints[spawnpointIndexCounter];
            spawnpointIndexCounter++;

            return ret;
        }

        throw new System.Exception("ERROR: spawnpointIndexCounter is larger of spawnpoints list size");
    }

    
}

public enum LevelType
{
    Lobby,
    Map
}
