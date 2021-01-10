using UnityEngine;

public class ChangeLanguagePanel : MonoBehaviour
{
    public async void ChooseAmericanEnglish()
    {
        await LocalizationManager.Instance.ChooseLanguage("en-US");
    }

    public async void ChooseGerman()
    {
        await LocalizationManager.Instance.ChooseLanguage("de-DE");
    }

    public async void ChooseTurkish()
    {
        await LocalizationManager.Instance.ChooseLanguage("tr-TR");
    }
}
