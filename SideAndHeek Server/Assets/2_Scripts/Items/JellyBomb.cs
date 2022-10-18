using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBomb : SpawnableObject
{
    public float explosionRadius;
    public float explosionSpeed;
    public AnimationCurve explosionCurve;
    public float explosionForce;
    public float throwForceMultiplier;
    public int lifeDuration;

    private Rigidbody rigidbody;
    private float currentExplosionSize;
    private float smoothCES;
    private float startExplosionSize;
    private bool isExploding = false;
    private bool hasExploded = false;
    private List<Player> trappedPlayers = new List<Player>();
    private List<TrappedBody> trappedBodies = new List<TrappedBody>();

    private class TrappedBody
    {
        public Rigidbody rigibody;
        public Player owner;
        public bool initialUseGravity;
        public float intialDrag;
        public float intialAngularDrag;
        public float initialOwnerGravity;

        public TrappedBody(Rigidbody _rigidbody, Player _owner = null)
        {
            rigibody = _rigidbody;
            owner = _owner;
            initialUseGravity = rigibody.useGravity;
            intialDrag = rigibody.drag;
            intialAngularDrag = rigibody.angularDrag;

            if (owner != null)
            {
                initialOwnerGravity = owner.movementController.gravity;
            }
        }

        public void Stick()
        {
            rigibody.useGravity = false;
            rigibody.drag = 2f;
            rigibody.angularDrag = 1.5f;

            if (owner != null)
            {
                owner.movementController.gravity = 0f;
                owner.isAcceptingInput = false;
            }
        }

        public void Reset()
        {
            rigibody.useGravity = initialUseGravity;
            rigibody.drag = intialDrag;
            rigibody.angularDrag = intialAngularDrag;

            if (owner != null)
            {
                owner.movementController.gravity = initialOwnerGravity;
                owner.isAcceptingInput = true;
            }
        }
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        startExplosionSize = transform.localScale.x;
        currentExplosionSize = startExplosionSize;
        smoothCES = startExplosionSize;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isExploding)
        {
            if (currentExplosionSize < explosionRadius)
            {
                transform.localScale = Vector3.one * currentExplosionSize;
                currentExplosionSize += explosionSpeed * Time.fixedDeltaTime;
                //smoothCES = explosionRadius * (explosionCurve.Evaluate((explosionRadius - startExplosionSize) / currentExplosionSize));
            }
            else
            {
                currentExplosionSize = startExplosionSize;
                isExploding = false;
                hasExploded = true;
                StartCoroutine(StartLifetimeCoundown());
            }
        }
    }

    public void Init(ushort _creatorId, int _code, Vector3 throwDirection, float throwForce)
    {
        base.Init(_creatorId, _code, true);

        rigidbody.AddForce(throwDirection * throwForce * throwForceMultiplier);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!hasExploded)
        {
            if (other.CompareTag("BodyCollider"))
            {
                Player player = other.GetComponentInParent<Player>();
                if (player.Id == creatorId)
                {
                    return;
                } else
                {
                    //player.movementController.root.velocity = Vector3.zero;
                    player.movementController.root.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                    TrappedBody trappedBody = new TrappedBody(player.movementController.root, player);
                    trappedBody.Stick();
                    trappedBodies.Add(trappedBody);

                    trappedBody = new TrappedBody(player.movementController.leftLeg);
                    trappedBody.Stick();
                    trappedBodies.Add(trappedBody);

                    trappedBody = new TrappedBody(player.movementController.rightLeg);
                    trappedBody.Stick();
                    trappedBodies.Add(trappedBody);

                    trappedBody = new TrappedBody(player.movementController.leftFootCollider.foot);
                    trappedBody.Stick();
                    trappedBodies.Add(trappedBody);

                    trappedBody = new TrappedBody(player.movementController.rightFootCollider.foot);
                    trappedBody.Stick();
                    trappedBodies.Add(trappedBody);
                }
            }

            if (!isExploding)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.useGravity = false;

                isExploding = true;
            }
        } else
        {
            if (other.CompareTag("BodyCollider"))
            {
                if (TrappedBodiesIndexOf(other.attachedRigidbody) == null)
                {
                    Player player = other.GetComponentInParent<Player>();
                    if (player.Id == creatorId)
                    {
                        TrappedBody trappedBody = new TrappedBody(player.movementController.root, other.attachedRigidbody == player.movementController.root ? player : null);
                        trappedBody.Stick();
                        trappedBodies.Add(trappedBody);
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BodyCollider"))
        {
            TrappedBody trappedBody = TrappedBodiesIndexOf(other.attachedRigidbody);
            if (trappedBody != null) {
                trappedBody.Reset();
                trappedBodies.Remove(trappedBody);
            }
        }
    }

    private TrappedBody TrappedBodiesIndexOf(Rigidbody _rigidbody)
    {
        for (int i = 0; i < trappedBodies.Count; i++)
        {
            if (trappedBodies[i].rigibody == _rigidbody)
            {
                return trappedBodies[i];
            }
        }

        return null;
    } 

    IEnumerator StartLifetimeCoundown()
    {
        int currentLifetimeCount = lifeDuration;
        while (currentLifetimeCount > 0)
        {
            yield return new WaitForSeconds(1);

            currentLifetimeCount--;
        }

        for (int i = 0; i < trappedBodies.Count; i++)
        {
            trappedBodies[i].Reset();
        }

        transform.localScale = Vector3.zero;
    }
}
