using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SubtractOperation", menuName = "Calendar/Operations/Subtract")]
public class SubtractOperationSO : PinOperationSO
{
    void OnEnable() => requiredPinCount = 2;

    protected override bool ValidateSpecific(List<PinController> pins) => true;

    public override void Execute(List<PinController> pins)
    {
        // Ýlk seçilen - ikinci seçilen (sýra önemli)
        int result = pins[0].Value - pins[1].Value;
        if(result < 0)
        {
            result = -result;
        }
        Debug.Log($"SubtractOperation: {pins[0].Value} - {pins[1].Value} = {result}");
        pins[0].SetValue(result);
        pins[1].Consume();
    }
}