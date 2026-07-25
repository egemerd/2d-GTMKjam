using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OperationButton : MonoBehaviour
{
    [SerializeField] private PinOperationSO operation;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text remainingLabel; // sayaç göstergesi
    [SerializeField] private OperationQuotaState quotaState;

    void Start()
    {
        if (iconImage != null && operation.icon != null) iconImage.sprite = operation.icon;
        if (labelText != null) labelText.text = operation.operationName;

        button.onClick.AddListener(OnClick);

        // Event'lere abone ol — quota Initialize olduğunda otomatik güncellenir
        if (PinSelectionManager.Instance != null)
            PinSelectionManager.Instance.OnSelectionChanged += Refresh;

        if (quotaState != null)
            quotaState.OnQuotaChanged += HandleQuotaChanged;

        // İlk render — quotaState hazırsa hemen değerlendir, değilse bekle
        HandleQuotaChanged();
    }

    void HandleQuotaChanged()
    {
        bool isAvailable = quotaState != null && quotaState.IsOperationAvailable(operation);
        int remaining = quotaState != null ? quotaState.GetRemaining(operation) : -999;

        Debug.Log($"[Button:{operation?.operationName}] Available: {isAvailable}, Remaining: {remaining}");

        gameObject.SetActive(isAvailable);

        if (isAvailable) Refresh();
    }

    void OnDestroy()
    {
        if (PinSelectionManager.Instance != null)
            PinSelectionManager.Instance.OnSelectionChanged -= Refresh;
        if (quotaState != null)
            quotaState.OnQuotaChanged -= HandleQuotaChanged;
    }

    void OnClick()
    {
        PinSelectionManager.Instance.TryExecute(operation);
    }

    void Refresh(System.Collections.Generic.IReadOnlyList<PinController> selection)
    {
        Refresh();
    }

    void Refresh()
    {
        bool canDoOperation = operation.CanExecute(
            new System.Collections.Generic.List<PinController>(PinSelectionManager.Instance.SelectedPins));

        bool hasUsesLeft = quotaState == null || quotaState.HasUsesLeft(operation);

        button.interactable = canDoOperation && hasUsesLeft;

        // Sayaç güncelle
        if (remainingLabel != null && quotaState != null)
        {
            int remaining = quotaState.GetRemaining(operation);
            remainingLabel.text = remaining.ToString();
        }
    }
}