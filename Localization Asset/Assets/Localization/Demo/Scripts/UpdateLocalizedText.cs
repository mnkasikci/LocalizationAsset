using UnityEngine;

public class UpdateLocalizedText : MonoBehaviour
{
    [SerializeField] LocalizeUIText localizeUITextComponent = default;

    public void UpdateText()
    {
        localizeUITextComponent.UpdateText();
    }
}
