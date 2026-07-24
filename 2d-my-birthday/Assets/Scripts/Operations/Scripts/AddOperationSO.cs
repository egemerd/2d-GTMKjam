using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AddOperation", menuName = "Calendar/Operations/Add")]
public class AddOperationSO : PinOperationSO
{
    void OnEnable() => requiredPinCount = 2;

    protected override bool ValidateSpecific(List<PinController> pins) => true;

    public override void Execute(List<PinController> pins)
    {
        int result = pins[0].Value + pins[1].Value;
        pins[0].SetValue(result);
        pins[1].Consume(); // ikinci pin yok olur
    }
}