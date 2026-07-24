using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CountdownOperation", menuName = "Calendar/Operations/Countdown")]
public class CountdownOperationSO : PinOperationSO
{
    [SerializeField] private MovesState movesState;

    [Header("Countdown Behavior")]
    [SerializeField] private bool skipFirstMove = true;
    [SerializeField] private bool startPaused = false;

    void OnEnable() => requiredPinCount = 1;

    protected override bool ValidateSpecific(List<PinController> pins)
        => pins[0].GetComponent<CountdownEffect>() == null;

    public override void Execute(List<PinController> pins)
    {
        var effect = pins[0].gameObject.AddComponent<CountdownEffect>();
        effect.Initialize(pins[0], movesState, skipFirstMove, startPaused);
    }
}