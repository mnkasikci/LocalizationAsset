using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalizationEditorHelper
{
    public static Dictionary<string, string> localizedText;

    public static string GetLocalizedValue(string key)
    {
        if(localizedText == null)
        {
            //Debug.LogWarning("No localization file activated. Please open a localization file from the localization editor.");
            return "";
        }

        if (localizedText.ContainsKey(key))
            return localizedText[key];
        else
        {
            //Debug.LogWarning("Localized value for '" + key + "' could not be found.");
            return "";
        }
    }
}
