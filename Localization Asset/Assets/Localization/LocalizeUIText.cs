using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add the script of this class (LocalizationText.cs) to a text which should be localized
/// </summary>
public class LocalizeUIText : MonoBehaviour
{
    [Tooltip("Add the Scripts/ Scriptable Objects/ Game object Components which have the property / field you want to be shown. Enter the Property/Field names by the order to be shown in text.  For the values which are not strings, .ToString() values will be used.")]
    [SerializeField] private DynamicParts DynamicParts = default;
    [SerializeField] private string key;
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

        string text = dpForCode != null ? LocalizationManager.Instance.GetLocalizedValue(key, dpForCode) :
            LocalizationManager.Instance.GetLocalizedValue(key, DynamicParts);
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
            if (a.Length != dpForCode.DynamicVariables.Count)
            {
                Debug.LogError("Inconsistent number of variables sent to update." +
                    "You created this localized text with " + dpForCode.DynamicVariables.Count +
                    " variables but you provided " + a.Length +
                    " variables to update.\n This text will not be updated");
                return;
            }
            for (int i = 0; i < a.Length; i++)
                dpForCode.DynamicVariables[i] = a[i];
        }
        GetTranslatedText();
    }

    public void SetDynamicVariables(List<object> DynamicVariables)
    {
        this.dpForCode = new DynamicVarsForCode();
        this.dpForCode.DynamicVariables = DynamicVariables;
    }

    public void SetKey(string key) => this.key = key;

    private DynamicVarsForCode dpForCode;
    private bool isCreatedByCode;
    public bool SetCodeCreated() => this.isCreatedByCode = true;
    #endregion

}

[Serializable]
public class DynamicParts
{
    public List<DynamicPartInLocalizedText> dynamicPartList;
}

[Serializable]
public class DynamicPartInLocalizedText
{
    public UnityEngine.Object script;
    public string VariableName;
}

public class DynamicVarsForCode
{
    public List<object> DynamicVariables;
}

public static class LocalizationTextCreator
{
    public static LocalizeUIText Add(GameObject g, string key, params object[] DynamicParts)
    {

        if (g == null || key == null)
        {
            Debug.LogError("You must provide a game object and string which are not null");
            return null;
        }
        if (g.GetComponent<LocalizeUIText>() != null) return null;

        g.SetActive(false);

        LocalizeUIText lt = g.AddComponent<LocalizeUIText>();

        lt.SetKey(key);
        lt.SetCodeCreated();
        if (DynamicParts.Length != 0) lt.SetDynamicVariables(DynamicParts.ToList());
        g.SetActive(true);

        return lt;
    }
}