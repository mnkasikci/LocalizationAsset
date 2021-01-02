using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public static class DataFinder
{
    /// <summary>
    /// Returns the asked data (property or field) from the provided script or component in the form of a string
    /// </summary>
    /// <param name="ScriptorcomponentorSO"> The script to look for the data </param>
    /// <param name="Name">The name of the property / field </param>
    /// <returns></returns>
    public static object GetData(UnityEngine.Object ScriptorcomponentorSO, string Name)
    {
        Type type;
        
        if (ScriptorcomponentorSO is MonoScript) //If a script is dragged. thus, a singleton
        {
            MonoScript Script = ScriptorcomponentorSO as MonoScript;
            type = Script.GetClass();
            if (type.GetProperty("Instance") != null)
            {
                MonoBehaviour InstanceData = (MonoBehaviour)type.GetProperty("Instance").GetValue(ScriptorcomponentorSO);
                return GetData(InstanceData, Name);
            }
            else if (type.GetProperty("instance") != null)
            {
                MonoBehaviour InstanceData = (MonoBehaviour)type.GetProperty("instance").GetValue(ScriptorcomponentorSO);
                return GetData(InstanceData, Name);
            }

            throw new LocalizationException("The script " + Script.name + " doesn't have a singleton (No Property called \"Instance\" or \"instance\". )\n");
            
        }
        else if (ScriptorcomponentorSO is ScriptableObject) 
        {
            ScriptableObject scriptableObject = (ScriptableObject)ScriptorcomponentorSO;
            type = scriptableObject.GetType();
            return GetPropertyorField(scriptableObject, type, Name);
        }
        else if (ScriptorcomponentorSO is Component)
        { 
            Component component = (Component)ScriptorcomponentorSO;
            type = component.GetType();
            return GetPropertyorField(component, type, Name);
        }
        throw new LocalizationException("The dragged component \"" + ScriptorcomponentorSO.name+ "\" is neither a component nor a SO nor a Script");
        
    }

    private static object GetPropertyorField(UnityEngine.Object component, Type type, string Name)
    {
        PropertyInfo[] Properties; Properties = type.GetProperties();

        foreach (var property in Properties)
        {
            if (property.Name == Name)
                return (property.GetValue(component));

        }
        FieldInfo[] Fields = type.GetFields();
        foreach (var field in Fields)
        {
            if (field.Name == Name)
                return (field.GetValue(component));
        }
        throw new LocalizationException("The class" + type.Name + " doesn't have a property or field called " + Name);
    }

    public static IReadOnlyList<Type> GetSubClasses<T>() where T : class
    {
        
        return typeof(T).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(T))).ToList();
    }
}

