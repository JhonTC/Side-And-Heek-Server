using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BodyCollider") //todo:being called multiple times... needs a 1 second cooldown or something
        {
            Player player = other.GetComponent<BodyCollisionDetection>().player;

            GameManager.instance.gameMode.OnPlayerHitFallDetector(player);

            Debug.Log(player.Username + " fell out of the map.");
        }
    }
}
