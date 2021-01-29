using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BodyCollider")
        {
            Player player = other.GetComponent<BodyCollisionDetection>().player;
            player.TeleportPlayer(LevelManager.GetLevelManagerForScene(GameManager.instance.activeSceneName).GetNextSpawnpoint(true));



            Debug.Log(player.username + " fell out of the map.");
        }
    }
}
