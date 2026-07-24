using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewCalendarLevel", menuName = "Calendar/Level Data")]
public class CalendarLevelData : ScriptableObject
{
    [Header("Takvim Boyutu")]
    public int totalDays = 30;
    public int columns = 7;

    [Header("Pin Aralýðý")]
    public int startDay = 3;
    public int endDay = 8;

    [Header("Varsayýlan Pin")]
    public GameObject defaultPinPrefab;

    [Header("Character / Age Puzzle")]
    public GameObject characterPrefab;
    public Vector3 characterSpawnPosition = Vector3.zero;

    [Header("Birth Date")]
    public int birthMonth = 1;
    public int birthYear = 2000;

    [Header("Puzzle 'Today'")]
    public int currentYear = 2026;

    [Header("Skipped Days (kullanýcý seçimi, sarý)")]
    public List<int> skippedDays = new List<int>();

    [Header("Weekend Detection")]
    public int firstDayOfWeek = 1; // varsayýlan: takvim Pazartesi baþlýyor

    // Puzzle'ýn hedef deðeri — pin iþlemleriyle ulaþýlmasý gereken sayý
    public int TargetAge => currentYear - birthYear;

    [Header("Grid Offset")]
    [Tooltip("Number of empty cells before Day 1 starts (like a real calendar's first-week offset)")]
    public int startOffset = 0;

    [Header("Özel Hücreler (sadece override gereken günler)")]
    public List<CellData> cellOverrides = new List<CellData>();

    public bool IsDayInRange(int day) => day >= startDay && day <= endDay;

    public bool IsWeekend(int dayNumber)
    {
        // Grid'deki pozisyonu bul: offset + (day - 1)
        int gridIndex = startOffset + (dayNumber - 1);
        int columnIndex = gridIndex % columns;

        // Sütun 5 ve 6 (0-indexed) = haftasonu (Cumartesi, Pazar) — 7 sütunlu takvim varsayýmý
        // Ama daha esnek olmak için: son iki sütun
        return columnIndex >= columns - 2;
    }

    public bool IsSkipped(int dayNumber) => skippedDays.Contains(dayNumber);

    // Bu gün pin taþýmalý mý?
    public bool ShouldSpawnPin(int dayNumber)
    {
        if (!IsDayInRange(dayNumber)) return false;
        if (IsWeekend(dayNumber)) return false;
        if (IsSkipped(dayNumber)) return false;
        return true;
    }

    public CellData GetCellOverride(int day)
    {
        return cellOverrides.FirstOrDefault(c => c.dayNumber == day);
    }   
}
