using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperationButton : MonoBehaviour
{
    [SerializeField] private PinOperationSO operation;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;

    void Start()
    {
        if (iconImage != null && operation.icon != null) iconImage.sprite = operation.icon;
        if (labelText != null) labelText.text = operation.operationName;

        button.onClick.AddListener(OnClick);
        PinSelectionManager.Instance.OnSelectionChanged += RefreshInteractable;
        RefreshInteractable(PinSelectionManager.Instance.SelectedPins);
    }

    void OnDestroy()
    {
        if (PinSelectionManager.Instance != null)
            PinSelectionManager.Instance.OnSelectionChanged -= RefreshInteractable;
    }

    void OnClick()
    {
        PinSelectionManager.Instance.TryExecute(operation);
    }

    void RefreshInteractable(IReadOnlyList<PinController> selection)
    {
        button.interactable = operation.CanExecute(new System.Collections.Generic.List<PinController>(selection));
    }
}