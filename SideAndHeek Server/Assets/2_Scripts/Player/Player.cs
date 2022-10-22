using Riptide;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void AppointNewHost()
    {
        if (list.Count > 0)
        {
            Player player = list.ElementAt(0).Value;
            player.isHost = true;

            ServerSend.SetPlayerHost(player);
        }
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.SendSpawned(id);
        }

        Transform spawnpoint = LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(list.Count <= 0);
        //player.transform.rotation = spawnpoint.rotation;

        Player player = Instantiate(GameLogic.Instance.PlayerPrefab, spawnpoint.position, Quaternion.identity);
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        player.activeColour = GameManager.instance.GetNextAvaliableColour();
        player.isHost = list.Count <= 0;

        player.SpawnBody();
        player.SendSpawned();

        list.Add(id, player);

        foreach (PickupSpawner spawner in PickupSpawner.spawners.Values)
        {
            ServerSend.CreatePickupSpawner(spawner.id, spawner.transform.position, id);
        }
    }

    private void SendSpawned()
    {
        NetworkManager.Instance.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }
    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Instance.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
    }
    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddBool(isHost);
        message.AddVector3(transform.position);
        message.AddColour(activeColour);

        return message;
    }

    public bool isHost;
    public bool isReady = false;
    public PlayerType playerType = PlayerType.Default;

    public float inputSpeed = 0;
    private bool[] otherInputs = { false, false, false};
    private Quaternion rotation = Quaternion.identity;

    [HideInInspector] public SimplePlayerController movementController;

    public bool test = false;

    public int itemAmount = 0;
    public int maxItemCount = 100;
    
    //[HideInInspector]
    public List<int> activePlayerCollisionIds = new List<int>();

    public bool isBodyActive = false;
    public bool isAcceptingInput = true;

    [SerializeField] private SimplePlayerController bodyPrefab;
    [SerializeField] private FootCollisionHandler largeGroundColliderPrefab;
    [SerializeField] private Transform feetMidpoint;

    public BasePickup activePickup;

    [HideInInspector] public Color activeColour;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void FixedUpdate()
    {
        if (isBodyActive && isAcceptingInput)
        {
            if (otherInputs[0])
            {
                movementController.OnJump();
            }

            if (otherInputs[1])
            {
                movementController.OnFlopKey(true);
            } else
            {
                movementController.OnFlopKeyUp();
            }

            movementController.isSneaking = otherInputs[2];
            movementController.CustomFixedUpdate(inputSpeed);
            movementController.SetRotation(rotation);
        }

        ServerSend.PlayerPositions(this);
        ServerSend.PlayerRotations(this);
        ServerSend.PlayerState(this);
    }

    public void SetReady(bool _isReady)
    {
        isReady = _isReady;

        ServerSend.PlayerReadyToggled(Id, isReady);
    }

    public void SetInput(float _inputSpeed, bool[] _otherInputs, Quaternion _rotation)
    {
        inputSpeed = _inputSpeed;
        otherInputs = _otherInputs;
        rotation = _rotation;
    }

    public bool AttemptPickupItem()
    {
        if (activePickup != null)
        {
            if (activePickup.pickupSO != null)
            {
                return false;
            }
        }

        return true;
    }

    public void TeleportPlayer(Transform _spawnpoint)
    {
        DespawnPlayer();
        transform.position = _spawnpoint.position;
        ServerSend.PlayerTeleported(Id, _spawnpoint.position);
        SpawnBody();
    }

    public void OnCollisionWithOther(float flopTime, bool turnToHunter)
    {
        if (isBodyActive)
        {
            movementController.OnCollisionWithOther(flopTime);
            if (turnToHunter)
            {
                SetPlayerType(PlayerType.Hunter, false);
                GameManager.instance.CheckForGameOver();
            }
        }
    }

    public void SetPlayerType(PlayerType type, bool isFirstHunter)
    {
        playerType = type;

        float speedMultiplier = 1;

        if (playerType == PlayerType.Hunter)
        {
            switch(GameManager.instance.gameRules.speedBoostType)
            {
                case SpeedBoostType.FirstHunter:
                    if (isFirstHunter)
                    {
                        speedMultiplier = GameManager.instance.gameRules.speedMultiplier;
                    }
                    break;
                case SpeedBoostType.AllHunters:
                    speedMultiplier = GameManager.instance.gameRules.speedMultiplier;
                    break;
            }
        }

        movementController.forwardForceMultipler = speedMultiplier;

        ServerSend.SetPlayerType(Id, playerType, true);
    }

    public void SpawnBody()
    {
        if (!isBodyActive)
        {
            movementController = Instantiate(bodyPrefab, transform);
            movementController.largeGroundCollider = Instantiate(largeGroundColliderPrefab, transform);
            movementController.feetMidpoint = feetMidpoint;
            movementController.SetupBodyCollisionHandlers(this);
            isBodyActive = true;
        }
    }

    public void DespawnPlayer()
    {
        if (isBodyActive)
        {
            isBodyActive = false;
            Destroy(movementController.largeGroundCollider.gameObject);
            Destroy(movementController.gameObject);
        }
    }

    public void PickupSpawned(int code)
    {
        NetworkObjectsManager.instance.pickupHandler.SpawnPickup(Id, code, transform.position, transform.rotation);
    }

    public void PickupPickedUp(PickupSO pickup)
    {
        activePickup = NetworkObjectsManager.instance.pickupHandler.HandlePickup(pickup, this);
    }

    public void PickupUsed()
    {
        if (activePickup != null)
        {
            Debug.Log("Item Used");
            activePickup.PickupUsed();
        }
    }

    public void ItemUseComplete()
    {
        activePickup = null;
        ServerSend.ItemUseComplete(Id);
    }
}

public enum PlayerType
{
    Default = 0,
    Hunter,
    Hider,
    Spectator
}

public enum MaterialType
{
    Default = 0,
    Invisible
}
