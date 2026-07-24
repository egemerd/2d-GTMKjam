using TMPro;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text birthDateLabel;

    private int birthMonth, birthYear;
    private int targetAge;

    public int TargetAge => targetAge;

    public void Setup(int month, int year, int currentYear)
    {
        birthMonth = month;
        birthYear = year;
        targetAge = currentYear - year;

        if (birthDateLabel != null)
            birthDateLabel.text = $"{year}";
    }


}
