using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CountdownOperation", menuName = "Calendar/Operations/Countdown")]
public class CountdownOperationSO : PinOperationSO
{
    [Header("Runtime References")]
    [SerializeField] private MovesState movesState;

    [Header("Countdown Behavior")]
    [SerializeField] private bool skipFirstMove = true;
    [SerializeField] private bool startPaused = false;

    void OnEnable() => requiredPinCount = 1;

    protected override bool ValidateSpecific(List<PinController> pins)
    {
        // Ayný pin'e ikinci kez countdown eklenmesin
        return pins[0].GetComponent<CountdownEffect>() == null;
    }

    public override void Execute(List<PinController> pins)
    {
        // Görsel mod deðiþimi
        pins[0].SetCalendarMode(true);

        // Effect ekle
        CountdownEffect effect = pins[0].gameObject.AddComponent<CountdownEffect>();
        effect.Initialize(pins[0], movesState, skipFirstMove, startPaused);
    }
}