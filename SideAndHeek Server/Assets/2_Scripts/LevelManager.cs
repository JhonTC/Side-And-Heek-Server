using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Transform specialSpawnpoint;
    public Transform[] spawnpoints;

    public LevelType levelType;
    
    private int spawnpointIndexCounter = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, disabling other object and replacing it with this!");
            //instance.gameObject.SetActive(false);
            instance = this;
        }
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
