using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CountdownOperation", menuName = "Calendar/Operations/Countdown")]
public class CountdownOperationSO : PinOperationSO
{
    [SerializeField] private MovesState movesState;

    void OnEnable() => requiredPinCount = 1;

    protected override bool ValidateSpecific(List<PinController> pins)
    {
        // Zaten countdown varsa tekrar ekleme
        return pins[0].GetComponent<CountdownEffect>() == null;
    }

    public override void Execute(List<PinController> pins)
    {
        CountdownEffect effect = pins[0].gameObject.AddComponent<CountdownEffect>();
        effect.Initialize(pins[0], movesState);
    }
}