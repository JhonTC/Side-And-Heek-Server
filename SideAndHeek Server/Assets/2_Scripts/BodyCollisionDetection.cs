using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollisionDetection : MonoBehaviour
{
    [HideInInspector] public Player player;
    [SerializeField] private bool isHeadCollider = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (player != null)
        {
            if (collision.gameObject.tag == "BodyCollider")
            {
                Player other = collision.gameObject.GetComponent<BodyCollisionDetection>().player;
                if (other != player)
                {
                    GameManager.instance.gameMode.OnPlayerCollision(player, other);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (player != null)
        {
            if (collision.gameObject.tag == "BodyCollider")
            {
                Player other = collision.gameObject.GetComponent<BodyCollisionDetection>().player;
                if (other != player)
                {
                    if (player.activePlayerCollisionIds.Contains(other.Id))
                    {
                        player.activePlayerCollisionIds.Remove(other.Id);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (player != null)
        {
            if (collider.tag == "BodyCollider")
            {
                Player other = collider.GetComponent<BodyCollisionDetection>().player;
                if (player != null)
                {
                    if (other != player)
                    {
                        GameManager.instance.gameMode.OnPlayerCollision(player, other);
                    }
                }
            }

            if (isHeadCollider)
            {
                bool canSend = true;

                if (collider.tag == "BodyCollider")
                {
                    Player other = collider.GetComponent<BodyCollisionDetection>().player;
                    if (other != player)
                    {
                        player.movementController.canKnockOutOthers = false;
                    } else
                    {
                        canSend = false;
                    }
                }

                if (canSend)
                {
                    player.headCollided = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (player != null)
        {
            if (collider.tag == "BodyCollider")
            {
                Player other = collider.GetComponent<BodyCollisionDetection>().player;
                if (other != player)
                {
                    if (player.activePlayerCollisionIds.Contains(other.Id))
                    {
                        player.activePlayerCollisionIds.Remove(other.Id);
                    }
                }
            }
        }
    }
}
