using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public static GameLogic _instance;

    public static GameLogic Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
                _instance = value;
            else if (_instance != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate");
                Destroy(value);
            }
        }
    }

    public Player PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private Player playerPrefab;

    private void Awake()
    {
        Instance = this;
    }
}
