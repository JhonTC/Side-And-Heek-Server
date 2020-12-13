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

    public SimplePlayerController controller;

    public bool test = false;

    public int itemAmount = 0;
    public int maxItemCount = 100;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Initialize(int _id, string _username, Transform _transform)
    {
        id = _id;
        username = _username;
        controller = GetComponent<SimplePlayerController>();

        //controller.TeleportPhysicalBody(_transform.position);
        controller.rigidbody.rotation = _transform.rotation;
    }

    public void FixedUpdate()
    {
        if (otherInputs[0])
        {
            controller.OnJump();
        }
        if (otherInputs[1])
        {
            controller.OnFlop();
        }
        controller.CustomFixedUpdate(inputSpeed);
        controller.SetRotation(rotation);

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
        controller.TeleportPhysicalBody(transform.position);

        transform.position = _spawnpoint.position;
        controller.rigidbody.rotation = _spawnpoint.rotation;
    }
}

public enum PlayerType
{
    Default = 0,
    Hunter,
    Hider,
    Spectator
}
