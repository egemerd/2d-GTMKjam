using UnityEngine;
using DG.Tweening;

public class PinScatter : MonoBehaviour
{
    [Header("Scatter Area")]
    [SerializeField] private Vector2 scatterCenter = Vector2.zero;
    [SerializeField] private Vector2 scatterSize = new Vector2(5f, 3f);

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float scaleDuration = 0.4f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private float staggerBetweenPins = 0.05f;

    public void ScatterAllPins()
    {
        PinController[] allPins = FindObjectsOfType<PinController>();

        for (int i = 0; i < allPins.Length; i++)
        {
            Vector3 randomPos = GetRandomPositionInArea();
            float delay = i * staggerBetweenPins;

            var dragDrop = allPins[i].GetComponent<DragDrop>();
            if (dragDrop != null) dragDrop.ClearSlotReference();

            // Pozisyon animasyonu
            allPins[i].transform.DOMove(randomPos, moveDuration)
                .SetEase(moveEase)
                .SetDelay(delay);

            // Scale'i direkt 1'e büyüt
            allPins[i].transform.DOScale(Vector3.one, scaleDuration)
                .SetEase(scaleEase)
                .SetDelay(delay);
        }

        Debug.Log($"[PinScatter] {allPins.Length} pin daðýtýldý.");
    }

    Vector3 GetRandomPositionInArea()
    {
        float x = scatterCenter.x + Random.Range(-scatterSize.x / 2f, scatterSize.x / 2f);
        float y = scatterCenter.y + Random.Range(-scatterSize.y / 2f, scatterSize.y / 2f);
        return new Vector3(x, y, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireCube(scatterCenter, scatterSize);
    }
}