using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

public class DeletSaveFiles : EditorWindow
{


    static string GameDataFileName = "saveData.dat";
    static string JsonFileName = "jsonData.dat";


    [MenuItem("Tools/Reset Data")]
    public static void ResetData()
    {
        ClearLog();

        string path = GetFilePath(JsonFileName);
        if (File.Exists(path))
        {
            Debug.Log("Deleting json file...");
            File.Delete(path);

            path = GetFilePath(GameDataFileName);
            if (File.Exists(path))
            {
                Debug.Log("Deleting game data file...");
                File.Delete(path);
            }

        }
        PlayerPrefs.DeleteAll();
    }

    static string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }

    static void ClearLog() //you can copy/paste this code to the bottom of your script
    {
        var assembly = Assembly.GetAssembly(typeof(Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}