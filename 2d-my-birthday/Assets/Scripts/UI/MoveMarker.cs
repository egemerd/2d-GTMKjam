using UnityEngine;
using DG.Tweening;

public class MoveMarker : MonoBehaviour
{
    [SerializeField] private float animateDuration = 0.35f;
    [SerializeField] private Vector3 finalScale = Vector3.one;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    void Start()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(finalScale, animateDuration).SetEase(scaleEase);
    }
}