using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CalendarLevelGeneratorWindow : EditorWindow
{
    // Kullanıcının ayarladığı base değerler
    private int totalDays = 30;
    private int columns = 7;
    private int startOffset = 0;
    private int firstDayOfWeek = 1;
    private GameObject defaultPinPrefab;

    // Görsel seçim state'i
    private int startDay = -1;
    private int endDay = -1;
    private HashSet<int> skippedDays = new HashSet<int>();

    // Etkileşim modu
    private enum SelectionMode { StartDay, EndDay, SkippedDays }
    private SelectionMode currentMode = SelectionMode.StartDay;

    // Kayıt yolu
    private string saveFolder = "Assets/CalendarLevels";
    private string levelName = "NewLevel";

    private Vector2 scroll;

    [MenuItem("Tools/Calendar/Level Generator")]
    public static void Open() => GetWindow<CalendarLevelGeneratorWindow>("Level Generator");

    void OnGUI()
    {
        EditorGUILayout.LabelField("Level Generator — Visual Design Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Base ayarları yap, grid üzerinden görsel seç, Generate ile asset oluştur.", MessageType.Info);

        EditorGUILayout.Space(8);
        DrawBaseSettings();

        EditorGUILayout.Space(8);
        DrawModeSelector();

        EditorGUILayout.Space(4);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(280));
        DrawInteractiveGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        DrawSelectionSummary();

        EditorGUILayout.Space(8);
        DrawSaveSection();
    }

    void DrawBaseSettings()
    {
        EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
        totalDays = EditorGUILayout.IntField("Total Days", totalDays);
        columns = EditorGUILayout.IntField("Columns", columns);
        startOffset = EditorGUILayout.IntField("Start Offset", startOffset);
        firstDayOfWeek = EditorGUILayout.IntField("First Day of Week (0=Sun)", firstDayOfWeek);
        defaultPinPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Default Pin Prefab", defaultPinPrefab, typeof(GameObject), false);
    }

    void DrawModeSelector()
    {
        EditorGUILayout.LabelField("Grid Selection Mode", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        DrawModeButton("Start Day", SelectionMode.StartDay, new Color(0.4f, 0.9f, 0.4f));
        DrawModeButton("End Day", SelectionMode.EndDay, new Color(0.2f, 0.6f, 1f));
        DrawModeButton("Skipped Days", SelectionMode.SkippedDays, new Color(1f, 0.9f, 0.3f));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(GetModeHelp(), MessageType.None);
    }

    void DrawModeButton(string label, SelectionMode mode, Color modeColor)
    {
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = currentMode == mode ? modeColor : Color.gray;
        if (GUILayout.Button(label, GUILayout.Height(30))) currentMode = mode;
        GUI.backgroundColor = prev;
    }

    string GetModeHelp()
    {
        switch (currentMode)
        {
            case SelectionMode.StartDay: return "Grid'den başlangıç gününü seç.";
            case SelectionMode.EndDay: return "Grid'den bitiş gününü seç.";
            case SelectionMode.SkippedDays: return "Atlamak istediğin günleri tıkla (tekrar tıkla → geri al).";
            default: return "";
        }
    }

    void DrawInteractiveGrid()
    {
        int cols = Mathf.Max(1, columns);
        int totalCells = totalDays + startOffset;
        int rows = Mathf.CeilToInt((float)totalCells / cols);

        for (int r = 0; r < rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < cols; c++)
            {
                int gridIndex = r * cols + c;
                int day = gridIndex - startOffset + 1;

                if (day < 1 || day > totalDays)
                {
                    GUILayout.Box("", GUILayout.Width(36), GUILayout.Height(36));
                    continue;
                }

                bool isWeekend = c >= cols - 2; // son iki sütun
                bool isStart = day == startDay;
                bool isEnd = day == endDay;
                bool isSkipped = skippedDays.Contains(day);
                bool inRange = startDay > 0 && endDay > 0 && day >= startDay && day <= endDay;

                Color prev = GUI.backgroundColor;

                if (isStart) GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
                else if (isEnd) GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
                else if (isWeekend && inRange) GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                else if (isSkipped) GUI.backgroundColor = new Color(1f, 0.9f, 0.3f);
                else if (inRange) GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                else if (isWeekend) GUI.backgroundColor = new Color(0.6f, 0.4f, 0.4f); // aralık dışı haftasonu

                if (GUILayout.Button(day.ToString(), GUILayout.Width(36), GUILayout.Height(36)))
                {
                    HandleCellClick(day, isWeekend);
                }

                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void HandleCellClick(int day, bool isWeekend)
    {
        switch (currentMode)
        {
            case SelectionMode.StartDay:
                startDay = day;
                if (endDay > 0 && endDay < startDay) endDay = -1; // bitiş öncede kalırsa reset
                break;
            case SelectionMode.EndDay:
                if (startDay < 0) { Debug.LogWarning("Önce Start Day seç."); return; }
                if (day < startDay) { Debug.LogWarning("Bitiş, başlangıçtan önce olamaz."); return; }
                endDay = day;
                break;
            case SelectionMode.SkippedDays:
                if (isWeekend) { Debug.LogWarning("Haftasonu zaten otomatik atlanır, skip'e gerek yok."); return; }
                if (skippedDays.Contains(day)) skippedDays.Remove(day);
                else skippedDays.Add(day);
                break;
        }
    }

    void DrawSelectionSummary()
    {
        EditorGUILayout.LabelField("Current Selection", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Start Day: {(startDay > 0 ? startDay.ToString() : "not set")}");
        EditorGUILayout.LabelField($"End Day: {(endDay > 0 ? endDay.ToString() : "not set")}");
        EditorGUILayout.LabelField($"Skipped Days: {(skippedDays.Count > 0 ? string.Join(", ", skippedDays) : "none")}");
    }

    void DrawSaveSection()
    {
        EditorGUILayout.LabelField("Save", EditorStyles.boldLabel);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        GUI.enabled = startDay > 0 && endDay > 0;
        if (GUILayout.Button("Generate Level Asset", GUILayout.Height(40)))
        {
            GenerateAsset();
        }
        GUI.enabled = true;

        if (startDay < 0 || endDay < 0)
        {
            EditorGUILayout.HelpBox("Start ve End Day seçmeden generate edemezsin.", MessageType.Warning);
        }
    }

    void GenerateAsset()
    {
        // Folder yoksa oluştur
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            AssetDatabase.Refresh();
        }

        CalendarLevelData asset = ScriptableObject.CreateInstance<CalendarLevelData>();
        asset.totalDays = totalDays;
        asset.columns = columns;
        asset.startOffset = startOffset;
        asset.firstDayOfWeek = firstDayOfWeek;
        asset.startDay = startDay;
        asset.endDay = endDay;
        asset.defaultPinPrefab = defaultPinPrefab;
        asset.skippedDays = new List<int>(skippedDays);

        string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{levelName}.asset");
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        // Yeni oluşturulan asset'i seçili yap ve Project window'da göster
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"[LevelGenerator] Yeni level asset'i oluşturuldu: {path}");
    }
}