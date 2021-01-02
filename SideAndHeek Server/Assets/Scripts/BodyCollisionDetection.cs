using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollisionDetection : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private bool isHeadCollider = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "BodyCollider")
        {
            Player other = collision.gameObject.GetComponent<BodyCollisionDetection>().player;
            if (other != player)
            {
                if (other.playerType == player.playerType)
                {
                    if (player.controller.canKnockOutOthers)
                    {
                        other.OnCollisionWithOther(3f, false);
                    }
                }
                else if (player.playerType == PlayerType.Hunter)
                {
                    if (!player.activePlayerCollisionIds.Contains(other.id))
                    {
                        player.activePlayerCollisionIds.Add(other.id);

                        other.OnCollisionWithOther(5f, true);
                        //send caught to other player
                    }
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "BodyCollider")
        {
            Player other = collision.gameObject.GetComponent<BodyCollisionDetection>().player;
            if (other != player)
            {
                if (player.activePlayerCollisionIds.Contains(other.id))
                {
                    player.activePlayerCollisionIds.Remove(other.id);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "BodyCollider")
        {
            Player other = collider.GetComponent<BodyCollisionDetection>().player;
            if (other != player)
            {
                if (other.playerType == player.playerType)
                {
                    if (player.controller.canKnockOutOthers)
                    {
                        other.OnCollisionWithOther(3f, false);
                    }
                }
                else if (player.playerType == PlayerType.Hunter)
                {
                    if (!player.activePlayerCollisionIds.Contains(other.id))
                    {
                        player.activePlayerCollisionIds.Add(other.id);

                        other.OnCollisionWithOther(5f, true);
                        //send caught to other player
                    }
                }
            }
        }
        
        if (isHeadCollider)
        {
            if (collider.tag == "BodyCollider")
            {
                Player other = collider.GetComponent<BodyCollisionDetection>().player;
                if (other == player)
                {
                    return;
                }
            }

            player.controller.canKnockOutOthers = false;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "BodyCollider")
        {
            Player other = collider.GetComponent<BodyCollisionDetection>().player;
            if (other != player)
            {
                if (player.activePlayerCollisionIds.Contains(other.id))
                {
                    player.activePlayerCollisionIds.Remove(other.id);
                }
            }
        }
    }
}
