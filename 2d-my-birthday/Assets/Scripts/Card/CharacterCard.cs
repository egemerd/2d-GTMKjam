using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CharacterCard : MonoBehaviour
{
    [Header("Positions (world space)")]
    [SerializeField] private Vector3 hiddenPosition;   // yarýsý görünür halde
    [SerializeField] private Vector3 hoverPosition;    // hover'da hafif kayma
    [SerializeField] private Vector3 revealedPosition; // týklayýnca tam açýk

    [Header("Tween Settings")]
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float revealDuration = 0.4f;

    [Header("References")]
    [SerializeField] private Collider2D col;
    [SerializeField] private PinDropValidatorSO dropValidator;
    [SerializeField] private LayerMask pinLayerMask;

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

        // HOVER (sadece revealed deðilken)
        if (!isRevealed)
        {
            if (mouseOverCard && !isHovering)
            {
                isHovering = true;
                transform.DOKill();
                transform.DOMove(hoverPosition, hoverDuration).SetEase(Ease.OutQuad);
            }
            else if (!mouseOverCard && isHovering)
            {
                isHovering = false;
                transform.DOKill();
                transform.DOMove(hiddenPosition, hoverDuration).SetEase(Ease.OutQuad);
            }
        }

        // TIKLAMA — reveal / geri kapatma
        if (Mouse.current.leftButton.wasPressedThisFrame && mouseOverCard)
        {
            ToggleReveal();
        }
    }

    void ToggleReveal()
    {
        isRevealed = !isRevealed;
        transform.DOKill();
        Vector3 target = isRevealed ? revealedPosition : hiddenPosition;
        transform.DOMove(target, revealDuration).SetEase(Ease.OutBack);

        if (!isRevealed) isHovering = false;
    }

    // DragDrop bu fonksiyonu çaðýracak — pin karta býrakýldýðýnda
    public bool TryAcceptPin(PinController pin)
    {
        if (!isRevealed) return false; // sadece açýkken kabul et

        bool valid = dropValidator != null && dropValidator.Validate(pin.Value);

        if (valid)
        {
            Debug.Log($"[Card] OK — pin deðeri {pin.Value} yaþa uyuyor!");
            // TODO: doðru sonuç davranýþý buraya
        }
        else
        {
            Debug.Log($"[Card] YANLIÞ — pin deðeri {pin.Value}, hedef yaþa uymuyor.");
            // TODO: yanlýþ sonuç davranýþý buraya
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