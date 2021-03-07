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
    private List<Player> trappedPlayers = new List<Player>();

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        startExplosionSize = transform.localScale.x;
        currentExplosionSize = startExplosionSize;
        smoothCES = startExplosionSize;
    }

    private void FixedUpdate()
    {
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

                //
            }
        }
    }

    public void Init(PickupSpawner _spawner, int _creatorId, PickupType _pickupType, int _code, Vector3 throwDirection, float throwForce)
    {
        rigidbody.AddForce(throwDirection * throwForce * throwForceMultiplier);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isExploding)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.useGravity = false;

            isExploding = true;
        }

        if (other.CompareTag("BodyCollider"))
        {
            Player player = other.GetComponentInParent<Player>();
            //trap them
            player.movementController.root.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            trappedPlayers.Add(player);
        }
    }

    IEnumerator StartLifetimeCoundown()
    {
        int currentLifetimeCount = lifeDuration;
        while (currentLifetimeCount > 0)
        {
            yield return new WaitForSeconds(1);

            currentLifetimeCount--;
        }

        //destroy this;
    }
}
