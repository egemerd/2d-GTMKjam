using UnityEngine;

public class CharacterBoil : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Boþ býrakýrsan bu transform kullanýlýr. Ýdeal olan: karakterin altýna bir 'VisualRoot' child koyup onu buraya baðlamak.")]
    [SerializeField] private Transform target;

    [Header("Boil Settings")]
    [Tooltip("Saniyede kaç kez yeni pozisyon (8-12 klasik animasyon hissi)")]
    [SerializeField] private float framesPerSecond = 10f;

    [Tooltip("Pozisyon sapmasý (world unit)")]
    [SerializeField] private float positionStrength = 0.03f;

    [Tooltip("Rotasyon sapmasý (derece)")]
    [SerializeField] private float rotationStrength = 2f;

    [Tooltip("Scale sapmasý (0.02 = ±%2)")]
    [SerializeField] private float scaleStrength = 0.02f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private float frameTimer;

    private void Awake()
    {
        if (target == null) target = transform;
        originalPosition = target.localPosition;
        originalRotation = target.localRotation;
        originalScale = target.localScale;
    }

    private void OnDisable()
    {
        // Kapatýldýðýnda orijinal haline dön (temiz state)
        target.localPosition = originalPosition;
        target.localRotation = originalRotation;
        target.localScale = originalScale;
    }

    private void Update()
    {
        frameTimer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            ApplyBoilFrame();
        }
    }

    private void ApplyBoilFrame()
    {
        // Pozisyon: orijinalin etrafýnda random offset
        Vector3 posOffset = new Vector3(
            Random.Range(-positionStrength, positionStrength),
            Random.Range(-positionStrength, positionStrength),
            0f
        );
        target.localPosition = originalPosition + posOffset;

        // Rotasyon: orijinalin etrafýnda random Z ekseni
        float angle = Random.Range(-rotationStrength, rotationStrength);
        target.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);

        // Scale: hafif "nefes"
        float scaleFactor = 1f + Random.Range(-scaleStrength, scaleStrength);
        target.localScale = originalScale * scaleFactor;
    }
}