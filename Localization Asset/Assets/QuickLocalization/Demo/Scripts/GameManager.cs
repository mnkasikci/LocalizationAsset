using System.Collections;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;


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
        //var original = EditorBuildSettings.scenes;
        //var newSettings = new EditorBuildSettingsScene[original.Length + 1];
        //System.Array.Copy(original, newSettings, original.Length);
        //var sceneToAdd = new EditorBuildSettingsScene("Assets/QuickLocalization/Demo/Scripts/Actual Scene.unity", true);
        //newSettings[newSettings.Length - 1] = sceneToAdd;
        //EditorBuildSettings.scenes = newSettings;

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
