using System;
using System.Runtime.Serialization;
using Debug = UnityEngine.Debug;
// NEW: Make a system that users automatically submit error.logs for localization exceptions (and for others).This should change the way Localizationexception(message,folderpath) works.
public class LocalizationException : Exception
{
    public string filepath;
    public LocalizationException() : base()
    {
        
    }
    public LocalizationException(string message) : base()
    {
        
        Debug.LogError(message);
        
    }
    public LocalizationException (bool firstSceneException)
    {
        if(firstSceneException)
            Debug.LogError("Be sure \n" +
                        "1- You started from the first scene\n" +
                        "2- You placed a localization manager gameobject in the first scene\n" +
                        "3- You placed localization manager script to that gameobject\n" +
                        "4- The localization manager script has singleton called \"Instance\" \n" +
                        "5- That singleton is not destroyed on scene load (check documentary for implementing monobehaviour singletons)."
                        );
    }
    public LocalizationException(string message, string path) : base()
    {
        this.filepath = path;
        Debug.LogError(message);
        Debug.LogError("Path was: " + path);
    }
    public LocalizationException(string message, string path, Exception innerexception) : this(message, path)
    {
        
    }
}