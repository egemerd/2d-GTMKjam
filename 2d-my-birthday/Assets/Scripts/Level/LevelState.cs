using UnityEngine;
using System;

public enum LevelResult
{
    NotStarted,  // yeni — tutorial/intro sırasında
    InProgress,
    Won,
    Lost
}

[CreateAssetMenu(fileName = "LevelState", menuName = "Calendar/Runtime/Level State")]
public class LevelState : ScriptableObject
{
    [System.NonSerialized] public LevelResult currentResult = LevelResult.NotStarted;

    public event Action OnLevelWon;
    public event Action OnLevelLost;
    public event Action OnLevelReset;

    public void StartGameplay()
    {
        if (currentResult != LevelResult.NotStarted) return;
        currentResult = LevelResult.InProgress;
        Debug.Log("[LevelState] Oynanış başladı.");
    }

    public void ReportWin()
    {
        if (currentResult != LevelResult.InProgress) return;
        currentResult = LevelResult.Won;
        Debug.Log("[LevelState] ✓ LEVEL KAZANILDI");
        OnLevelWon?.Invoke();
    }

    public void ReportLoss()
    {
        if (currentResult != LevelResult.InProgress) return;
        currentResult = LevelResult.Lost;
        Debug.Log("[LevelState] ✗ LEVEL KAYBEDİLDİ");
        OnLevelLost?.Invoke();
    }

    public void ResetState()
    {
        currentResult = LevelResult.NotStarted; // InProgress değil, NotStarted
        OnLevelReset?.Invoke();
    }
}