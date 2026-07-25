using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Narrative/Dialogue Sequence")]
public class DialogueSequenceSO : ScriptableObject
{
    public List<DialogueEntry> entries = new List<DialogueEntry>();

    [Header("Default Settings")]
    [Tooltip("Karakter baþýna bekleme süresi (saniye)")]
    public float defaultCharDelay = 0.04f;

    [Tooltip("Metin bittikten sonra otomatik geçiþ süresi (0 = manuel devam)")]
    public float defaultHoldAfter = 1.5f;
}

[System.Serializable]
public class DialogueEntry
{
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Bu metin için özel karakter hýzý (0 = default kullan)")]
    public float charDelayOverride = 0f;

    [Tooltip("Bu metin için özel bekleme süresi (0 = default kullan)")]
    public float holdAfterOverride = 0f;

    [Tooltip("Metin bittikten sonra týklama beklesin mi? (varsa hold süresi göz ardý edilir)")]
    public bool waitForInput = false;
}