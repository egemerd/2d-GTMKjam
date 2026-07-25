using UnityEngine;
using TMPro;

public class TextShake : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Boil Settings")]
    [Tooltip("Saniyede kaç kez yeni pozisyon (8-12 arasý klasik animasyon hissi)")]
    [SerializeField] private float framesPerSecond = 10f;

    [Tooltip("Piksel cinsinden max sapma")]
    [SerializeField] private float strength = 2f;

    [Tooltip("Karakterleri hafifçe döndür (derece cinsinden)")]
    [SerializeField] private float rotationStrength = 3f;

    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;
    private float frameTimer;
    private bool initialized;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (text == null) text = GetComponent<TMP_Text>();

        text.ForceMeshUpdate();
        textInfo = text.textInfo;

        originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var verts = textInfo.meshInfo[i].vertices;
            originalVertices[i] = new Vector3[verts.Length];
            System.Array.Copy(verts, originalVertices[i], verts.Length);
        }

        initialized = true;
        frameTimer = 0f;
        ApplyBoilFrame(); // Ýlk frame'i hemen çiz
    }

    private void Update()
    {
        if (!initialized) return;

        // Text deðiþmiþse vertex'leri yenile
        if (text.havePropertiesChanged)
        {
            text.havePropertiesChanged = false;
            Initialize();
            return;
        }

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
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int matIndex = charInfo.materialReferenceIndex;

            var verts = textInfo.meshInfo[matIndex].vertices;
            var orig = originalVertices[matIndex];

            // Bu karakter için rastgele offset (her frame yeniden)
            Vector3 offset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f
            );

            // Karakterin merkezi etrafýnda hafif rotasyon
            Vector3 charCenter = (orig[vertexIndex + 0] + orig[vertexIndex + 2]) * 0.5f;
            float angle = Random.Range(-rotationStrength, rotationStrength);
            Quaternion rot = Quaternion.Euler(0, 0, angle);

            for (int v = 0; v < 4; v++)
            {
                Vector3 localPos = orig[vertexIndex + v] - charCenter;
                localPos = rot * localPos;
                verts[vertexIndex + v] = charCenter + localPos + offset;
            }
        }

        // Mesh'i güncelle
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}