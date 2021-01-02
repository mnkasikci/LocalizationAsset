using UnityEditor;
using UnityEngine;

// TODO: Solve the "Unsupported type LocalizedString" error

[CustomPropertyDrawer(typeof(LocalizedString))]
public class LozalizedStringDrawer : PropertyDrawer
{
    bool dropdown = false;
    float height;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (dropdown)
            return height + 25;

        return 20;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        position.width -= 17;
        position.height = 18;

        Rect valueRect = new Rect(position);
        valueRect.x += 15;
        valueRect.width -= 15;

        Rect foldButtonRect = new Rect(position);
        foldButtonRect.width = 15;

        dropdown = EditorGUI.Foldout(foldButtonRect, dropdown, "");

        position.x += 15;
        position.width -= 15;

        SerializedProperty key = property.FindPropertyRelative("key");
        key.stringValue = EditorGUI.TextField(position, key.stringValue);

        position.x += position.width + 2;
        position.width = 17;
        position.height = 17;

        if (GUI.Button(position, "+"))
            LocalizationAddItemWindow.Open();

        if (dropdown)
        {
            var value =  LocalizationEditorHelper.GetLocalizedValue(key.stringValue);
            if (value == string.Empty) value = "Key not found.";
            GUIStyle style = GUI.skin.box;
            height = style.CalcHeight(new GUIContent(value), valueRect.width);

            valueRect.height = height;
            valueRect.y += 21;
            EditorGUI.LabelField(valueRect, value, EditorStyles.wordWrappedLabel);
        }

        EditorGUI.EndProperty();
    }
}
