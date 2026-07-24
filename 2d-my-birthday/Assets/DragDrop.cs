using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DragDrop : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool dragging;
    private bool coasting; // býrakýldýktan sonraki devamlýlýk fazý
    private Vector3 dragTargetPos;

    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Collider2D col;

    [Header("Momentum / Inertia")]
    [SerializeField] private float momentumDrag = 4f;      // sönümleme hýzý (yüksek = daha çabuk durur)
    [SerializeField] private float maxMomentumSpeed = 15f;  // hýz sýnýrý, aþýrý fýrlamayý engeller
    private Vector3 velocity;
    private Vector3 prevPos;

    [Header("Scale Feedback")]
    [SerializeField] private float dragScaleMultiplier = 1.15f;
    [SerializeField] private float scaleTweenDuration = 0.12f;
    private Vector3 originalScale;

    private int originalSortingOrder;
    private Vector3 lastValidPos;

    void Start()
    {
        cam = Camera.main;
        lastValidPos = transform.position;
        if (col == null) col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            if (col.OverlapPoint(mouseWorld))
            {
                StartDrag(mouseWorld);
            }
        }

        if (dragging)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            dragTargetPos = mouseWorld + offset;

            prevPos = transform.position;
            transform.position = Vector3.Lerp(transform.position, dragTargetPos, Time.deltaTime * followSpeed);

            // Anlýk hýzý hesapla (bir sonraki frame'de coasting için kullanýlacak)
            velocity = (transform.position - prevPos) / Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxMomentumSpeed);

            float velX = (dragTargetPos.x - transform.position.x);
            float targetTilt = Mathf.Clamp(-velX * 15f, -12f, 12f);
            transform.rotation = Quaternion.Euler(0, 0,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetTilt, Time.deltaTime * 10f));
        }
        else if (coasting)
        {
            // Býrakýldýktan sonra son hýzla devam edip yavaþça dur
            transform.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * momentumDrag);

            // Tilt de sýfýra dönsün
            transform.rotation = Quaternion.Euler(0, 0,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0f, Time.deltaTime * 8f));

            if (velocity.magnitude < 0.05f)
            {
                coasting = false;
                transform.rotation = Quaternion.identity;
            }
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    void StartDrag(Vector3 mouseWorld)
    {
        dragging = true;
        coasting = false; // yeni drag baþlarsa coasting'i iptal et
        offset = transform.position - mouseWorld;
        originalSortingOrder = sr.sortingOrder;
        //sr.sortingOrder = 100;

        transform.DOKill(true);
        transform.DOScale(originalScale * dragScaleMultiplier, scaleTweenDuration).SetEase(Ease.OutQuad);
    }

    void EndDrag()
    {
        dragging = false;
        coasting = true; // momentum fazýna geç
        //sr.sortingOrder = originalSortingOrder;

        //TrySnapToSlot();
        transform.DOScale(originalScale, scaleTweenDuration).SetEase(Ease.OutQuad);
    }

    Vector3 GetMouseWorldPos()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mp = new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z);
        return cam.ScreenToWorldPoint(mp);
    }
}