using UnityEngine;

[CreateAssetMenu(fileName = "AgeValidator", menuName = "Calendar/Validators/Age")]
public class AgeValidatorSO : PinDropValidatorSO
{
    [SerializeField] private CurrentLevelState levelState;

    public override bool Validate(int pinValue)
    {
        return levelState != null && pinValue == levelState.targetAge;
    }
}