using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new task difficaulty", menuName = "Data/Task Difficaulty")]
public class TaskDifficaulty : ScriptableObject
{
    public DifficultyLevel difficulty;
    public Color color;
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Extreme
}
