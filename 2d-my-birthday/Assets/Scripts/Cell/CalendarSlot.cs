using UnityEngine;

public class CalendarSlot : MonoBehaviour
{
    public int dayNumber;
    public CellModifier modifier;
    [HideInInspector] public DragDrop currentPin;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color weekendColor = new Color(0.9f, 0.3f, 0.3f); // kýrmýzý
    [SerializeField] private Color skippedColor = new Color(1f, 0.9f, 0.2f);   // sarý
    [SerializeField] private Color blockedColor = new Color(0.3f, 0.3f, 0.3f); // gri

    public bool IsOccupied => currentPin != null;

    public bool CanAcceptPin()
    {
        if (IsOccupied) return false;
        if (modifier.HasFlag(CellModifier.Blocked)) return false;
        if (modifier.HasFlag(CellModifier.Weekend)) return false;
        if (modifier.HasFlag(CellModifier.Skipped)) return false;
        return true;
    }

    public void AssignPin(DragDrop pin) => currentPin = pin;
    public void ClearPin() => currentPin = null;

    public void ApplyVisuals()
    {
        if (mainRenderer == null) return;

        // Öncelik sýrasý: Weekend > Skipped > Blocked > Normal
        if (modifier.HasFlag(CellModifier.Weekend)) mainRenderer.color = weekendColor;
        else if (modifier.HasFlag(CellModifier.Skipped)) mainRenderer.color = skippedColor;
        else if (modifier.HasFlag(CellModifier.Blocked)) mainRenderer.color = blockedColor;
        else mainRenderer.color = normalColor;
    }
}