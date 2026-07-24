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

    [Header("Grid Offset")]
    [Tooltip("Number of empty cells before Day 1 starts (like a real calendar's first-week offset)")]
    public int startOffset = 0;

    [Header("Özel Hücreler (sadece override gereken günler)")]
    public List<CellData> cellOverrides = new List<CellData>();

    public bool IsDayInRange(int day) => day >= startDay && day <= endDay;

    public CellData GetCellOverride(int day)
    {
        return cellOverrides.FirstOrDefault(c => c.dayNumber == day);
    }
}