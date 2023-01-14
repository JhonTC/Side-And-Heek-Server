using UnityEngine;
using Riptide;
using Riptide.Utils;


public enum ServerToClientId : ushort
{
    welcome = 1,
    playerSpawned,
    playerPosition,
    playerRotation,
    playerState,
    createItemSpawner,
    pickupSpawned,
    pickupPickedUp,
    itemSpawned,
    networkObjectTransform,
    itemUseComplete,
    networkObjectDestroyed,
    playerReadyToggled,
    changeScene,
    unloadScene,
    setPlayerType,
    setSpecialCountdown,
    setPlayerColour,
    setPlayerMaterialType,
    sendErrorResponseCode,
    gameStart,
    gameOver,
    playerTeleported,
    gameRulesChanged,
    setPlayerHost
}

public enum ClientToServerId : ushort
{
    name = 1,
    playerMovement,
    playerReady,
    tryStartGame,
    setPlayerColour,
    pickupSelected,
    itemUsed,
    gameRulesChanged
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;

    public static NetworkManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
                _instance = value;
            else if (_instance != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate");
                Destroy(value);
            }
        }
    }

    public Riptide.Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Riptide.Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Update();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        GameManager.instance.OnPlayerLeft(e.Client.Id);
    }
}

