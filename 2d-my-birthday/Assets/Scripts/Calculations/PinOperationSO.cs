using UnityEngine;
using System.Collections.Generic;

public abstract class PinOperationSO : ScriptableObject
{
    [Header("Operation Info")]
    public string operationName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Requirements")]
    public int requiredPinCount = 1;

    // Seçim bu operasyon için geçerli mi?
    public virtual bool CanExecute(List<PinController> selectedPins)
    {
        if (selectedPins == null || selectedPins.Count != requiredPinCount)
            return false;
        return ValidateSpecific(selectedPins);
    }

    // Her operasyonun kendi ek kurallarý (2 basamak þartý vb.)
    protected abstract bool ValidateSpecific(List<PinController> selectedPins);

    // Sonucu hesapla ve pinlere uygula
    public abstract void Execute(List<PinController> selectedPins);
}