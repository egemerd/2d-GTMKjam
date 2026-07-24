using UnityEngine;

public class CalendarSlot : MonoBehaviour
{
    public int dayNumber;
    public CellModifier modifier;
    [HideInInspector] public DragDrop currentPin;

    [SerializeField] private SpriteRenderer highlightRenderer; // opsiyonel hover feedback

    public bool IsOccupied => currentPin != null;

    public bool CanAcceptPin()
    {
        return !IsOccupied && !modifier.HasFlag(CellModifier.Blocked);
    }

    public void AssignPin(DragDrop pin) => currentPin = pin;
    public void ClearPin() => currentPin = null;

    public void SetHighlight(bool active)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = active;
    }
}