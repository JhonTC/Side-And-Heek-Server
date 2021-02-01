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

    private float inputSpeed = 0;
    private bool[] otherInputs = { false, false };
    private Quaternion rotation = Quaternion.identity;

    [HideInInspector] public SimplePlayerController movementController;

    public bool test = false;

    public int itemAmount = 0;
    public int maxItemCount = 100;
    
    //[HideInInspector]
    public List<int> activePlayerCollisionIds = new List<int>();

    public bool isBodyActive = false;

    [SerializeField] private SimplePlayerController bodyPrefab;
    [SerializeField] private FootCollisionHandler largeGroundCollider;
    [SerializeField] private Transform feetMidpoint;

    public List<BaseTask> activeTasks = new List<BaseTask>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Initialize(int _id, string _username, bool _spawnPlayer)
    {
        id = _id;
        username = _username;

        if (_spawnPlayer)
        {
            SpawnPlayer();
        }
    }

    private void Update()
    {
        for (int i = 0; i < activeTasks.Count; i++)
        {
            activeTasks[i].UpdateProgress();
        }
    }

    public void FixedUpdate()
    {
        if (isBodyActive)
        {
            if (otherInputs[0])
            {
                movementController.OnJump();
            }
            if (otherInputs[1])
            {
                movementController.OnFlop();
            }
            movementController.CustomFixedUpdate(inputSpeed);
            movementController.SetRotation(rotation);
        }

        ServerSend.PlayerPositions(this);
        ServerSend.PlayerRotations(this);
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
        if (itemAmount >= maxItemCount)
        {
            return false;
        }

        itemAmount++;
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
                playerType = PlayerType.Hunter;
                ServerSend.SetPlayerType(id, playerType, true);
                GameManager.instance.CheckForGameOver();
            }
        }
    }

    public void SpawnPlayer()
    {
        if (!isBodyActive)
        {
            movementController = Instantiate(bodyPrefab, transform);
            movementController.largeGroundCollider = largeGroundCollider;
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
            Destroy(movementController.gameObject);
        }
    }

    public void ItemPickedUp(TaskSO task)
    {
        activeTasks.Add(TaskManager.instance.HandleTask(task, this));
    }

    public void TaskProgressed(TaskCode code, float progress)
    {
        ServerSend.TaskProgressed(id, code, progress);
    }

    public void TaskComplete(BaseTask task)
    {
        if (activeTasks.Contains(task))
        {
            activeTasks.Remove(task);
        }

        ServerSend.TaskComplete(id, task.task.taskCode);
    }

    private BaseTask GetActiveTaskWithCode(TaskCode code)
    {
        foreach (BaseTask task in activeTasks)
        {
            if (task.task.taskCode == code)
            {
                return task;
            }
        }

        return null;
    }
}

public enum PlayerType
{
    Default = 0,
    Hunter,
    Hider,
    Spectator
}
