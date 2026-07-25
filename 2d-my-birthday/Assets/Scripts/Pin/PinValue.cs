using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PinValue : MonoBehaviour
{
    [SerializeField] private int value;
    [SerializeField] private List<TMP_Text> valueLabels = new List<TMP_Text>();

    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            RefreshLabels();
        }
    }

    void Start() => RefreshLabels();

    void RefreshLabels()
    {
        string s = value.ToString();
        foreach (var label in valueLabels)
            if (label != null) label.text = s;
    }
}