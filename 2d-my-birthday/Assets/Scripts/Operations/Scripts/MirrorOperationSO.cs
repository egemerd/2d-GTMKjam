using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MirrorOperation", menuName = "Calendar/Operations/Mirror")]
public class MirrorOperationSO : PinOperationSO
{
    void OnEnable() => requiredPinCount = 1;

    protected override bool ValidateSpecific(List<PinController> pins)
    {
        // Sadece çift basamaklý sayýlar için (10-99)
        int v = pins[0].Value;
        return v >= 10 && v <= 99;
    }

    public override void Execute(List<PinController> pins)
    {
        int v = pins[0].Value;
        int tens = v / 10;
        int ones = v % 10;
        int mirrored = ones * 10 + tens;
        pins[0].SetValue(mirrored);
    }
}