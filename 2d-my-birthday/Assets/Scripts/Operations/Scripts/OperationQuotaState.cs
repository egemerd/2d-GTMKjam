using System;
using System.Collections.Generic;
using UnityEngine;
using static CalendarLevelData;

[CreateAssetMenu(fileName = "OperationQuotaState", menuName = "Calendar/Runtime/Operation Quota State")]
public class OperationQuotaState : ScriptableObject
{
    [System.NonSerialized] private Dictionary<PinOperationSO, int> remainingUses = new Dictionary<PinOperationSO, int>();

    public event Action OnQuotaChanged;

    public void Initialize(List<OperationQuota> quotas)
    {
        remainingUses.Clear();
        foreach (var q in quotas)
        {
            if (q.operation != null)
                remainingUses[q.operation] = q.uses;
        }
        OnQuotaChanged?.Invoke();
    }

    public int GetRemaining(PinOperationSO operation)
    {
        if (operation == null) return 0;
        return remainingUses.TryGetValue(operation, out int uses) ? uses : 0;
    }

    public bool HasUsesLeft(PinOperationSO operation) => GetRemaining(operation) > 0;

    public bool ConsumeUse(PinOperationSO operation)
    {
        if (!HasUsesLeft(operation)) return false;
        remainingUses[operation]--;
        Debug.Log($"[Quota] {operation.operationName} kullanýldý. Kalan: {remainingUses[operation]}");
        OnQuotaChanged?.Invoke();
        return true;
    }

    public bool IsOperationAvailable(PinOperationSO operation)
    {
        // Level'da hiç yoksa (dictionary'de key yoksa) false
        return remainingUses.ContainsKey(operation);
    }
}