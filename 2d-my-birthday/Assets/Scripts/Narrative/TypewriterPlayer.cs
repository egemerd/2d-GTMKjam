using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System;

public class TypewriterPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private DialogueSequenceSO sequence;

    [Header("Skip Input")]
    [SerializeField] private bool allowSkip = true;

    private Coroutine playRoutine;
    private bool isTypingCurrentLine;
    private bool skipCurrentLineRequested;
    private bool skipAllRequested;

    public event Action OnSequenceComplete;

    void Start()
    {
        if (targetText != null) targetText.text = "";
    }

    void Update()
    {
        if (!allowSkip || playRoutine == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandleSkipInput();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            HandleSkipInput();

        // ESC ile tüm sequence'i atla
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            skipAllRequested = true;
            skipCurrentLineRequested = true;
        }
    }

    void HandleSkipInput()
    {
        if (isTypingCurrentLine)
        {
            // Şu an yazılıyor → mevcut satırı hemen tamamla
            skipCurrentLineRequested = true;
        }
        else
        {
            // Satır zaten tamamlandı, holdAfter bekleniyor → bir sonraki satıra geç
            skipCurrentLineRequested = true;
        }
    }

    public void Play()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlaySequence());
    }

    public void Stop()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    IEnumerator PlaySequence()
    {
        if (sequence == null || sequence.entries.Count == 0)
        {
            OnSequenceComplete?.Invoke();
            yield break;
        }

        foreach (var entry in sequence.entries)
        {
            if (skipAllRequested) break;
            yield return TypeEntry(entry);
        }

        playRoutine = null;
        OnSequenceComplete?.Invoke();
    }

    IEnumerator TypeEntry(DialogueEntry entry)
    {
        float charDelay = entry.charDelayOverride > 0 ? entry.charDelayOverride : sequence.defaultCharDelay;
        float holdAfter = entry.holdAfterOverride > 0 ? entry.holdAfterOverride : sequence.defaultHoldAfter;

        targetText.text = "";
        isTypingCurrentLine = true;
        skipCurrentLineRequested = false;

        // Karakter karakter yaz
        for (int i = 0; i < entry.text.Length; i++)
        {
            if (skipCurrentLineRequested)
            {
                targetText.text = entry.text; // hemen tamamla
                break;
            }
            targetText.text += entry.text[i];
            yield return new WaitForSeconds(charDelay);
        }

        isTypingCurrentLine = false;
        skipCurrentLineRequested = false;

        // Skip all istendiyse hiç bekleme
        if (skipAllRequested) yield break;

        // waitForInput ise tıklama bekle, değilse hold süresi kadar bekle
        if (entry.waitForInput)
        {
            yield return new WaitUntil(() => skipCurrentLineRequested || skipAllRequested);
            skipCurrentLineRequested = false;
        }
        else
        {
            float t = 0f;
            while (t < holdAfter && !skipCurrentLineRequested && !skipAllRequested)
            {
                t += Time.deltaTime;
                yield return null;
            }
            skipCurrentLineRequested = false;
        }
    }
}