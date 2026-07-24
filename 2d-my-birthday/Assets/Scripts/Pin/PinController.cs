using UnityEngine;
using DG.Tweening;

public class PinController : MonoBehaviour
{
    [SerializeField] private PinValue pinValue;
    [SerializeField] private SpriteRenderer selectionHighlight; // seçildiðinde açýlan sprite
    [SerializeField] private SpriteRenderer hoverHighlight;     // hover'da açýlan sprite

    public int Value => pinValue.Value;
    public bool IsSelected { get; private set; }

    void Start()
    {
        if (selectionHighlight != null) selectionHighlight.enabled = false;
        if (hoverHighlight != null) hoverHighlight.enabled = false;
    }

    public void SetValue(int newValue)
    {
        pinValue.Value = newValue;
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f); // deðer deðiþme feedback
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectionHighlight != null) selectionHighlight.enabled = selected;
    }

    public void SetHover(bool hovering)
    {
        // Seçiliyken hover gösterme (görsel karmaþayý önle)
        if (hoverHighlight != null && !IsSelected)
            hoverHighlight.enabled = hovering;
    }

    public void Consume()
    {
        // Merge/Add/Subtract'ta yok olan pin için
        PinSelectionManager.Instance.Deselect(this);
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}