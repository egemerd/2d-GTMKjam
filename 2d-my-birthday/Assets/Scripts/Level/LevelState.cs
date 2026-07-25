using UnityEngine;
using System;

public enum LevelResult
{
    InProgress,
    Won,
    Lost
}

[CreateAssetMenu(fileName = "LevelState", menuName = "Calendar/Runtime/Level State")]
public class LevelState : ScriptableObject
{
    [System.NonSerialized] public LevelResult currentResult = LevelResult.InProgress;

    public event Action OnLevelWon;
    public event Action OnLevelLost;
    public event Action OnLevelReset;

    public void ReportWin()
    {
        if (currentResult != LevelResult.InProgress) return; // zaten karar verildi
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
        currentResult = LevelResult.InProgress;
        OnLevelReset?.Invoke();
    }
}