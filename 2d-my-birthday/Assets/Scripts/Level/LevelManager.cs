using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelState mainLevelState;
    [SerializeField] private MovesState movesState;

    void OnEnable()
    {
        if (movesState != null) movesState.OnMovesDepleted += HandleMovesDepleted;
    }

    void OnDisable()
    {
        if (movesState != null) movesState.OnMovesDepleted -= HandleMovesDepleted;
    }

    void HandleMovesDepleted()
    {
        // Hamle bittiðinde eðer hâlâ InProgress ise kaybettin demektir
        // (Win olsaydý zaten LevelState kararý vermiþ olurdu)
        //mainLevelState?.ReportLoss();
    }
}