using UnityEngine;
using System;

[CreateAssetMenu(fileName = "MovesState", menuName = "Calendar/Runtime/Moves State")]
public class MovesState : ScriptableObject
{
    [System.NonSerialized] public int currentMoves;
    [System.NonSerialized] public int maxMoves;

    public event Action<int, int> OnMovesChanged; // (current, max)
    public event Action OnMovesDepleted;
    public event Action OnMoveConsumed;

    public void Initialize(int max)
    {
        maxMoves = max;
        currentMoves = max;
        OnMovesChanged?.Invoke(currentMoves, maxMoves);
        
    }

    // Bir hamle harcama giriþimi — baþarýlýysa true döner
    public bool ConsumeMove()
    {
        if (currentMoves <= 0)
        {
            Debug.Log("[MovesState] Hamle yok, harcanamadý.");
            return false;
        }

        currentMoves--;
        Debug.Log($"[MovesState] Hamle harcandý. Kalan: {currentMoves}/{maxMoves}");
        OnMovesChanged?.Invoke(currentMoves, maxMoves);
        OnMoveConsumed?.Invoke();
        if (currentMoves == 0) OnMovesDepleted?.Invoke();
        return true;
    }

    public void Clear()
    {
        currentMoves = 0;
        maxMoves = 0;
    }
}