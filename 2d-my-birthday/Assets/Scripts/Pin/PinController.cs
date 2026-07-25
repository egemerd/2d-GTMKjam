using UnityEngine;
using DG.Tweening;

public class PinController : MonoBehaviour
{
    [SerializeField] private PinValue pinValue;

    [Header("Visual Groups")]
    [SerializeField] private GameObject pinVisualGroup;
    [SerializeField] private GameObject calendarVisualGroup;

    [Header("Pin Mode Highlights")]
    [SerializeField] private GameObject pinSelectionHighlight;
    [SerializeField] private GameObject pinHoverHighlight;

    [Header("Calendar Mode Highlights")]
    [SerializeField] private GameObject calendarSelectionHighlight;
    [SerializeField] private GameObject calendarHoverHighlight;
    public int Value => pinValue.Value;
    public bool IsSelected { get; private set; }
    public bool IsCalendarMode { get; private set; } = false;

    void Start()
    {
        // Baþlangýç: pin modu, tüm highlight'lar kapalý
        SetCalendarMode(false);

        if (pinSelectionHighlight != null) pinSelectionHighlight.SetActive(false);
        if (pinHoverHighlight != null) pinHoverHighlight.SetActive(false);
        if (calendarSelectionHighlight != null) calendarSelectionHighlight.SetActive(false);
        if (calendarHoverHighlight != null) calendarHoverHighlight.SetActive(false);
    }

    public void SetValue(int newValue)
    {
        pinValue.Value = newValue;
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f);

        if (newValue <= 0)
        {
            Debug.Log("[Pin] Deðer 0 veya altýna indi, pin yok oluyor.");
            Consume();
        }
    }

    public void SetCalendarMode(bool calendar)
    {
        IsCalendarMode = calendar;

        if (pinVisualGroup != null) pinVisualGroup.SetActive(!calendar);
        if (calendarVisualGroup != null) calendarVisualGroup.SetActive(calendar);

        // Aktif olmayan grubun highlight'larýný da kapat (kalýntý görünüm engeli)
        if (calendar)
        {
            if (pinSelectionHighlight != null) pinSelectionHighlight.SetActive(false);
            if (pinHoverHighlight != null) pinHoverHighlight.SetActive(false);
        }
        else
        {
            if (calendarSelectionHighlight != null) calendarSelectionHighlight.SetActive(false);
            if (calendarHoverHighlight != null) calendarHoverHighlight.SetActive(false);
        }

        // Mod deðiþince mevcut seçim durumunu yeni set'e uygula
        ApplySelectedVisual();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        ApplySelectedVisual();
    }

    void ApplySelectedVisual()
    {
        // Sadece aktif olan modun highlight'ýný yönet
        if (IsCalendarMode)
        {
            if (calendarSelectionHighlight != null)
                calendarSelectionHighlight.SetActive(IsSelected);
        }
        else
        {
            if (pinSelectionHighlight != null)
                pinSelectionHighlight.SetActive(IsSelected);
        }
    }

    public void SetHover(bool hovering)
    {
        // Seçiliyken hover göstermeyelim (görsel karmaþayý önlemek için)
        if (IsSelected) return;

        if (IsCalendarMode)
        {
            if (calendarHoverHighlight != null)
                calendarHoverHighlight.SetActive(hovering);
        }
        else
        {
            if (pinHoverHighlight != null)
                pinHoverHighlight.SetActive(hovering);
        }
    }

    public void Consume()
    {
        PinSelectionManager.Instance.Deselect(this);
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}