using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DragDrop : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool dragging;
    private bool coasting; // bırakıldıktan sonraki devamlılık fazı
    private Vector3 dragTargetPos;

    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Collider2D col;
    [SerializeField] private LayerMask slotLayerMask;

    [Header("Scale Feedback")]
    [SerializeField] private float dragScaleMultiplier = 1.15f;
    [SerializeField] private float scaleTweenDuration = 0.12f;
    private Vector3 originalScale;

    [Header("Start Settings")]
    [SerializeField] private Vector3 startScaleScaleDown;
    [SerializeField] private Vector3 startScaleOriginal;
    [SerializeField] private float grownScaleTweenDuration = 0.25f;
    private bool hasBeenPickedUp = false;

    [Header("Momentum / Inertia")]
    [SerializeField] private float momentumDrag = 4f;       // sönümleme hızı (yüksek = daha çabuk durur)
    [SerializeField] private float maxMomentumSpeed = 15f;  // hız sınırı
    private Vector3 velocity;
    private Vector3 prevPos;

    [Header("Click vs Drag")]
    [SerializeField] private float dragThreshold = 0.1f; // world unit, bu kadar hareket edince drag olur
    private Vector3 pressStartPos;
    private bool pressed = false;
    private bool dragStarted = false;
    private PinController pinController;

    [Header("Screen Boundaries")]
    [SerializeField] private float boundaryPadding = 0.3f;
    [SerializeField][Range(0f, 1f)] private float bounceEnergyRetention = 0.7f; // çarpma sonrası hızın % kaçı kalır
    [SerializeField] private float minBounceSpeed = 1f; // bu hızın altında sekme, sadece dur
    private Vector2 screenMin, screenMax;

    private int originalSortingOrder;
    private Vector3 lastValidPos;
    private CalendarSlot currentSlot;

    private static DragDrop currentlyDragging = null;
    void Start()
    {
        cam = Camera.main;
        transform.localScale = startScaleScaleDown;
        originalScale = startScaleOriginal;
        lastValidPos = transform.position;
        if (col == null) col = GetComponent<Collider2D>();
        pinController = GetComponent<PinController>();
        CalculateScreenBounds();
    }

    void CalculateScreenBounds()
    {
        // Ekranın alt-sol ve üst-sağ köşelerini world space'e çevir
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -cam.transform.position.z));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -cam.transform.position.z));

        screenMin = new Vector2(bottomLeft.x + boundaryPadding, bottomLeft.y + boundaryPadding);
        screenMax = new Vector2(topRight.x - boundaryPadding, topRight.y - boundaryPadding);
    }

    void Update()
    {
        if (!Mouse.current.leftButton.isPressed)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            bool isHovering = col.OverlapPoint(mouseWorld);
            if (pinController != null) pinController.SetHover(isHovering);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyDragging == null)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            if (col.OverlapPoint(mouseWorld))
            {
                pressed = true;
                dragStarted = false;
                pressStartPos = mouseWorld;
            }
        }

        if (pressed && !dragStarted && Mouse.current.leftButton.isPressed)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            if (Vector3.Distance(mouseWorld, pressStartPos) > dragThreshold)
            {
                dragStarted = true;
                StartDrag(mouseWorld);
            }
        }

        if (pressed && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (dragStarted)
            {
                EndDrag();
            }
            else
            {
                // Sürüklemedi, sadece tıkladı → SEÇİM
                PinSelectionManager.Instance.ToggleSelect(pinController);
            }
            pressed = false;
            dragStarted = false;
        }

        if (dragging)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            dragTargetPos = mouseWorld + offset;

            prevPos = transform.position;
            Vector3 newPos = Vector3.Lerp(transform.position, dragTargetPos, Time.deltaTime * followSpeed);

            // Sınır clamp'i — pin ekran dışına çıkamaz
            newPos.x = Mathf.Clamp(newPos.x, screenMin.x, screenMax.x);
            newPos.y = Mathf.Clamp(newPos.y, screenMin.y, screenMax.y);
            transform.position = newPos;

            velocity = (transform.position - prevPos) / Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxMomentumSpeed);

            float velX = (dragTargetPos.x - transform.position.x);
            float targetTilt = Mathf.Clamp(-velX * 15f, -12f, 12f);
            transform.rotation = Quaternion.Euler(0, 0,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetTilt, Time.deltaTime * 10f));
        }
        else if (coasting)
        {
            Vector3 nextPos = transform.position + velocity * Time.deltaTime;

            // X ekseninde sınıra çarptı mı?
            if (nextPos.x < screenMin.x)
            {
                nextPos.x = screenMin.x + (screenMin.x - nextPos.x); // sınırın içinden yansıt
                velocity.x = -velocity.x * bounceEnergyRetention;
                OnBounce();
            }
            else if (nextPos.x > screenMax.x)
            {
                nextPos.x = screenMax.x - (nextPos.x - screenMax.x);
                velocity.x = -velocity.x * bounceEnergyRetention;
                OnBounce();
            }

            // Y ekseninde sınıra çarptı mı?
            if (nextPos.y < screenMin.y)
            {
                nextPos.y = screenMin.y + (screenMin.y - nextPos.y);
                velocity.y = -velocity.y * bounceEnergyRetention;
                OnBounce();
            }
            else if (nextPos.y > screenMax.y)
            {
                nextPos.y = screenMax.y - (nextPos.y - screenMax.y);
                velocity.y = -velocity.y * bounceEnergyRetention;
                OnBounce();
            }

            transform.position = nextPos;

            // Sürtünme — pin yavaşça yavaşlasın
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * momentumDrag);

            // Tilt yumuşakça sıfıra
            transform.rotation = Quaternion.Euler(0, 0,
                Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0f, Time.deltaTime * 8f));

            if (velocity.magnitude < 0.05f)
            {
                coasting = false;
                transform.rotation = Quaternion.identity;
            }
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame) EndDrag();
    }

    void StartDrag(Vector3 mouseWorld)
    {
        dragging = true;
        coasting = false; // yeni drag başlarsa momentum'u iptal et
        currentlyDragging = this;
        offset = transform.position - mouseWorld;
        originalSortingOrder = sr.sortingOrder;
        //sr.sortingOrder = 100;

        if (currentSlot != null) currentSlot.ClearPin();

        // Sadece scale tween'ini kill et, transform.DOKill(true) tüm tweenleri öldürüyor
        // ama scale zaten burada yeniden başlatılacak, position tween yoksa problem yok
        transform.DOKill(true);

        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;
            transform.DOScale(originalScale * dragScaleMultiplier, grownScaleTweenDuration)
                .SetEase(Ease.OutBack);
        }
        else
        {
            transform.DOScale(originalScale * dragScaleMultiplier, scaleTweenDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    void OnBounce()
    {
        // Sadece scale tween'ini kill et
        DOTween.Kill(transform, "scale");
        transform.DOPunchScale(Vector3.one * -0.06f, 0.15f, 4, 0.5f).SetId("scale");
    }

    void EndDrag()
    {
        dragging = false;
        //sr.sortingOrder = originalSortingOrder;
        currentlyDragging = null;
        // Scale tween'i (bırakınca eski boyutuna dönme) başlat
        // NOT: transform.DOKill() burada YOK — sadece kendi scale tween'imi yönetiyorum,
        // aksi halde momentum için gereken velocity uygulaması sırasında sorun olmaz
        transform.DOScale(originalScale, scaleTweenDuration).SetEase(Ease.OutQuad);

        // Snap denenmiyorsa (şu an TrySnapToSlot kapalı) → momentum devreye girsin
        // Snap açtığında: snap başarılıysa coasting = false kalmalı (aşağıdaki nota bak)
        coasting = true;

        //TrySnapToSlot();
    }

    bool TrySnapToSlot()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position, slotLayerMask);
        if (hit != null && hit.TryGetComponent(out CalendarSlot slot) && slot.CanAcceptPin())
        {
            coasting = false; // snap oluyorsa momentum durmalı, çakışma engellenir
            transform.DOMove(slot.transform.position, 0.18f).SetEase(Ease.OutBack);
            slot.AssignPin(this);
            currentSlot = slot;
            lastValidPos = slot.transform.position;
            return true;
        }
        else
        {
            coasting = false; // reject animasyonu için de momentum durmalı
            transform.DOMove(lastValidPos, 0.2f).SetEase(Ease.OutBack);
            transform.DOPunchPosition(Vector3.right * 0.1f, 0.2f, 8);
            if (currentSlot != null) currentSlot.AssignPin(this);
            return false;
        }
    }

    

    public void SetHomeSlot(CalendarSlot slot)
    {
        currentSlot = slot;
        lastValidPos = slot.transform.position;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mp = new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z);
        return cam.ScreenToWorldPoint(mp);
    }
}