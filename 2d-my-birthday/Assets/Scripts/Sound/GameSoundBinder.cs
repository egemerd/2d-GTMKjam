using UnityEngine;

public class GameSoundBinder : MonoBehaviour
{
    [Header("State References")]
    [SerializeField] private LevelState levelState;
    [SerializeField] private MovesState movesState;
    [SerializeField] private PinSelectionManager selectionManager; // opsiyonel

    [Header("Sounds")]
    [SerializeField] private SoundSO onMoveConsumed;
    [SerializeField] private SoundSO onMovesDepleted;
    [SerializeField] private SoundSO onLevelWon;
    [SerializeField] private SoundSO onLevelLost;
    [SerializeField] private SoundSO onGameplayStarted;
    [SerializeField] private SoundSO onSelectionChanged;

    private void OnEnable()
    {
        movesState.OnMoveConsumed += HandleMoveConsumed;
        movesState.OnMovesDepleted += HandleMovesDepleted;
        levelState.OnLevelWon += HandleWin;
        levelState.OnLevelLost += HandleLose;
        //levelState.OnGameplayStarted += HandleStart;

        //if (selectionManager != null)
        //    selectionManager.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        movesState.OnMoveConsumed -= HandleMoveConsumed;
        movesState.OnMovesDepleted -= HandleMovesDepleted;
        levelState.OnLevelWon -= HandleWin;
        levelState.OnLevelLost -= HandleLose;
        //levelState.OnGameplayStarted -= HandleStart;

        //if (selectionManager != null)
        //    selectionManager.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void HandleMoveConsumed() => Play(onMoveConsumed);
    private void HandleMovesDepleted() => Play(onMovesDepleted);
    private void HandleWin() => Play(onLevelWon);
    private void HandleLose() => Play(onLevelLost);
    private void HandleStart() => Play(onGameplayStarted);
    //private void HandleSelectionChanged() => Play(onSelectionChanged);

    private void Play(SoundSO s)
    {
        if (s != null && AudioManager.Instance != null)
            AudioManager.Instance.Play(s);
    }
}