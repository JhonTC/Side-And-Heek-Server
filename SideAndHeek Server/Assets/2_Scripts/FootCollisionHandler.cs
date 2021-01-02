using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollisionHandler : MonoBehaviour
{
    public Rigidbody foot;
    public bool isGrounded { get { return activeCollisionCount > 0; } }
    public bool IsGrounded;
    int activeCollisionCount = 0;

    private void Update()
    {
        IsGrounded = isGrounded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            activeCollisionCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            activeCollisionCount--;
        }
    }
}
