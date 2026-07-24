using UnityEngine;

public class MoveMarkerManager : MonoBehaviour
{
    public static MoveMarkerManager Instance { get; private set; }

    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private CalendarBuilder calendarBuilder;
    [SerializeField] private CalendarLevelData levelData;

    [Header("Placement")]
    [SerializeField] private Vector3 offsetOnSlot = Vector3.zero; // slot'un üstüne göre kaydırma
    [SerializeField] private int startingDayOffset = 0; // ilk X hangi günden başlasın (0 = startDay)

    private int nextMarkerDay;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        nextMarkerDay = levelData.startDay + startingDayOffset;
        //levelData=calendarBuilder.GetLevelData();
    }

    public void PlaceMarker()
    {
        // Aralık dışına çıkarsa dur
        if (nextMarkerDay > levelData.endDay)
        {
            Debug.Log("[MarkerManager] Aralık sonu, X konulamaz.");
            return;
        }

        CalendarSlot slot = calendarBuilder.GetSlot(nextMarkerDay);
        if (slot == null)
        {
            Debug.LogWarning($"[MarkerManager] Slot bulunamadı: gün {nextMarkerDay}");
            return;
        }

        Vector3 spawnPos = slot.transform.position + offsetOnSlot;
        Instantiate(markerPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"[MarkerManager] X konuldu → gün {nextMarkerDay}");
        nextMarkerDay++;
    }
}