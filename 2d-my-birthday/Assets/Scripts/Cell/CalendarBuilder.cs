using UnityEngine;

public class CalendarBuilder : MonoBehaviour
{
    [SerializeField] private CalendarLevelData levelData;
    [SerializeField] private CurrentLevelState levelState;
    [SerializeField] private CalendarSlot slotPrefab;
    [SerializeField] private Transform gridParent;

    [Header("Grid Yerleþimi")]
    [SerializeField] private float cellWidth = 1.2f;
    [SerializeField] private float cellHeight = 1.2f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [SerializeField] private CharacterCard characterCard;

    private CalendarSlot[] slots;

    void Start()
    {
        levelState.Initialize(levelData);
        BuildCalendar();
        SpawnCharacter();
    }

    void OnDestroy()
    {
        if (levelState != null) levelState.Clear();
    }

    public void BuildCalendar()
    {
        slots = new CalendarSlot[levelData.totalDays];

        for (int i = 0; i < levelData.totalDays; i++)
        {
            int dayNumber = i + 1;

            int gridIndex = i + levelData.startOffset;
            int row = gridIndex / levelData.columns;
            int col = gridIndex % levelData.columns;
            Vector3 pos = origin + new Vector3(col * cellWidth, -row * cellHeight, 0);

            CalendarSlot slot = Instantiate(slotPrefab, pos, Quaternion.identity, gridParent);
            slot.name = $"Day_{dayNumber}";
            slot.dayNumber = dayNumber;

            // Modifier'larý uygula
            CellModifier finalModifier = CellModifier.None;

            if (levelData.IsDayInRange(dayNumber))
            {
                if (levelData.IsWeekend(dayNumber)) finalModifier |= CellModifier.Weekend;
                if (levelData.IsSkipped(dayNumber)) finalModifier |= CellModifier.Skipped;
            }

            // Manuel override modifier'ý da varsa ekle
            CellData overrideData = levelData.GetCellOverride(dayNumber);
            if (overrideData != null) finalModifier |= overrideData.modifier;

            slot.modifier = finalModifier;
            slot.ApplyVisuals(); // yeni fonksiyon, aþaðýda

            slots[i] = slot;

            if (levelData.ShouldSpawnPin(dayNumber))
            {
                GameObject prefabToUse = overrideData?.pinPrefabOverride != null
                    ? overrideData.pinPrefabOverride
                    : levelData.defaultPinPrefab;

                GameObject pinObj = Instantiate(prefabToUse, pos, Quaternion.identity);
                if (pinObj.TryGetComponent(out DragDrop pin))
                {
                    slot.AssignPin(pin);
                    pin.SetHomeSlot(slot);
                    pin.SetCharacterCard(characterCard);
                }
                if (pinObj.TryGetComponent(out PinValue pinValue))
                {
                    pinValue.Value = dayNumber;
                }
            }
        }
    }

    void SpawnCharacter()
    {
        if (levelData.characterPrefab == null) return;

        GameObject charObj = Instantiate(
            levelData.characterPrefab,
            levelData.characterSpawnPosition,
            Quaternion.identity);

        if (charObj.TryGetComponent(out CharacterInfo info))
        {
            info.Setup(
                levelData.birthMonth,
                levelData.birthYear,
                levelData.currentYear);
        }
    }

    public CalendarSlot GetSlot(int dayNumber) =>
        (dayNumber < 1 || dayNumber > slots.Length) ? null : slots[dayNumber - 1];
}