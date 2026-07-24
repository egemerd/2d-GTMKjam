using UnityEngine;

[CreateAssetMenu(fileName = "CurrentLevelState", menuName = "Calendar/Runtime/Current Level State")]
public class CurrentLevelState : ScriptableObject
{
    [System.NonSerialized] public int targetAge;
    [System.NonSerialized] public CalendarLevelData activeLevel;

    public void Initialize(CalendarLevelData level)
    {
        activeLevel = level;
        targetAge = level.TargetAge;
    }

    public void Clear()
    {
        activeLevel = null;
        targetAge = 0;
    }
}