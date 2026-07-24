using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CharacterCard : MonoBehaviour
{
    [Header("Positions (world space)")]
    [SerializeField] private Vector3 hiddenPosition;   // yarısı görünür halde
    [SerializeField] private Vector3 hoverPosition;    // hover'da hafif kayma
    [SerializeField] private Vector3 revealedPosition; // tıklayınca tam açık

    [Header("Tween Settings")]
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float revealDuration = 0.4f;

    [Header("References")]
    [SerializeField] private Collider2D col;
    [SerializeField] private PinDropValidatorSO dropValidator;
    [SerializeField] private LayerMask pinLayerMaskForBlock;

    [Header("Pin Area")]
    [SerializeField] private Transform pinArea; 

    private Camera cam;
    private bool isRevealed = false;
    private bool isHovering = false;

    void Start()
    {
        cam = Camera.main;
        transform.position = hiddenPosition;
    }

    void Update()
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        bool mouseOverCard = col.OverlapPoint(mouseWorld);

        // Mouse şu an bir pin'in üzerinde mi? (kartın da üzerinde olsa bile pin öncelik alır)
        bool mouseOverPin = Physics2D.OverlapPoint(mouseWorld, pinLayerMaskForBlock) != null;

        // HOVER (sadece revealed değilken ve mouse bir pin üzerinde değilken)
        if (!isRevealed)
        {
            if (mouseOverCard && !mouseOverPin && !isHovering)
            {
                isHovering = true;
                transform.DOKill();
                transform.DOMove(hoverPosition, hoverDuration).SetEase(Ease.OutQuad);
            }
            else if ((!mouseOverCard || mouseOverPin) && isHovering)
            {
                isHovering = false;
                transform.DOKill();
                transform.DOMove(hiddenPosition, hoverDuration).SetEase(Ease.OutQuad);
            }
        }

        // TIKLAMA — reveal/close (mouse bir pin üzerinde değilse)
        if (Mouse.current.leftButton.wasPressedThisFrame && mouseOverCard && !mouseOverPin)
        {
            ToggleReveal();
        }
    }

    public Transform GetPinArea()
    {
        return pinArea;
    }
    void ToggleReveal()
    {
        isRevealed = !isRevealed;
        transform.DOKill();
        Vector3 target = isRevealed ? revealedPosition : hiddenPosition;
        transform.DOMove(target, revealDuration).SetEase(Ease.OutBack);

        if (!isRevealed) isHovering = false;
    }

    // DragDrop bu fonksiyonu çağıracak — pin karta bırakıldığında
    public bool TryAcceptPin(PinController pin)
    {
        Debug.Log($"[Card] TryAcceptPin çağrıldı — pin değeri: {pin.Value}, kart açık mı: {isRevealed}");

        if (!isRevealed)
        {
            Debug.Log("[Card] Kart kapalı, pin kabul edilmedi.");
            return false;
        }

        if (dropValidator == null)
        {
            Debug.LogError("[Card] Drop Validator ATANMAMIŞ! Inspector'dan AgeValidator asset'ini sürüklemen lazım.");
            return false;
        }

        bool valid = dropValidator.Validate(pin.Value);

        if (valid)
        {
            Debug.Log($"<color=lime>[Card] ✓ BAŞARILI — pin değeri {pin.Value} hedef yaşa eşit!</color>");
        }
        else
        {
            Debug.Log($"<color=orange>[Card] ✗ BAŞARISIZ — pin değeri {pin.Value} hedef yaşa uymuyor.</color>");
        }

        return valid;
    }

    public bool IsPointerOverCard(Vector3 worldPos) => col.OverlapPoint(worldPos);

    Vector3 GetMouseWorldPos()
    {
        Vector2 sp = Mouse.current.position.ReadValue();
        Vector3 mp = new Vector3(sp.x, sp.y, -cam.transform.position.z);
        return cam.ScreenToWorldPoint(mp);
    }
}