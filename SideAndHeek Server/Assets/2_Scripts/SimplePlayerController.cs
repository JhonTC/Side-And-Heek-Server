using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float standingForce;
    [SerializeField] private float jumpingForce;
    [SerializeField] private float walkFootForwardForce;
    [SerializeField] private float sneakFootForwardForce;
    [SerializeField] private float footVerticalForce;
    [SerializeField] private float flopForce;
    [SerializeField] private float footReturnSpeed;
    [SerializeField] private float rootReturnSpeed;

    public bool useGravity = true;
    [HideInInspector] public Rigidbody root;

    [SerializeField] private float turnSpeed;

    public float maxForwardForceMultipler = 1;
    [HideInInspector] public float forwardForceMultipler = 1;
    
    public Rigidbody rightLeg;
    public Rigidbody leftLeg;
    public FootCollisionHandler leftFootCollider;
    public FootCollisionHandler rightFootCollider;
    [HideInInspector] public FootCollisionHandler largeGroundCollider;
    [HideInInspector] public Transform feetMidpoint;

    private Vector3 leftFootInitialDisplacement;
    private Vector3 rightFootInitialDisplacement;
    private Vector3 activeFootDisplacement;
    private Vector3 otherFootDisplacement;

    //private bool isWKeyPressed = false;
    private bool isFlopKeyDown = false;
    private FootCollisionHandler activeWalkingFoot;
    private FootCollisionHandler otherWalkingFoot;

    [SerializeField] private bool isPlayerControlled = false;

    [SerializeField] private BodyCollisionDetection[] bodyCollisionDetectors;

    [HideInInspector] public Player owner;

    private void Start()
    {
        root = GetComponent<Rigidbody>();

        activeWalkingFoot = rightFootCollider;
        otherWalkingFoot = leftFootCollider;
        awfRigidBody = activeWalkingFoot.foot.GetComponent<Rigidbody>();
        owfRigidBody = otherWalkingFoot.foot.GetComponent<Rigidbody>();

        leftFootInitialDisplacement = root.transform.position - leftFootCollider.foot.position;
        rightFootInitialDisplacement = root.transform.position - rightFootCollider.foot.position;
        activeFootDisplacement = leftFootInitialDisplacement;
        otherFootDisplacement = rightFootInitialDisplacement;

        lastRotation = root.rotation;

        jumpForceMultiplier = 1;
        flopForceMultiplier = 1;
        forwardForceMultipler = maxForwardForceMultipler;
    }

    public bool isJumping= false;
    float jumpTimer = 0f;
    float minJumpDuration = 0.5f;
    [HideInInspector] public float jumpForceMultiplier;

    public int jumpCount = 0;
    public void OnJump()
    {
        if (!isFlopping && !isJumping)
        {
            if (largeGroundCollider.isGrounded)
            {
                root.AddForce((Vector3.up * jumpingForce) * jumpForceMultiplier);
                //root.AddForceAtPosition(Vector3.up * jumpingForce / 2, leftFootCollider.foot.position);
                //root.AddForceAtPosition(Vector3.up * jumpingForce / 2, rightFootCollider.foot.position);

                //leftFootCollider.foot.AddForce(Vector3.up * jumpingForce/2);
                //rightFootCollider.foot.AddForce(Vector3.up * jumpingForce/2);

                ToggleActiveWalkingFoot();

                isJumping = true;
                jumpForceMultiplier = 1;
                jumpCount++;
            }
        }
    }

    public bool isFlopping = false;
    float flopTimer = 0f;
    float maxFlopDuration = 3f;
    public float defaultFlopDuration = 3f;
    public bool canKnockOutOthers = false;
    [HideInInspector] public float flopForceMultiplier;
    public bool lastIsFlopping = false;

    public int flopCount = 0;

    public void OnFlopKey(bool flopKeyPressed) { OnFlop(true, false, defaultFlopDuration, flopKeyPressed); }
    public void OnFlop(bool applyFlopForce, bool resetFlop, float duration, bool flopKeyPressed)
    {
        if (flopKeyPressed)
        {
            if (GameManager.instance.gameRules.continuousFlop) {
                isFlopKeyDown = true;
            }
        }

        if (resetFlop)
        {
            flopTimer = 0;
        }

        if (!isFlopping || resetFlop)
        {
            maxFlopDuration = duration;

            if (applyFlopForce/* && largeGroundCollider.isGrounded*/)
            {
                Vector3 forward = root.transform.forward;
                forward.y = 0f;
                root.AddForce((forward * flopForce + Vector3.up * (flopForce / 2f)) * flopForceMultiplier);
                canKnockOutOthers = true;
            }

            ToggleActiveWalkingFoot();

            isFlopping = true;
            flopForceMultiplier = 1;
            flopCount++;
        }

        lastIsFlopping = isFlopping;
    }

    public void OnFlopKeyUp()
    {
        if (isFlopKeyDown)
        {
            isFlopKeyDown = false;
        }
    }

    [HideInInspector] public bool isSneaking = false;

    [SerializeField] private float moveStageDuration;
    float moveStageTimer = 0f;
    WalkStage walkStage = WalkStage.LiftFoot;
    Rigidbody awfRigidBody;
    Rigidbody owfRigidBody;

    public enum WalkStage
    {
        LiftFoot,
        DropFoot
    }

    [SerializeField] private float standingHeight;
    [SerializeField] private LayerMask groundMask;
    public float gravity;
    private Quaternion lastRotation;

    private bool toggleWalkingFoot = false;

    private float inputSpeed = 0;
    private bool isFirst = true;

    public void SetRotation(Quaternion _rotation)
    {
        if (!isFlopping)
        {
            //if (inputSpeed > 0)
            //{
                root.rotation = Quaternion.Lerp(root.rotation, _rotation, Time.fixedDeltaTime * turnSpeed);
                lastRotation = _rotation;
            //}
        }
    }

    public void CustomFixedUpdate(float _inputSpeed)
    {
        inputSpeed = _inputSpeed;

        if (isJumping)
        {
            jumpTimer += Time.fixedDeltaTime;
            if (jumpTimer >= minJumpDuration)
            {
                jumpTimer = 0;
                isJumping = false;
            }
        }
        
        if (!isFlopping)
        {
            Vector3 footCentrePos = leftFootCollider.foot.position + (rightFootCollider.foot.position - leftFootCollider.foot.position) / 2;

            Vector3 rayPosition = root.position;
            Vector3 rayDirection = Vector3.down;
            RaycastHit rayhit;
            if (Physics.Raycast(rayPosition, rayDirection, out rayhit, standingHeight, groundMask))
            {
                root.AddForceAtPosition(Vector3.up * standingForce * 2, rayPosition);
                Debug.DrawRay(rayPosition, rayDirection, Color.green, standingHeight);
            }

            if (inputSpeed > 0 || isJumping)
            {
                isFirst = true;

                if (moveStageTimer < moveStageDuration)
                {
                    Vector3 position = awfRigidBody.transform.position - root.transform.forward * 0.2f;
                    Vector3 force = Vector3.zero;

                    if (walkStage == WalkStage.LiftFoot)
                    {
                        Vector3 forward = root.transform.forward;
                        forward.y = 0;

                        float currentFootForwardForce = walkFootForwardForce;
                        if (isSneaking)
                        {
                            currentFootForwardForce = sneakFootForwardForce;
                        }

                        force = forward * (currentFootForwardForce * forwardForceMultipler * inputSpeed) + Vector3.up * footVerticalForce;

                        otherWalkingFoot.foot.position = Vector3.Lerp(otherWalkingFoot.foot.position, root.transform.position - activeFootDisplacement, footReturnSpeed * Time.fixedDeltaTime);
                    }
                    else if (walkStage == WalkStage.DropFoot)
                    {
                        force = Vector3.up * -footVerticalForce * 1.5f;
                    }

                    if (largeGroundCollider.isGrounded)
                    {
                        //standingHeight = 1.55f;
                        awfRigidBody.AddForceAtPosition(force, position);
                        root.AddForce(force * 0.125f);
                    }
                    else
                    {
                        awfRigidBody.AddForceAtPosition(force * 0.5f, position);
                        root.AddForce(force * 0.125f);
                        //standingHeight = 1.63f;
                    }

                    moveStageTimer += Time.fixedDeltaTime;
                }
                else
                {
                    if (walkStage == WalkStage.LiftFoot)
                    {
                        walkStage = WalkStage.DropFoot;
                    }
                    else if (walkStage == WalkStage.DropFoot)
                    {
                        walkStage = WalkStage.LiftFoot;
                        toggleWalkingFoot = true;
                    }

                    moveStageTimer = 0;
                }
            }
            else
            {   //not walking
                if (isFirst)
                {
                    ToggleActiveWalkingFoot();
                    isFirst = false;
                }

                walkStage = WalkStage.LiftFoot;
                moveStageTimer = 0;

                if (rightFootCollider.isGrounded)
                {
                    Vector3 rightFootPos = footCentrePos;
                    rightFootPos -= root.transform.right * rightFootInitialDisplacement.x;


                    //Vector3 tempRight = root.transform.right * rightFootInitialDisplacement.x;
                    //Vector3 tempForward = root.transform.forward * rightFootInitialDisplacement.z;
                    //Vector3 tempUp = root.transform.up * rightFootInitialDisplacement.y;
                    //Vector3 newPos = root.position - (tempRight + tempUp);
                    
                    float distance = Mathf.Abs(Vector3.Distance(rightFootCollider.foot.position, rightFootPos));

                    //rightFootCollider.foot.position = Vector3.Lerp(rightFootCollider.foot.position, rightFootPos, footReturnSpeed * Time.fixedDeltaTime);
                }

                if (leftFootCollider.isGrounded)
                {
                    Vector3 leftFootPos = footCentrePos;
                    leftFootPos -= root.transform.right * leftFootInitialDisplacement.x;

                    //Vector3 tempRight = root.transform.right * leftFootInitialDisplacement.x;
                    //Vector3 tempForward = root.transform.forward * leftFootInitialDisplacement.z;
                    //Vector3 tempUp = root.transform.up * leftFootInitialDisplacement.y;
                    //Vector3 newPos = root.position - (tempRight + tempUp);

                    float distance = Mathf.Abs(Vector3.Distance(leftFootCollider.foot.position, leftFootPos));

                    //leftFootCollider.foot.position = Vector3.Lerp(leftFootCollider.foot.position, leftFootPos, footReturnSpeed * Time.fixedDeltaTime);
                }

                Vector3 footNewPos = otherWalkingFoot.transform.position;
                footNewPos += root.transform.right * activeFootDisplacement.x;

                //Vector3 feetMidpointPos = leftFootCollider.transform.position + (rightFootCollider.transform.position - leftFootCollider.transform.position) / 2;
                feetMidpoint.position = footNewPos;
                Vector3 rootPos = footNewPos;
                rootPos.y += standingHeight;

                //root.position = Vector3.Lerp(root.position, rootPos, rootReturnSpeed * Time.fixedDeltaTime);
                //root.rotation = Quaternion.Lerp(root.rotation, lastRotation, rootReturnSpeed * 10 * Time.fixedDeltaTime);
            }
        } else
        {
            flopTimer += Time.fixedDeltaTime;
            if (flopTimer >= maxFlopDuration && !isFlopKeyDown)
            {
                flopTimer = 0;
                isFlopping = false;
            }
        }

        if (useGravity)
        {
            float tempGravity = gravity;
            if (inputSpeed > 0 || isFlopping || isJumping)
            {
                tempGravity *= 0.5f;
            }

            Vector3 rightDisplacement = root.position - (root.transform.right * leftFootInitialDisplacement.x);
            Vector3 forcePosition = new Vector3(rightDisplacement.x, leftFootCollider.foot.position.y, rightDisplacement.z);
            leftFootCollider.foot.AddForceAtPosition(Vector3.down * tempGravity * Time.fixedDeltaTime, forcePosition);

            rightDisplacement = root.position - (root.transform.right * rightFootInitialDisplacement.x);
            forcePosition = new Vector3(rightDisplacement.x, rightFootCollider.foot.position.y, rightDisplacement.z);
            rightFootCollider.foot.AddForceAtPosition(Vector3.down * tempGravity * Time.fixedDeltaTime, forcePosition);
        }

        if (toggleWalkingFoot && largeGroundCollider.isGrounded)
        {
            ToggleActiveWalkingFoot();
            toggleWalkingFoot = false;
        }

        largeGroundCollider.transform.position = otherWalkingFoot.transform.position;
    }

    private bool IsGrounded()
    {
        if (!leftFootCollider.isGrounded)
        {
            return false;
        }

        if (!rightFootCollider.isGrounded)
        {
            return false;
        }

        return true;
    }

    private void ToggleActiveWalkingFoot()
    {
        owner.footCollided = true;

        if (activeWalkingFoot == rightFootCollider)
        {
            activeWalkingFoot = leftFootCollider;
            otherWalkingFoot = rightFootCollider;
            activeFootDisplacement = rightFootInitialDisplacement;
            otherFootDisplacement = leftFootInitialDisplacement;
        }
        else if (activeWalkingFoot == leftFootCollider)
        {
            activeWalkingFoot = rightFootCollider;
            otherWalkingFoot = leftFootCollider;
            activeFootDisplacement = leftFootInitialDisplacement;
            otherFootDisplacement = rightFootInitialDisplacement;
        }
        else
        {
            throw new Exception("ERROR: ActiveWalkingFoot not assigned to either left or right foot");
        }

        awfRigidBody = activeWalkingFoot.foot.GetComponent<Rigidbody>();
        owfRigidBody = otherWalkingFoot.foot.GetComponent<Rigidbody>();
    }
    
    public void OnCollisionWithOther(float flopTime)
    {
        OnFlop(false, true, flopTime, false);
    }

    public void SetupBodyCollisionHandlers(Player owner)
    {
        foreach (BodyCollisionDetection bcd in bodyCollisionDetectors)
        {
            bcd.player = owner;
        }
    }
}