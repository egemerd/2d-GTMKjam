using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DigitSumOperation", menuName = "Calendar/Operations/Digit Sum")]
public class DigitSumOperationSO : PinOperationSO
{
    void OnEnable() => requiredPinCount = 1;

    protected override bool ValidateSpecific(List<PinController> pins)
    {
        // Sadece çift basamaklý için
        int v = pins[0].Value;
        return v >= 10 && v <= 99;
    }

    public override void Execute(List<PinController> pins)
    {
        int v = pins[0].Value;
        int sum = (v / 10) + (v % 10);
        pins[0].SetValue(sum);
    }
}