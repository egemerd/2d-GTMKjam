using UnityEngine;

public class CountdownEffect : MonoBehaviour
{
    private PinController pin;
    private MovesState movesState;

    // Runtime davranış bayrakları (SO'dan gelir)
    private bool skipNextTick;
    private bool isPaused;

    public void Initialize(PinController targetPin, MovesState state, bool skipFirstMove, bool startPaused)
    {
        pin = targetPin;
        movesState = state;
        skipNextTick = skipFirstMove;
        isPaused = startPaused;

        if (movesState != null) movesState.OnMoveConsumed += HandleMoveConsumed;

        Debug.Log($"[Countdown] Pin ({pin.Value})'e eklendi. skipFirst={skipFirstMove}, startPaused={startPaused}");
    }

    void OnDestroy()
    {
        if (movesState != null) movesState.OnMoveConsumed -= HandleMoveConsumed;
    }

    void HandleMoveConsumed()
    {
        if (pin == null) return;

        if (skipNextTick)
        {
            skipNextTick = false;
            Debug.Log($"[Countdown] İlk hamle atlandı, sonraki hamleden başlayacak.");
            return;
        }

        if (isPaused)
        {
            Debug.Log($"[Countdown] Duraklatılmış durumda, değer değişmedi.");
            return;
        }

        int newValue = pin.Value - 1;
        Debug.Log($"[Countdown] Pin değeri {pin.Value} → {newValue}");
        pin.SetValue(newValue);
    }
}