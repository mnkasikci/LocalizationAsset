using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add this script to a UI text component which should be localized.
/// </summary>
public class LocalizeUIText : MonoBehaviour
{
    [Space]
    [Tooltip("This key will be used to retrieve the localized text from the relevant language file.")]
    [SerializeField] private string key;
    [Space]
    [Tooltip("This is only necessary if you want to include variables in the localized text, otherwise leave this list empty. " +
        "Variable markup like '{0}' in the localized text will be replaced with these variables in the same order. " +
        "For each variable, you should provide its source and name.")]
    [SerializeField] private DynamicVariables variables = default;

    public void OnEnable()
    {
        try { GetTranslatedText(); }
        catch (NullReferenceException)
        {
            var le = new LocalizationException(true);
            throw le;
        }
    }

    private void AssignText(string text)
    {
        Component textComp = GetComponent<Text>();
        if (textComp == null) textComp = GetComponent<TextMeshProUGUI>();
        if (textComp == null) { Debug.LogError("You must place this script to a game object with Text or TextMeshPro component"); return; }

        if (string.IsNullOrEmpty(key)) return;

        if (textComp is TextMeshProUGUI)
            ((TextMeshProUGUI)textComp).text = text;
        else
            ((Text)textComp).text = text;
    }

    private void GetTranslatedText()
    {
        string text = dpForCode != null ? LocalizationManager.Instance.LocalizeThroughComponent(key, dpForCode) :
            LocalizationManager.Instance.LocalizeThroughComponent(key, variables);
        AssignText(text);
    }

    public void InspectorCreatedUpdate()
    {
        if (this.isCreatedByCode)
        {
            Debug.LogError("This localization text is created by code. Update this localized text from CodeCreatedUpdate");
            return;
        }
        GetTranslatedText();
    }

    #region CreatedByCode
    public void CodeCreatedUpdate(params object[] a)
    {
        if (!this.isCreatedByCode)
        {
            Debug.LogError("This localization text is not created by code. Update this localized text from InspectorCreatedUpdate");
            return;
        }
        if (dpForCode != null)
        {
            if (a.Length != dpForCode.dynamicVariables.Count)
            {
                Debug.LogError("Inconsistent number of variables sent to update." +
                    "You created this localized text with " + dpForCode.dynamicVariables.Count +
                    " variables but you provided " + a.Length +
                    " variables to update.\n This text will not be updated");
                return;
            }
            for (int i = 0; i < a.Length; i++)
                dpForCode.dynamicVariables[i] = a[i];
        }
        GetTranslatedText();
    }

    public void SetDynamicVariables(List<object> DynamicVariables)
    {
        this.dpForCode = new DynamicVarsForCode();
        this.dpForCode.dynamicVariables = DynamicVariables;
    }

    public void SetKey(string key) => this.key = key;

    private DynamicVarsForCode dpForCode;
    private bool isCreatedByCode;
    public bool SetCodeCreated() => this.isCreatedByCode = true;
    #endregion
}

[Serializable]
public class DynamicVariables
{
    public List<DynamicVariableInLocalizedText> list;
}

[Serializable]
public class DynamicVariableInLocalizedText
{
    [Tooltip("The source can be a script that implements the singleton pattern, a scriptable object, or a gameobject component. ")]
    public UnityEngine.Object variableSource;
    [Tooltip("Name of the property or the field should be written exactly as it is in the source.")]
    public string variableName;
}

public class DynamicVarsForCode
{
    public List<object> dynamicVariables;
}

public static class LocalizationTextCreator
{
    
}