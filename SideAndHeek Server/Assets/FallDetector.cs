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

            string activeSceneName = GameManager.instance.activeSceneName;
            if (player.playerType == PlayerType.Hunter)
            {
                activeSceneName = GameManager.instance.activeHunterSceneName;
            }

            player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(true));

            Debug.Log(player.username + " fell out of the map.");
        }
    }
}
