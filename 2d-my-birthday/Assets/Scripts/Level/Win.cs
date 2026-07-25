using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Win : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelState levelState;
    [SerializeField] private ParticleSystem winParticleSystem;

    [Header("Win Text")]
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Vector3 winTextEndScale = Vector3.one;

    [Header("White Flash Overlay")]
    [SerializeField] private CanvasGroup whiteOverlay;
    [SerializeField] private CanvasGroup startImage;


    [Header("Timing")]
    [SerializeField] private float particleDelay = 0.6f;      // particle sonrasý text'e geçiþ
    [SerializeField] private float textPunchDuration = 0.5f;  // text'in scale punch süresi
    [SerializeField] private float textHoldDuration = 1.2f;   // text ekranda ne kadar dursun
    [SerializeField] private float whiteFadeDuration = 0.6f;  // beyazýn büyümesi
    [SerializeField] private float postWhiteDelay = 0.3f;     // beyaz tam kapladýktan sonra bekleme

    [Header("Audio")]
    [SerializeField] private SoundSO confettiSound;

    private void OnEnable()
    {
        if (levelState != null) levelState.OnLevelWon += HandleLevelWon;
    }

    private void OnDisable()
    {
        if (levelState != null) levelState.OnLevelWon -= HandleLevelWon;
    }

    private void Awake()
    {
        StartCoroutine(LevelStartAnimation());
    }
    private void Start()
    {
        // Baþlangýç durumlarý
        if (winText != null)
        {
            winText.transform.localScale = Vector3.zero;
            winText.gameObject.SetActive(false);
        }

        if (whiteOverlay != null)
        {
            whiteOverlay.alpha = 0f;
            whiteOverlay.transform.localScale = Vector3.zero;
            whiteOverlay.gameObject.SetActive(false);
        }
    }

    private void HandleLevelWon()
    {
        StartCoroutine(LevelWonCoroutine());
    }

    private IEnumerator LevelWonCoroutine()
    {
        // 1. Particle patlamasý
        if (winParticleSystem != null) winParticleSystem.Play();
        if (confettiSound != null && AudioManager.Instance != null)
            AudioManager.Instance.Play(confettiSound);
        yield return new WaitForSeconds(particleDelay);

        // 2. Text scale punch (yukarýdan aþaðý)
        yield return AnimateWinText();
        yield return new WaitForSeconds(textHoldDuration);

        // 3. Yuvarlak beyaz kapanma (ekran beyazlar)
        yield return AnimateWhiteFlash();
        yield return new WaitForSeconds(postWhiteDelay);

        // 4. Sonraki level'a geçiþ
        LevelTransitionManager.Instance?.LoadNextLevel();
    }

    private IEnumerator AnimateWinText()
    {
        if (winText == null) yield break;

        winText.gameObject.SetActive(true);
        winText.transform.localScale = new Vector3(winTextEndScale.x, 0f, winTextEndScale.z);
        // Y ekseninden baþlýyor, üstten aþaðý doðru açýlýyor gibi hissettirmek için

        Sequence seq = DOTween.Sequence();
        seq.Append(winText.transform.DOScaleY(winTextEndScale.y * 1.25f, textPunchDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Join(winText.transform.DOScaleX(winTextEndScale.x * 1.15f, textPunchDuration * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(winText.transform.DOScale(winTextEndScale, textPunchDuration * 0.5f).SetEase(Ease.OutBounce));

        yield return seq.WaitForCompletion();
    }

    private IEnumerator AnimateWhiteFlash()
    {
        if (whiteOverlay == null) yield break;

        whiteOverlay.gameObject.SetActive(true);
        whiteOverlay.transform.localScale = Vector3.zero;
        whiteOverlay.alpha = 1f;

        // Yuvarlak scale up — merkezden ekraný kaplayana kadar
        // Deðer büyük çünkü ekraný tamamen kaplamasý gerekiyor
        Tween scaleTween = whiteOverlay.transform.DOScale(Vector3.one * 20f, whiteFadeDuration).SetEase(Ease.InQuad);

        yield return scaleTween.WaitForCompletion();
    }

    private IEnumerator LevelStartAnimation()
    {
        if (startImage == null) yield break;

        startImage.gameObject.SetActive(true);
        startImage.alpha = 1f;

        // Yuvarlak scale up — merkezden ekraný kaplayana kadar
        // Deðer büyük çünkü ekraný tamamen kaplamasý gerekiyor
        Tween scaleTween = startImage.transform.DOScale(Vector3.zero, whiteFadeDuration).SetEase(Ease.InQuad);
        yield return scaleTween.WaitForCompletion();
    }
}