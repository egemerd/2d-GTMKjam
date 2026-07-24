using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MergeOperation", menuName = "Calendar/Operations/Merge")]
public class MergeOperationSO : PinOperationSO
{
    void OnEnable() => requiredPinCount = 2;

    protected override bool ValidateSpecific(List<PinController> pins)
    {
        // Ýki sayý da 2 basamaklý OLAMAZ
        return !(pins[0].Value >= 10 && pins[1].Value >= 10);
    }

    public override void Execute(List<PinController> pins)
    {
        // Ýlk seçilen baþa, ikinci sona
        string merged = pins[0].Value.ToString() + pins[1].Value.ToString();
        pins[0].SetValue(int.Parse(merged));
        pins[1].Consume();
    }
}