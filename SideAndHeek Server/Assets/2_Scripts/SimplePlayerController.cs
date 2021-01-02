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
    [SerializeField] private float maxFootForwardForce;
    [SerializeField] private float footVerticalForce;
    [SerializeField] private float flopForce;
    [SerializeField] private float footReturnSpeed;
    [HideInInspector] public Rigidbody root;

    [SerializeField] private float turnSpeed;
    
    public Transform rightLeg;
    public Transform leftLeg;
    public FootCollisionHandler leftFootCollider;
    public FootCollisionHandler rightFootCollider;
    [HideInInspector] public FootCollisionHandler largeGroundCollider;

    private Vector3 leftFootInitialDisplacement;
    private Vector3 rightFootInitialDisplacement;
    private Vector3 activeFootDisplacement;

    //private bool isWKeyPressed = false;
    private bool isFlopKeyPressed = false;
    private FootCollisionHandler activeWalkingFoot;
    private FootCollisionHandler otherWalkingFoot;

    [SerializeField] private bool isPlayerControlled = false;

    [SerializeField] private BodyCollisionDetection[] bodyCollisionDetectors;

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
    }

    bool isJumping= false;
    float jumpTimer = 0f;
    float minJumpDuration = 0.5f;
    public void OnJump()
    {
        if (!isFlopping && !isJumping)
        {
            if (largeGroundCollider.isGrounded)
            {
                leftFootCollider.foot.AddForce(Vector3.up * jumpingForce/2);
                rightFootCollider.foot.AddForce(Vector3.up * jumpingForce/2);

                isJumping = true;
            }
        }
    }

    bool isFlopping = false;
    float flopTimer = 0f;
    float maxFlopDuration = 3f;
    public float defaultFlopDuration = 3f;
    public bool canKnockOutOthers = false;

    public void OnFlop() { OnFlop(true, false, defaultFlopDuration); }
    public void OnFlop(bool applyFlopForce, bool resetFlop, float duration)
    {
        if (resetFlop)
        {
            flopTimer = 0;
        }

        if (!isFlopping || resetFlop)
        {
            maxFlopDuration = duration;

            if (applyFlopForce && largeGroundCollider.isGrounded)
            {
                Vector3 forward = root.transform.forward;
                forward.y = 0f;
                root.AddForce(forward * flopForce + Vector3.up * (flopForce / 2f));
                canKnockOutOthers = true;
            }

            isFlopping = true;
        }
    }

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
    [SerializeField] private float gravity;

    private bool toggleWalkingFoot = false;

    public void SetRotation(Quaternion _rotation)
    {
        if (!isFlopping)
        {
            root.rotation = Quaternion.Lerp(root.rotation, _rotation, Time.fixedDeltaTime * turnSpeed);
        }
    }

    public void CustomFixedUpdate(float inputSpeed)
    {
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

            Vector3 rayPosition = new Vector3(leftFootCollider.foot.position.x, root.position.y, leftFootCollider.foot.position.z);
            Vector3 rayDirection = Vector3.down;
            RaycastHit rayhit;
            if (Physics.Raycast(rayPosition, rayDirection, out rayhit, standingHeight, groundMask))
            {
                /*Vector3 footLocalPos = leftFootCollider.foot.transform.localPosition;
                Vector3 temp = rigidbody.transform.right * (leftFootCollider.foot.transform.parent.localPosition.x + footLocalPos.x);
                Vector3 temp2 = rigidbody.transform.forward * (leftFootCollider.foot.transform.parent.localPosition.z + footLocalPos.z);
                Vector3 forcePosition = new Vector3(rigidbody.position.x, leftFootCollider.foot.position.y, rigidbody.position.z) - temp2 / 4;

                footCentrePos.y = standingHeight;

                Vector3 forceDirection = rigidbody.position - forcePosition;*/

                root.AddForceAtPosition(Vector3.up * standingForce, root.position);
                Debug.DrawRay(root.position, rayDirection, Color.red, standingHeight);
            }

            rayPosition = new Vector3(rightFootCollider.foot.position.x, root.position.y, rightFootCollider.foot.position.z); // if rigidbody is tiled in the z, this position won't account for that!
            if (Physics.Raycast(rayPosition, rayDirection, out rayhit, standingHeight, groundMask))
            {
                float distance = Vector2.Distance(new Vector2(root.position.x, root.position.z), new Vector2(rightFootCollider.foot.position.x, rightFootCollider.foot.position.z));

                /*Vector3 footLocalPos = rightFootCollider.foot.transform.localPosition;
                Vector3 temp = rigidbody.transform.right * (rightFootCollider.foot.transform.parent.localPosition.x + footLocalPos.x);
                Vector3 temp2 = rigidbody.transform.forward * (rightFootCollider.foot.transform.parent.localPosition.z + footLocalPos.z);
                Vector3 forcePosition = new Vector3(rigidbody.position.x, rightFootCollider.foot.position.y, rigidbody.position.z) - temp2 / 4;

                footCentrePos.y = standingHeight;

                Vector3 forceDirection = rigidbody.position - forcePosition;*/
                
                root.AddForceAtPosition(Vector3.up * standingForce, root.position);
                Debug.DrawRay(root.position, rayDirection, Color.green, standingHeight);
            }

            if (inputSpeed > 0 || isJumping)
            {
                if (moveStageTimer < moveStageDuration)
                {
                    Vector3 position = awfRigidBody.transform.position - root.transform.forward * 0.2f;
                    Vector3 force = Vector3.zero;

                    if (walkStage == WalkStage.LiftFoot)
                    {
                        Vector3 forward = root.transform.forward;
                        forward.y = 0;

                        force = forward * (maxFootForwardForce * inputSpeed) + Vector3.up * footVerticalForce;

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
                    }
                    else
                    {
                        awfRigidBody.AddForceAtPosition(force * 0.5f, position);
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
                walkStage = WalkStage.LiftFoot;
                moveStageTimer = 0;
                
                if (rightFootCollider.isGrounded)
                {
                    Vector3 tempRight = root.transform.right * rightFootInitialDisplacement.x;
                    Vector3 tempForward = root.transform.forward * rightFootInitialDisplacement.z;
                    Vector3 tempUp = root.transform.up * rightFootInitialDisplacement.y;
                    Vector3 newPos = root.position - (tempRight + tempForward + tempUp);
                    //rightFootCollider.foot.position = Vector3.Lerp(rightFootCollider.foot.position, newPos, footReturnSpeed * Time.fixedDeltaTime);
                }

                if (leftFootCollider.isGrounded)
                {
                    Vector3 tempRight = root.transform.right * leftFootInitialDisplacement.x;
                    Vector3 tempForward = root.transform.forward * leftFootInitialDisplacement.z;
                    Vector3 tempUp = root.transform.up * leftFootInitialDisplacement.y;
                    Vector3 newPos = root.position - (tempRight + tempForward + tempUp);
                    //leftFootCollider.foot.position = Vector3.Lerp(leftFootCollider.foot.position, newPos, footReturnSpeed * Time.fixedDeltaTime);
                }
            }
        } else
        {
            flopTimer += Time.fixedDeltaTime;
            if (flopTimer >= maxFlopDuration)
            {
                flopTimer = 0;
                isFlopping = false;
            }
        }
        

        Vector3 rightDisplacement = root.position - (root.transform.right * leftFootInitialDisplacement.x);
        Vector3 forcePosition = new Vector3(rightDisplacement.x, leftFootCollider.foot.position.y, rightDisplacement.z);
        leftFootCollider.foot.AddForceAtPosition(Vector3.down * gravity * 0.5f * Time.fixedDeltaTime, forcePosition);

        rightDisplacement = root.position - (root.transform.right * rightFootInitialDisplacement.x);
        forcePosition = new Vector3(rightDisplacement.x, rightFootCollider.foot.position.y, rightDisplacement.z);
        rightFootCollider.foot.AddForceAtPosition(Vector3.down * gravity * 0.5f * Time.fixedDeltaTime, forcePosition);

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
        if (activeWalkingFoot == rightFootCollider)
        {
            activeWalkingFoot = leftFootCollider;
            otherWalkingFoot = rightFootCollider;
            activeFootDisplacement = rightFootInitialDisplacement;
        }
        else if (activeWalkingFoot == leftFootCollider)
        {
            activeWalkingFoot = rightFootCollider;
            otherWalkingFoot = leftFootCollider;
            activeFootDisplacement = leftFootInitialDisplacement;
        }
        else
        {
            throw new Exception("ERROR: ActiveWalkingFoot not assigned to either left or right foot");
        }

        awfRigidBody = activeWalkingFoot.foot.GetComponent<Rigidbody>();
        owfRigidBody = otherWalkingFoot.foot.GetComponent<Rigidbody>();
    }
    
    public void TeleportPhysicalBody(Vector3 _position)
    {
        Vector3 rootPosition = root.position;
        root.position = _position;
        rightLeg.position = _position + (rootPosition - rightLeg.position);
        leftLeg.position = _position + (rootPosition - leftLeg.position);
        rightFootCollider.foot.position = _position + (rootPosition - rightFootCollider.foot.position);
        leftFootCollider.foot.position = _position + (rootPosition - leftFootCollider.foot.position);
    }

    public void OnCollisionWithOther(float flopTime)
    {
        OnFlop(false, true, flopTime);
    }

    public void SetupBodyCollisionHandlers(Player owner)
    {
        foreach (BodyCollisionDetection bcd in bodyCollisionDetectors)
        {
            bcd.player = owner;
        }
    }
}
