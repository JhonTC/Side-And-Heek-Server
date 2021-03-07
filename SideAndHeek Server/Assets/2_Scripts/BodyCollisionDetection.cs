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
                    if (other.playerType == player.playerType)
                    {
                        if (player.movementController.canKnockOutOthers)
                        {
                            other.OnCollisionWithOther(3f, false);
                        }
                    }
                    else if (player.playerType == PlayerType.Hunter)
                    {
                        if (!player.activePlayerCollisionIds.Contains(other.id))
                        {
                            player.activePlayerCollisionIds.Add(other.id);

                            if (GameManager.instance.gameRules.catchType == CatchType.OnTouch || (GameManager.instance.gameRules.catchType == CatchType.OnFlop && player.movementController.canKnockOutOthers))
                            {
                                other.OnCollisionWithOther(5f, true);
                            }
                        }
                    }
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
                    if (player.activePlayerCollisionIds.Contains(other.id))
                    {
                        player.activePlayerCollisionIds.Remove(other.id);
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
                        if (other.playerType == player.playerType)
                        {
                            if (player.movementController.canKnockOutOthers)
                            {
                                other.OnCollisionWithOther(3f, false);
                            }
                        }
                        else if (player.playerType == PlayerType.Hunter)
                        {
                            if (!player.activePlayerCollisionIds.Contains(other.id))
                            {
                                player.activePlayerCollisionIds.Add(other.id);

                                if (GameManager.instance.gameRules.catchType == CatchType.OnTouch || (GameManager.instance.gameRules.catchType == CatchType.OnFlop && player.movementController.canKnockOutOthers))
                                {
                                    other.OnCollisionWithOther(5f, true);
                                }
                            }
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
                
                player.movementController.canKnockOutOthers = false;
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
                    if (player.activePlayerCollisionIds.Contains(other.id))
                    {
                        player.activePlayerCollisionIds.Remove(other.id);
                    }
                }
            }
        }
    }
}
