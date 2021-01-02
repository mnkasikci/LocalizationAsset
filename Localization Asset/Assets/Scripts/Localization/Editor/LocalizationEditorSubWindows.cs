using UnityEditor;
using UnityEngine;

public class LocalizationDeleteItemWindow : EditorWindow
{
    LocalizationItem item;
    public static void Open(LocalizationItem item)
    {
        LocalizationDeleteItemWindow window = CreateInstance<LocalizationDeleteItemWindow>();
        //window.ShowUtility();
        window.item = item;

        bool option = EditorUtility.DisplayDialog("Delete Localization Item", 
            "Delete the localization item with key '" + item.key + "'?", 
            "Yes, delete.", 
            "No, cancel");

        if(option)
            GetWindow<LocalizationEditor>().DeleteLocalizationItem(item);
    }
}

public class LocalizationEditItemWindow : EditorWindow
{
    public string key;
    public string oldKey;
    public string value;

    public static void Open(string key, string value)
    {
        LocalizationEditItemWindow window = CreateInstance<LocalizationEditItemWindow>();
        window.titleContent = new GUIContent("Edit Item");
        window.ShowUtility();
        window.key = key;
        window.oldKey = key;
        window.value = value;
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Key:", GUILayout.MaxWidth(50));
        key = EditorGUILayout.TextField(key);

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));
        EditorStyles.textArea.wordWrap = true;
        value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Width(3600), GUILayout.Height(100));

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save"))
        {
            if (LocalizationEditorHelper.GetLocalizedValue(key) != string.Empty && oldKey != key) // Replace key
                ConfirmReplaceLocalizationItem();
            else // Save edited key
            {
                GetWindow<LocalizationEditor>().ReplaceLocalizationItemWhole(oldKey, key, value);
                Close();
            }
        }

        minSize = new Vector2(360, 180);
        maxSize = minSize;
    }

    void ConfirmReplaceLocalizationItem()
    {
        bool option = EditorUtility.DisplayDialog("Replace Localization Item",
            "The current localization file already contains a value for this key. Do you want to replace the value?",
            "Yes, replace.",
            "No, cancel.");

        if(option)
        {
            GetWindow<LocalizationEditor>().ReplaceLocalizationItemWithKey(key, value);
            Close();
        }
    }
}

public class LocalizationAddItemWindow : EditorWindow
{
    public string key;
    public string value;

    public static void Open()
    {
        GetWindow(typeof(LocalizationEditor), false, null, false);
        if (GetWindow<LocalizationEditor>().localizationData == null)
        {
            EditorUtility.DisplayDialog("Load Localization Data File", "Please load a localization data file first.", "OK");
            GetWindow<LocalizationEditor>().LoadLocalizationData();
            // TODO: Solve the "InvalidOperationException: Stack empty." error
        }

        LocalizationAddItemWindow window = CreateInstance<LocalizationAddItemWindow>();
        window.titleContent = new GUIContent("Add New Item");
        window.ShowUtility();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Key:", GUILayout.MaxWidth(50));
        key = EditorGUILayout.TextField(key);

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));
        EditorStyles.textArea.wordWrap = true;
        value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Width(3600), GUILayout.Height(100));
        
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add"))
        {
            if (LocalizationEditorHelper.GetLocalizedValue(key) != string.Empty) // Replace key
                ConfirmReplaceLocalizationItem();
            else // Add key
            {
                GetWindow<LocalizationEditor>().AddLocalizationItem(key, value);
                Close();
            }   
        }

        minSize = new Vector2(360, 180);
        maxSize = minSize;
    }

    void ConfirmReplaceLocalizationItem()
    {
        bool option = EditorUtility.DisplayDialog("Replace Localization Item",
            "The current localization file already contains a value for this key. Do you want to replace the value?",
            "Yes, replace.",
            "No, cancel.");

        if(option)
        {
            GetWindow<LocalizationEditor>().ReplaceLocalizationItemWithKey(key, value);
            Close();
        }
    }
}
