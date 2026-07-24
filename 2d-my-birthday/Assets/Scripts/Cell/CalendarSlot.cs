using UnityEngine;
using TMPro;

public class CalendarSlot : MonoBehaviour
{
    public int dayNumber;
    public CellModifier modifier;
    [HideInInspector] public DragDrop currentPin;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color weekendColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color skippedColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private Color blockedColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Day Number Text (aralýk dýþý hücreler için)")]
    [SerializeField] private TMP_Text dayNumberLabel;
    [SerializeField] private Color dayNumberColor = Color.black;

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
        if (mainRenderer != null)
        {
            if (modifier.HasFlag(CellModifier.Weekend)) mainRenderer.color = weekendColor;
            else if (modifier.HasFlag(CellModifier.Skipped)) mainRenderer.color = skippedColor;
            else if (modifier.HasFlag(CellModifier.Blocked)) mainRenderer.color = blockedColor;
            else mainRenderer.color = normalColor;
        }
    }

    // Bu slot'ta pin yoksa gün numarasýný göster; varsa gizle
    public void SetDayNumberVisible(bool visible)
    {
        if (dayNumberLabel == null) return;

        dayNumberLabel.gameObject.SetActive(visible);
        if (visible)
        {
            dayNumberLabel.text = dayNumber.ToString();
            dayNumberLabel.color = dayNumberColor;
        }
    }
}