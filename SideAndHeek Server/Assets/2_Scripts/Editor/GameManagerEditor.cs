using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    bool showGameRulesFoldout = true;

    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;

        base.OnInspectorGUI();

        showGameRulesFoldout = EditorGUILayout.Foldout(showGameRulesFoldout, "Game Rules");
        if (showGameRulesFoldout)
        {
            EditorGUI.BeginDisabledGroup(true);
            if (gameManager.gameMode != null)
            {
                GameRules gameRules = gameManager.gameMode.GetGameRules();
                if (gameRules != null)
                {
                    Dictionary<string, object> valueList = gameManager.gameMode.GetGameRules().GetListOfValues();
                    foreach (var item in valueList)
                    {
                        EditorGUILayout.TextField(item.Key.ToString(), item.Value.ToString());
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        if (GUILayout.Button("SpawnNPC"))
        {
            Player.Spawn(100, "NPC");
        }

        if (GUILayout.Button("Check For Gameover"))
        {
            gameManager.CheckForGameOver();
        }

        if (GUILayout.Button("END GAME"))
        {
            gameManager.GameOver();
        }
    }
}
