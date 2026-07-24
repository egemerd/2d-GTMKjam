using UnityEngine;
using TMPro;

public class MovesUIDisplay : MonoBehaviour
{
    [SerializeField] private MovesState movesState;
    [SerializeField] private TMP_Text movesLabel;
    [SerializeField] private string format = "{0} / {1}"; // Inspector'dan format deðiþtirilebilir

    void OnEnable()
    {
        if (movesState != null)
        {
            movesState.OnMovesChanged += HandleMovesChanged;
            // Zaten initialize olduysa hemen güncelle
            HandleMovesChanged(movesState.currentMoves, movesState.maxMoves);
        }
    }

    void OnDisable()
    {
        if (movesState != null) movesState.OnMovesChanged -= HandleMovesChanged;
    }

    void HandleMovesChanged(int current, int max)
    {
        if (movesLabel != null) movesLabel.text = movesState.currentMoves.ToString();
    }
}