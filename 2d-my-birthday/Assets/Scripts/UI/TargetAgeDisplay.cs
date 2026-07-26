using UnityEngine;
using TMPro;

public class TargetAgeDisplay : MonoBehaviour
{
    [SerializeField] private CurrentLevelState levelState;
    [SerializeField] private TextMeshProUGUI ageLabel;
    [SerializeField] private string format = "Target: {0}"; // Inspector'dan deðiþtirilebilir

    void Start()
    {
        Refresh();
    }

    void OnEnable()
    {
        if (levelState != null) levelState.OnStateChanged += Refresh;
    }

    void OnDisable()
    {
        if (levelState != null) levelState.OnStateChanged -= Refresh;
    }


    public void Refresh()
    {
        if (levelState == null || ageLabel == null) return;
        ageLabel.text = levelState.targetAge.ToString();
    }
}