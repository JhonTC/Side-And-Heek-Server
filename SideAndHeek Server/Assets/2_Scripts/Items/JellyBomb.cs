using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBomb : SpawnableObject
{
    public float explosionRadius;
    public float explosionSpeed;
    public AnimationCurve explosionCurve;
    public float explosionForce;
    public float throwForce;
    public float throwForceMultiplier;

    private float lifeDuration;
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
        public Rigidbody rigidbody;
        public Player owner;
        public bool initialUseGravity;
        public float intialDrag;
        public float intialAngularDrag;
        public float initialOwnerGravity;

        public TrappedBody(Rigidbody _rigidbody, Player _owner = null)
        {
            rigidbody = _rigidbody;
            owner = _owner;
            initialUseGravity = rigidbody.useGravity;
            intialDrag = rigidbody.drag;
            intialAngularDrag = rigidbody.angularDrag;

            if (owner != null)
            {
                initialOwnerGravity = owner.movementController.gravity;
            }
        }

        public void Stick()
        {
            rigidbody.useGravity = false;
            rigidbody.drag = 5f;
            rigidbody.angularDrag = 5f;

            if (owner != null)
            {
                owner.movementController.useGravity = false;
                //owner.isAcceptingInput = false;
            }
        }

        public void Reset()
        {
            rigidbody.useGravity = initialUseGravity;
            rigidbody.drag = intialDrag;
            rigidbody.angularDrag = intialAngularDrag;

            if (owner != null)
            {
                Debug.Log(owner.name);
                owner.movementController.useGravity = true;
                //owner.isAcceptingInput = true;
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
        if (isExploding)
        {
            if (currentExplosionSize < explosionRadius)
            {
                transform.localScale = Vector3.one * currentExplosionSize;
                currentExplosionSize += Mathf.Clamp(explosionSpeed * Time.fixedDeltaTime, startExplosionSize, explosionRadius);
                foreach (TrappedBody body in trappedBodies)
                {
                    float multiplier = explosionRadius - currentExplosionSize;
                    body.rigidbody.AddForce(Vector3.up * explosionForce * multiplier);
                }
            }
            else
            {
                isExploding = false;
                hasExploded = true;
                StartCoroutine(StartLifetimeCoundown());
            }
        }

        base.FixedUpdate();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < trappedBodies.Count; i++)
        {
            trappedBodies[i].Reset();
        }
    }

    public override void Init(ushort _creatorId, int _code)
    {
        base.Init(_creatorId, _code);

        lifeDuration = activeItemDetails.pickupSO.duration;
        rigidbody.AddForce(Player.list[_creatorId].shootDirection * Player.list[_creatorId].throwForce * throwForceMultiplier);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyCollider"))
        {
            if (TrappedBodiesIndexOf(other.attachedRigidbody) == null)
            {
                Player player = other.GetComponentInParent<Player>();
                if (player.Id == creatorId && !isExploding && !hasExploded)
                {
                    return;
                }

                Transform rigidbodyObject = other.transform;
                if (rigidbodyObject.name == "Head")
                {
                    rigidbodyObject = rigidbodyObject.transform.parent;
                }

                Rigidbody trappedRigidbody = rigidbodyObject.GetComponent<Rigidbody>();
                if (trappedRigidbody == null)
                {
                    return;
                }

                TrappedBody trappedBody = new TrappedBody(trappedRigidbody, player);
                trappedBody.Stick();
                trappedBodies.Add(trappedBody);
            }
        }

        if (!isExploding && !hasExploded)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.useGravity = false;

            isExploding = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BodyCollider"))
        {
            Transform rigidbodyObject = other.transform;
            if (rigidbodyObject.name == "Head")
            {
                rigidbodyObject = rigidbodyObject.transform.parent;
            }

            TrappedBody trappedBody = TrappedBodiesIndexOf(rigidbodyObject.GetComponent<Rigidbody>());
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
            if (trappedBodies[i].rigidbody == _rigidbody)
            {
                return trappedBodies[i];
            }
        }

        return null;
    } 

    IEnumerator StartLifetimeCoundown()
    {
        int currentLifetimeCount = Mathf.FloorToInt(lifeDuration);
        while (currentLifetimeCount >= 0)
        {
            yield return new WaitForSeconds(1);

            currentLifetimeCount--;
        }

        for (int i = 0; i < trappedBodies.Count; i++)
        {
            trappedBodies[i].Reset();
        }

        transform.localScale = Vector3.zero;

        NetworkObjectsManager.instance.DestroyObject(this);
    }
}
