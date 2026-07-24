using UnityEngine;

public class CountdownEffect : MonoBehaviour
{
    private PinController pin;
    private MovesState movesState;

    public void Initialize(PinController targetPin, MovesState state)
    {
        pin = targetPin;
        movesState = state;

        if (movesState != null) movesState.OnMoveConsumed += HandleMoveConsumed;

        Debug.Log($"[Countdown] Pin ({pin.Value})'e eklendi.");
    }

    void OnDestroy()
    {
        if (movesState != null) movesState.OnMoveConsumed -= HandleMoveConsumed;
    }

    void HandleMoveConsumed()
    {
        if (pin == null) return;

        int newValue = pin.Value - 1;
        Debug.Log($"[Countdown] Pin değeri {pin.Value} → {newValue}");
        pin.SetValue(newValue);
    }
}