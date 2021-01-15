using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        Init();
        Debug.Log("Quick Localization added \"Actual Scene\" and \"Startup Scene\" scenes to scene build order in order to ensure that the demo works properly.");
    }

    public static void Init()
    {
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        List<string> SceneList = new List<string>();
        string MainFolder = "Assets/QuickLocalization/Demo";

        DirectoryInfo d = new DirectoryInfo(@MainFolder);
        FileInfo[] Files = d.GetFiles("*.unity"); //Getting unity files

        foreach (FileInfo file in Files)
            SceneList.Add(file.Name);

        for (int i = 0; i < SceneList.Count; i++)
        {
            string scenePath = MainFolder + "/" + SceneList[i];
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}
