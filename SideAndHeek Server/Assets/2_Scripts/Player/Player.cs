using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public bool isReady = false;
    public PlayerType playerType = PlayerType.Default;

    private float moveSpeed = 5f / Constants.TICKS_PER_SEC;

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

    public void Initialize(int _id, string _username, Transform _transform, Color _activeColour)
    {
        id = _id;
        username = _username;
        activeColour = _activeColour;

        SpawnPlayer();

        //controller.TeleportPhysicalBody(_transform.position);
        //movementController.root.rotation = _transform.rotation;
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

        ServerSend.PlayerReadyToggled(id, isReady);
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
        ServerSend.PlayerTeleported(id, _spawnpoint.position);
        SpawnPlayer();
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

        ServerSend.SetPlayerType(id, playerType, true);
    }

    public void SpawnPlayer()
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
        NetworkObjectsManager.instance.pickupHandler.SpawnPickup(id, code, transform.position, transform.rotation);
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
        ServerSend.ItemUseComplete(id);
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
