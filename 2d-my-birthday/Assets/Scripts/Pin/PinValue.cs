using UnityEngine;
using TMPro; // TextMeshPro kullanýyorsan, kullanmýyorsan UnityEngine.UI.Text

public class PinValue : MonoBehaviour
{
    [SerializeField] private int value;
    [SerializeField] private TMP_Text valueLabel; // opsiyonel: pin üstündeki sayý görseli

    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            RefreshLabel();
        }
    }

    void Start() => RefreshLabel();

    void RefreshLabel()
    {
        if (valueLabel != null) valueLabel.text = value.ToString();
    }
}