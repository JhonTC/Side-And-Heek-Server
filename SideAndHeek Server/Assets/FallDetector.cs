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
            } else
            {
                if (GameManager.instance.gameRules.fallRespawnType == HiderFallRespawnType.Hunter)
                {
                    player.SetPlayerType(PlayerType.Hunter, false);
                }
            }

            player.TeleportPlayer(LevelManager.GetLevelManagerForScene(activeSceneName).GetNextSpawnpoint(GameManager.instance.gameRules.fallRespawnLocation == FallRespawnLocation.Centre));

            Debug.Log(player.Username + " fell out of the map.");
        }
    }
}
