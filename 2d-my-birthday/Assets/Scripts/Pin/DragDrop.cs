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

    [Header("Level State")]
    [SerializeField] private LevelState levelState;

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

    [Header("Sorting Order Management")]
    [SerializeField] private Renderer[] pinRenderers; // tüm sprite + text renderer'ları
    private int[] originalOrders;
    private const int DRAG_BOOST = 100;

    [Header("Pin-to-Pin Collision")]
    [SerializeField] private float collisionRadius = 0.5f;      // pin'in çarpışma yarıçapı
    [SerializeField] private float bounceForce = 3f;            // itme kuvveti
    [SerializeField] private float bounceRecoveryTime = 0.2f;   // squash animasyon süresi
    [SerializeField] private LayerMask pinLayerMask;            // sadece pin layer'ını tara

    private float collisionCooldown = 0f; // aynı frame'de tekrar tekrar tetiklenmesin
    private const float COLLISION_COOLDOWN_TIME = 0.15f;

    [Header("Screen Boundaries")]
    [SerializeField] private float boundaryPadding = 0.3f;
    [SerializeField][Range(0f, 1f)] private float bounceEnergyRetention = 0.7f; // çarpma sonrası hızın % kaçı kalır
    [SerializeField] private float minBounceSpeed = 1f; // bu hızın altında sekme, sadece dur
    private Vector2 screenMin, screenMax;

    [Header("Audio")]
    [SerializeField] private SoundSO bounceSound;
    [SerializeField] private SoundSO pickupSound;  // yeni
    [SerializeField] private SoundSO dropSound;
    [SerializeField] private SoundSO selectSound;

    private int originalSortingOrder;
    private Vector3 lastValidPos;
    private CalendarSlot currentSlot;
    private CharacterCard characterCard;
    private bool hasBeenReleasedOnce = false;
    private static DragDrop currentlyDragging = null;

    public void MarkAsPickedUp() => hasBeenPickedUp = true;

    private void Awake()
    {
        originalOrders = new int[pinRenderers.Length];
    }
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
        if ((dragging || coasting) && collisionCooldown <= 0f && hasBeenReleasedOnce)
        {
            CheckPinCollisions();
        }

        if (collisionCooldown > 0f)
            collisionCooldown -= Time.deltaTime;

        if (levelState != null && levelState.currentResult != LevelResult.InProgress) return;

        if (!Mouse.current.leftButton.isPressed)
        {
            Vector3 mouseWorld = GetMouseWorldPos();

            // En üstteki pini bul
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
            DragDrop topPin = null;
            int highestOrder = int.MinValue;

            foreach (var hit in hits)
            {
                var candidate = hit.GetComponent<DragDrop>();
                if (candidate == null) continue;

                var candidateSr = candidate.GetComponent<SpriteRenderer>()
                                  ?? candidate.GetComponentInChildren<SpriteRenderer>();
                int order = candidateSr != null ? candidateSr.sortingOrder : 0;

                if (order > highestOrder)
                {
                    highestOrder = order;
                    topPin = candidate;
                }
            }

            // Sadece en üstteki pin hover olsun, diğerleri hover'ı kapatsın
            bool isHovering = (topPin == this);
            if (pinController != null) pinController.SetHover(isHovering);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyDragging == null)
        {
            Vector3 mouseWorld = GetMouseWorldPos();

            // Aynı noktadaki tüm collider'ları bul
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

            // En üstteki pin'i sorting order'a göre bul
            DragDrop topPin = null;
            int highestOrder = int.MinValue;

            foreach (var hit in hits)
            {
                var candidate = hit.GetComponent<DragDrop>();
                if (candidate == null) continue;

                var candidateSr = candidate.GetComponent<SpriteRenderer>()
                                  ?? candidate.GetComponentInChildren<SpriteRenderer>();
                int order = candidateSr != null ? candidateSr.sortingOrder : 0;

                if (order > highestOrder)
                {
                    highestOrder = order;
                    topPin = candidate;
                }
            }

            // Sadece EN ÜSTTEKİ pin tıklamayı alsın
            if (topPin == this && col.OverlapPoint(mouseWorld))
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
                AudioManager.Instance?.Play(selectSound);
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

    public void SetCharacterCard(CharacterCard card)
    {
        characterCard = card;
    }

    void StartDrag(Vector3 mouseWorld)
    {
        dragging = true;
        coasting = false; // yeni drag başlarsa momentum'u iptal et
        currentlyDragging = this;
        offset = transform.position - mouseWorld;
        //originalSortingOrder = sr.sortingOrder;
        //sr.sortingOrder = 100;
        AudioManager.Instance?.Play(pickupSound);
        if (currentSlot != null) currentSlot.ClearPin();

        // Sadece scale tween'ini kill et, transform.DOKill(true) tüm tweenleri öldürüyor
        // ama scale zaten burada yeniden başlatılacak, position tween yoksa problem yok
        transform.DOKill(true);

        for (int i = 0; i < pinRenderers.Length; i++)
        {
            originalOrders[i] = pinRenderers[i].sortingOrder;
            pinRenderers[i].sortingOrder += DRAG_BOOST;
        }

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

        for (int i = 0; i < pinRenderers.Length; i++)
            pinRenderers[i].sortingOrder = originalOrders[i];

        // Scale tween'i (bırakınca eski boyutuna dönme) başlat
        // NOT: transform.DOKill() burada YOK — sadece kendi scale tween'imi yönetiyorum,
        // aksi halde momentum için gereken velocity uygulaması sırasında sorun olmaz
        transform.DOScale(originalScale, scaleTweenDuration).SetEase(Ease.OutQuad);
        hasBeenReleasedOnce = true;
        AudioManager.Instance?.Play(dropSound);
        if (TryDropOnCard())
        {
            // Karta bırakıldı, momentum başlatma
            return;
        }

        // Snap denenmiyorsa (şu an TrySnapToSlot kapalı) → momentum devreye girsin
        // Snap açtığında: snap başarılıysa coasting = false kalmalı (aşağıdaki nota bak)
        coasting = true;

        //TrySnapToSlot();
    }

    void CheckPinCollisions()
    {
        // Bu pin'in etrafındaki diğer collider'ları bul
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
            transform.position, collisionRadius, pinLayerMask);

        foreach (var other in nearbyColliders)
        {
            if (other == col) continue; // kendini atla

            var otherPin = other.GetComponent<DragDrop>();
            if (otherPin == null) continue;

            // İtme yönünü hesapla (diğer pinden bize doğru)
            Vector3 pushDirection = (transform.position - other.transform.position).normalized;
            if (pushDirection.sqrMagnitude < 0.001f)
                pushDirection = Random.insideUnitCircle.normalized; // aynı noktadalarsa rastgele yön

            // İki pin için de bounce tetikle
            ApplyBounce(pushDirection);
            otherPin.ApplyBounce(-pushDirection);

            collisionCooldown = COLLISION_COOLDOWN_TIME;
            break; // aynı frame'de birden fazla collision tetiklemesin
        }
    }

    public void ApplyBounce(Vector3 direction)
    {
        if (!hasBeenReleasedOnce) return;

        velocity += direction * bounceForce;
        velocity = Vector3.ClampMagnitude(velocity, maxMomentumSpeed);

        // ÖNCE tüm scale tween'lerini öldür
        transform.DOKill();

        // Şu an olması gereken doğru scale'i belirle
        Vector3 targetScale = dragging ? originalScale * dragScaleMultiplier : originalScale;

        // Scale'i doğru değere zorla (geçmişte yarım kalmış tween varsa temizle)
        transform.localScale = targetScale;

        // Punch uygula — hedef scale artık doğru olduğu için doğru değere geri dönecek
        transform.DOPunchScale(Vector3.one * -0.08f, bounceRecoveryTime, 6, 0.5f);

        AudioManager.Instance?.PlayAt(bounceSound, transform.position);

        if (!dragging && !coasting)
        {
            coasting = true;
        }
    }

    bool TryDropOnCard()
    {
        if (characterCard == null) return false;

        Vector3 mouseWorld = GetMouseWorldPos();
        if (!characterCard.IsPointerOverCard(mouseWorld)) return false;

        Debug.Log($"[DragDrop] Pin ({pinController.Value}) karta bırakıldı — validation başlıyor");
        bool accepted = characterCard.TryAcceptPin(pinController);

        // Kabul edilse de reddedilse de pin şu an eski pozisyonuna kayar
        if(!accepted)
        {
            transform.DOMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            levelState?.ReportLoss();
        }
        else
        {
            transform.DOMove(characterCard.GetPinArea().position, 0.25f).SetEase(Ease.OutBack);      
        }        
        
        coasting = false;
        return true;
    }

    public void ClearSlotReference()
    {
        if (currentSlot != null)
        {
            currentSlot.ClearPin();
            currentSlot = null;
        }
        lastValidPos = transform.position; // dağıldıkları yer artık yeni "ev"
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