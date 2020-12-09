using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public bool isReady = false;

    private float moveSpeed = 5f / Constants.TICKS_PER_SEC;

    private float inputSpeed = 0;
    private bool[] otherInputs = { false, false };
    private Quaternion rotation = Quaternion.identity;

    public SimplePlayerController controller;

    public bool test = false;

    public int itemAmount = 0;
    public int maxItemCount = 100;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        controller = GetComponent<SimplePlayerController>();
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
}
