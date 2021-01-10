using UnityEngine;

public class AddLocalizedText : MonoBehaviour
{
    [SerializeField] GameObject textGameObject = default;

    void Start()
    {
        LocalizationManager.AddLocalizeUIText(textGameObject, "Hello World");
    }
}
