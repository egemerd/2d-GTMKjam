using UnityEngine;

public abstract class PinDropValidatorSO : ScriptableObject
{
    public abstract bool Validate(int pinValue);
}