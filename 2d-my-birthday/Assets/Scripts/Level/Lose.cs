using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Lose : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelState levelState;
    [SerializeField] private ParticleSystem loseParticleSystem;

    [Header("Lose Text")]
    [SerializeField] private TMP_Text loseText;
    [SerializeField] private Vector3 loseTextEndScale = Vector3.one;
    [Tooltip("Rastgele seçilecek lose metinleri")]
    [SerializeField] private string[] loseMessages;

    [Header("Lose Image Overlay")]
    [SerializeField] private Image loseOverlayImage;
    [SerializeField] private CanvasGroup loseOverlayCanvasGroup;

    [Header("Timing")]
    [SerializeField] private float particleDelay = 0.6f;
    [SerializeField] private float textPunchDuration = 0.5f;
    [SerializeField] private float textHoldDuration = 1.5f;
    [SerializeField] private float overlayFadeDuration = 0.6f;
    [SerializeField] private float postOverlayDelay = 0.3f;

    private void OnEnable()
    {
        if (levelState != null) levelState.OnLevelLost += HandleLevelLost;
    }

    private void OnDisable()
    {
        if (levelState != null) levelState.OnLevelLost -= HandleLevelLost;
    }

    private void Start()
    {
        // Baþlangýç durumlarý
        if (loseText != null)
        {
            loseText.transform.localScale = Vector3.zero;
            loseText.gameObject.SetActive(false);
        }

        if (loseOverlayCanvasGroup != null)
        {
            loseOverlayCanvasGroup.alpha = 0f;
            loseOverlayCanvasGroup.transform.localScale = Vector3.zero;
            loseOverlayCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void HandleLevelLost()
    {
        StartCoroutine(LevelLostCoroutine());
    }

    private IEnumerator LevelLostCoroutine()
    {
        // 1. Particle (varsa)
        if (loseParticleSystem != null) loseParticleSystem.Play();
        yield return new WaitForSeconds(particleDelay);

        // 2. Rastgele mesaj seç ve text'i punch ile göster
        SelectRandomMessage();
        yield return AnimateLoseText();
        yield return new WaitForSeconds(textHoldDuration);

        // 3. Overlay büyüme (windeki beyaz gibi ama farklý görsel/renkte olabilir)
        yield return AnimateOverlay();
        yield return new WaitForSeconds(postOverlayDelay);

        // 4. Level'ý yeniden baþlat (veya lose menüsüne git — tercihine göre)
        LevelTransitionManager.Instance?.RestartCurrentLevel();
    }

    private void SelectRandomMessage()
    {
        if (loseText == null) return;

        if (loseMessages == null || loseMessages.Length == 0)
        {
            loseText.text = "You Lose";
            Debug.LogWarning("[Lose] loseMessages boþ, varsayýlan metin kullanýlýyor.");
            return;
        }

        int randomIndex = Random.Range(0, loseMessages.Length);
        loseText.text = loseMessages[randomIndex];
        Debug.Log($"[Lose] Seçilen mesaj: {loseMessages[randomIndex]}");
    }

    private IEnumerator AnimateLoseText()
    {
        if (loseText == null) yield break;

        loseText.gameObject.SetActive(true);
        loseText.transform.localScale = new Vector3(loseTextEndScale.x, 0f, loseTextEndScale.z);

        // Win ile ayný "yukarýdan aþaðý" punch mantýðý
        Sequence seq = DOTween.Sequence();
        seq.Append(loseText.transform.DOScaleY(loseTextEndScale.y * 1.25f, textPunchDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Join(loseText.transform.DOScaleX(loseTextEndScale.x * 1.15f, textPunchDuration * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(loseText.transform.DOScale(loseTextEndScale, textPunchDuration * 0.5f).SetEase(Ease.OutBounce));

        yield return seq.WaitForCompletion();
    }

    private IEnumerator AnimateOverlay()
    {
        if (loseOverlayCanvasGroup == null) yield break;

        loseOverlayCanvasGroup.gameObject.SetActive(true);
        loseOverlayCanvasGroup.transform.localScale = Vector3.zero;
        loseOverlayCanvasGroup.alpha = 1f;

        Tween scaleTween = loseOverlayCanvasGroup.transform
            .DOScale(Vector3.one * 20f, overlayFadeDuration)
            .SetEase(Ease.InQuad);

        yield return scaleTween.WaitForCompletion();
    }
}