using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("END GAME"))
        {
            gameManager.GameOver();
        }
    }
}
