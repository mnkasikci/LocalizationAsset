using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        transform.parent = null;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    void Start()
    {
        StartCoroutine(LoadFirstScene("Actual Scene"));
    }

    IEnumerator LoadFirstScene(string sceneName)
    {
        // Wait for a frame if the LocalizationManager is not ready yet
        while (LocalizationManager.Instance.IsReady == false)
            yield return null;

        // Once the LocalizationManager is ready, load the scene
        SceneManager.LoadScene(sceneName);
    }
}
