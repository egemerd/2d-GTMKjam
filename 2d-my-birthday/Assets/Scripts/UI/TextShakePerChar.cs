using UnityEngine;
using TMPro;

public class TextShakePerChar : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float strength = 2f;    // piksel cinsinden max sapma
    [SerializeField] private float frequency = 25f;  // titreme hýzý
    [SerializeField] private bool shakeOnStart = true;

    private bool isShaking = false;
    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (shakeOnStart) StartShake();
    }

    public void StartShake()
    {
        text.ForceMeshUpdate();
        textInfo = text.textInfo;

        // Orijinal vertex pozisyonlarýný sakla (text deðiþtiðinde bunu yenile!)
        originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var verts = textInfo.meshInfo[i].vertices;
            originalVertices[i] = new Vector3[verts.Length];
            System.Array.Copy(verts, originalVertices[i], verts.Length);
        }

        isShaking = true;
    }

    public void StopShake()
    {
        isShaking = false;
        // Orijinal haline döndür
        if (originalVertices != null)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = originalVertices[i];
                text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }

    private void Update()
    {
        if (!isShaking || textInfo == null) return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            // Her karakter için farklý seed ile Perlin noise
            // (Random.Range her frame yeni deðer verir, jittery olur;
            //  Perlin noise ile daha smooth bir titreme elde ederiz)
            float t = Time.time * frequency;
            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(t, i) - 0.5f) * strength * 2f,
                (Mathf.PerlinNoise(i, t) - 0.5f) * strength * 2f,
                0f
            );

            var verts = textInfo.meshInfo[materialIndex].vertices;
            var orig = originalVertices[materialIndex];

            verts[vertexIndex + 0] = orig[vertexIndex + 0] + offset;
            verts[vertexIndex + 1] = orig[vertexIndex + 1] + offset;
            verts[vertexIndex + 2] = orig[vertexIndex + 2] + offset;
            verts[vertexIndex + 3] = orig[vertexIndex + 3] + offset;
        }

        // Mesh'i güncelle
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}