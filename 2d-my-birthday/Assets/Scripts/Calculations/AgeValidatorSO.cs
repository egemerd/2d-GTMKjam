using UnityEngine;

[CreateAssetMenu(fileName = "AgeValidator", menuName = "Calendar/Validators/Age")]
public class AgeValidatorSO : PinDropValidatorSO
{
    [SerializeField] private CurrentLevelState levelState;

    public override bool Validate(int pinValue)
    {
        if (levelState == null)
        {
            Debug.LogError("[AgeValidator] LevelState null! Inspector'dan CurrentLevelState asset'ini sürüklemen lazým.");
            return false;
        }

        int target = levelState.targetAge;
        bool match = pinValue == target;
        Debug.Log($"[AgeValidator] Pin: {pinValue} | Hedef Yaþ: {target} | Eþleþme: {match}");
        return match;
    }
}