using UnityEngine;
using System.Collections.Generic;

public class PinSelectionManager : MonoBehaviour
{
    public static PinSelectionManager Instance { get; private set; }

    private List<PinController> selectedPins = new List<PinController>();
    public IReadOnlyList<PinController> SelectedPins => selectedPins;

    public System.Action<IReadOnlyList<PinController>> OnSelectionChanged;

    [SerializeField] private MovesState movesState;
    [SerializeField] private OperationQuotaState quotaState;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ToggleSelect(PinController pin)
    {
        if (selectedPins.Contains(pin))
        {
            selectedPins.Remove(pin);
            pin.SetSelected(false);
        }
        else
        {
            selectedPins.Add(pin); // ekleme sýrasý ÖNEMLÝ (subtract, merge için)
            pin.SetSelected(true);
        }
        OnSelectionChanged?.Invoke(selectedPins);
    }

    public void Deselect(PinController pin)
    {
        if (selectedPins.Remove(pin))
        {
            pin.SetSelected(false);
            OnSelectionChanged?.Invoke(selectedPins);
        }
    }

    public void ClearAll()
    {
        foreach (var p in selectedPins) p.SetSelected(false);
        selectedPins.Clear();
        OnSelectionChanged?.Invoke(selectedPins);
    }

    public bool TryExecute(PinOperationSO operation)
    {
        if (!operation.CanExecute(selectedPins)) return false;

        // Quota kontrolü
        if (quotaState != null && !quotaState.HasUsesLeft(operation))
        {
            Debug.Log($"[Selection] {operation.operationName} hakký bitti.");
            return false;
        }

        // Hamle kontrolü
        if (movesState != null && movesState.currentMoves <= 0)
        {
            return false;
        }

        var pinsCopy = new List<PinController>(selectedPins);
        operation.Execute(pinsCopy);
        ClearAll();

        // Quota harca
        quotaState?.ConsumeUse(operation);

        // Hamle harca
        movesState?.ConsumeMove();
        MoveMarkerManager.Instance?.PlaceMarker();

        return true;
    }
}