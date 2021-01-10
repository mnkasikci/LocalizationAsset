
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocalizationEditor : EditorWindow
{
    public LocalizationData localizationData;
    string openedFilePath;
    string searchValue = "";
    Vector2 scroll;

    [MenuItem("Tools/Localization/Editor")]
    static void Init()
    {
        GetWindow(typeof(LocalizationEditor)).Show();
    }

    void OnGUI()
    {
        if (localizationData != null)
        {
            EditorGUILayout.BeginHorizontal("Box");

            EditorGUILayout.LabelField("Search: ", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            searchValue = EditorGUILayout.TextField(searchValue);
            if (GUILayout.Button("Add New Item", GUILayout.MaxWidth(100)))
                LocalizationAddItemWindow.Open();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            DrawSeparator();
            EditorGUILayout.Space();

            if (searchValue == string.Empty && localizationData.items.Count > 0)
            {
                EditorGUILayout.BeginVertical();
                scroll = EditorGUILayout.BeginScrollView(scroll);

                foreach (LocalizationItem item in localizationData.items)
                {
                    GUILayout.BeginHorizontal("Box");

                    EditorGUILayout.LabelField(item.key, GUILayout.MinWidth(180));
                    EditorGUILayout.LabelField(item.value, GUILayout.MinWidth(180));
                    if (GUILayout.Button("Edit", GUILayout.MaxHeight(20), GUILayout.MaxWidth(50)))
                        LocalizationEditItemWindow.Open(item.key, item.value);
                    if (GUILayout.Button("Delete", GUILayout.MaxHeight(20), GUILayout.MaxWidth(50)))
                    {
                        LocalizationDeleteItemWindow.Open(item);
                        break;
                    }

                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else if (searchValue != string.Empty && localizationData.items.Count > 0)
            {
                EditorGUILayout.BeginVertical();
                scroll = EditorGUILayout.BeginScrollView(scroll);

                foreach (LocalizationItem item in localizationData.items)
                    if (item.key.ToLower().Contains(searchValue.ToLower()) || item.value.ToLower().Contains(searchValue.ToLower()))
                    {
                        GUILayout.BeginHorizontal("Box");

                        EditorGUILayout.LabelField(item.key, GUILayout.MinWidth(180));
                        EditorGUILayout.LabelField(item.value, GUILayout.MinWidth(180));
                        if (GUILayout.Button("Edit", GUILayout.MaxHeight(20), GUILayout.MaxWidth(50)))
                            LocalizationEditItemWindow.Open(item.key, item.value);
                        if (GUILayout.Button("Delete", GUILayout.MaxHeight(20), GUILayout.MaxWidth(50)))
                        {
                            LocalizationDeleteItemWindow.Open(item);
                            break;
                        }

                        GUILayout.EndHorizontal();
                    }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Localization File Settings", EditorStyles.boldLabel);
        DrawSeparator();

        GUILayout.BeginHorizontal();

        if (localizationData != null && GUILayout.Button("Save"))
            SaveLocalizationData();

        if (localizationData != null && GUILayout.Button("Save As"))
            SaveAsLocalizationData();

        if (GUILayout.Button("Load"))
            LoadLocalizationData();

        if (GUILayout.Button("Create New"))
            CreateNewLocalizationData();

        GUILayout.EndHorizontal();

        minSize = new Vector2(360, 500);
        maxSize = new Vector2(640, 500);
    }

    #region Localization Item Methods
    public void AddLocalizationItem(string key, string value)
    {
        LocalizationItem tempItem = new LocalizationItem();
        tempItem.key = key;
        tempItem.value = value;
        localizationData.items.Add(tempItem);
    }

    public void ReplaceLocalizationItemWithKey(string key, string value)
    {
        foreach (LocalizationItem item in localizationData.items)
        {
            if (item.key == key)
            {
                item.value = value;
                break;
            }
        }
    }

    public void ReplaceLocalizationItemWhole(string oldKey, string newKey, string newValue)
    {
        foreach (LocalizationItem item in localizationData.items)
        {
            if (item.key == oldKey)
            {
                item.key = newKey;
                item.value = newValue;
                break;
            }
        }
    }

    public void DeleteLocalizationItem(LocalizationItem item)
    {
        localizationData.items.Remove(item);
    }

    public bool IsKeyAlreadyInLocalizationData(string key)
    {
        if (localizationData.items.Any(f => f.key == key))
            return true;
        else 
            return false;
    }
    #endregion

    #region File Methods
    public void LoadLocalizationData()
    {
        if (localizationData != null)
            UnsavedChanges();

        openedFilePath = EditorUtility.OpenFilePanel("Open Localization Data File", Application.streamingAssetsPath + "/Languages", "json");

        if (!string.IsNullOrEmpty(openedFilePath))
        {
            string dataAsJson = File.ReadAllText(openedFilePath);
            localizationData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
        }
    }

    void SaveLocalizationData()
    {
        if (!string.IsNullOrEmpty(openedFilePath))
        {
            string dataAsJson = JsonUtility.ToJson(localizationData);
            File.WriteAllText(openedFilePath, dataAsJson);
            EditorUtility.DisplayDialog("Save Localization Data File", "Successfully saved the current localization file.", "OK");
        }
        else if (string.IsNullOrEmpty(openedFilePath))
            EditorUtility.DisplayDialog("Save Localization Data File", "There is no save path. Please use 'Save As'.", "OK");
    }

    void SaveAsLocalizationData()
    {
        string filePath = EditorUtility.SaveFilePanel("Save Localization Data File", Application.streamingAssetsPath, "", "json");

        if (!string.IsNullOrEmpty(filePath))
        {
            string dataAsJson = JsonUtility.ToJson(localizationData);
            File.WriteAllText(filePath, dataAsJson);
            EditorUtility.DisplayDialog("Save Localization Data File", "Successfully saved the current localization file.", "OK");
            openedFilePath = filePath;
        }
    }

    void CreateNewLocalizationData()
    {
        localizationData = new LocalizationData();
        localizationData.items = new List<LocalizationItem>();
    }
    #endregion

    static void DrawSeparator()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    void UnsavedChanges()
    {
        int option = EditorUtility.DisplayDialogComplex("Unsaved Changes",
            "Do you want to save the changes you made?",
            "Save",
            "Don't Save",
            "Save As");

        switch (option)
        {
            // Save
            case 0:
                if (localizationData != null)
                    SaveLocalizationData();
                break;

            // Save as
            case 2:
                if (localizationData != null)
                    SaveAsLocalizationData();
                break;

            // Don't save
            case 1:
                break;

            default:
                Debug.LogError("Unrecognized option.");
                break;
        }
    }

    void OnDestroy()
    {
        if (localizationData != null)
            UnsavedChanges();
    }
}
