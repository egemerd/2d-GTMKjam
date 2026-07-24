using UnityEditor;
using UnityEngine;

public class CalendarLevelEditorWindow : EditorWindow
{
    private CalendarLevelData levelData;
    private int selectedDay = -1;
    private Vector2 scroll;

    [MenuItem("Tools/Calendar/Level Editor")]
    public static void Open() => GetWindow<CalendarLevelEditorWindow>("Calendar Level Editor");

    void OnGUI()
    {
        levelData = (CalendarLevelData)EditorGUILayout.ObjectField(
            "Level Data", levelData, typeof(CalendarLevelData), false);

        if (levelData == null)
        {
            EditorGUILayout.HelpBox("Select a CalendarLevelData asset to edit.", MessageType.Info);
            return;
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Calendar Settings", EditorStyles.boldLabel);
        levelData.totalDays = EditorGUILayout.IntField("Total Days", levelData.totalDays);
        levelData.columns = EditorGUILayout.IntField("Column Count", levelData.columns);
        levelData.startOffset = EditorGUILayout.IntField("Start Offset", levelData.startOffset);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Pin Range", EditorStyles.boldLabel);
        levelData.startDay = EditorGUILayout.IntField("Start Day", levelData.startDay);
        levelData.endDay = EditorGUILayout.IntField("End Day", levelData.endDay);

        EditorGUILayout.Space(6);
        levelData.defaultPinPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Pin Prefab", levelData.defaultPinPrefab, typeof(GameObject), false);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Day Grid (click to edit)", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
        DrawDayGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(12);
        DrawSelectedDayInspector();

        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(levelData);
    }

    void DrawDayGrid()
    {
        int cols = Mathf.Max(1, levelData.columns);
        int totalCells = levelData.totalDays + levelData.startOffset;
        int rows = Mathf.CeilToInt((float)totalCells / cols);

        for (int r = 0; r < rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < cols; c++)
            {
                int gridIndex = r * cols + c;
                int day = gridIndex - levelData.startOffset + 1;

                // Offset alanýndaki boþ hücreler
                if (day < 1 || day > levelData.totalDays)
                {
                    GUILayout.Box("", GUILayout.Width(32), GUILayout.Height(32));
                    continue;
                }

                bool inRange = levelData.IsDayInRange(day);
                bool hasOverride = levelData.GetCellOverride(day) != null;

                Color prev = GUI.backgroundColor;
                if (day == selectedDay) GUI.backgroundColor = Color.cyan;
                else if (hasOverride) GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
                else if (inRange) GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);

                if (GUILayout.Button(day.ToString(), GUILayout.Width(32), GUILayout.Height(32)))
                    selectedDay = day;

                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawSelectedDayInspector()
    {
        if (selectedDay < 1) return;

        EditorGUILayout.LabelField($"Day {selectedDay} — Properties", EditorStyles.boldLabel);

        CellData data = levelData.GetCellOverride(selectedDay);
        bool hadOverride = data != null;
        if (data == null) data = new CellData { dayNumber = selectedDay };

        data.modifier = (CellModifier)EditorGUILayout.EnumFlagsField("Modifier", data.modifier);
        data.pinPrefabOverride = (GameObject)EditorGUILayout.ObjectField(
            "Pin Prefab Override", data.pinPrefabOverride, typeof(GameObject), false);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            if (!hadOverride) levelData.cellOverrides.Add(data);
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
        }
        if (hadOverride && GUILayout.Button("Delete Override"))
        {
            levelData.cellOverrides.RemoveAll(c => c.dayNumber == selectedDay);
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();
    }
}