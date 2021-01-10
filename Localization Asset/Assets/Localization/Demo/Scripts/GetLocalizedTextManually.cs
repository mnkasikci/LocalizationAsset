using UnityEngine;
using UnityEngine.UI;

public class GetLocalizedTextManually : MonoBehaviour
{
    [SerializeField] Text text = default;

    void Start()
    {
        text.text = LocalizationManager.Instance.GetLocalizedValue("Hello World");
    }
}
