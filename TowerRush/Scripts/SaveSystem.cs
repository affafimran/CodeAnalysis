using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public  class SaveSystem : MonoBehaviour
{
    static string saveFileName = "saveData.dat";
    static string jsonFileName = "jsonData.dat";

    public static SaveManager SavedData;

    public delegate void SavedDataLoaded();
    public static event SavedDataLoaded OnSavedDataLoaded;

    


    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        LoadGameData.OnLoadDataComplete += LoadGame;
    }

    private void OnDisable()
    {
        LoadGameData.OnLoadDataComplete -= LoadGame;
    }
    


    public static void SaveGame()
    {
        PlayerPrefs.DeleteKey("GameData");
        Debug.Log("Saving...");
        string path = GetFilePath(saveFileName);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        SaveManager data = new SaveManager();
        string jsondata = JsonUtility.ToJson(data);
        WriteToFile(jsonFileName, jsondata);
        formatter.Serialize(stream, data);
        stream.Close();

        PlayerPrefs.SetInt("GameData", 1);
        Debug.Log("Data Saved: " + PlayerPrefs.GetInt("GameData"));
    }



    static void WriteToFile(string fileName, string json)
    {

        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            Debug.Log("File already exist: deleting...");
            File.Delete(path);
        }

        Debug.Log("Trying to write " + path);
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }
    }

    static string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }


    //SaveManager LoadGame()
    //{

    //    string path = GetFilePath(saveFileName);


    //    if (File.Exists(path))
    //    {

    //        BinaryFormatter formatter = new BinaryFormatter();
    //        FileStream stream = new FileStream(path, FileMode.Open);
    //        SaveManager data = formatter.Deserialize(stream) as SaveManager;
    //        stream.Close();

    //        Debug.Log("TRYING TO LOAD THE GAME THROUGH FILE");
    //        return data;
    //    }
    //    else
    //    {
    //        Debug.Log("save file not found on:" + path);
    //        return null;
    //    }
    //}

    void LoadGame()
    {

        string path = GetFilePath(saveFileName);


        if (File.Exists(path))
        {

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SavedData = formatter.Deserialize(stream) as SaveManager;
            stream.Close();

            Debug.Log("TRYING TO LOAD THE GAME THROUGH FILE");
            OnSavedDataLoaded?.Invoke();

        }
        else
        {
            Debug.Log("save file not found on:" + path);

        }
    }



    private static string ReadFromFile(string fileName)
    {
        string path = GetFilePath(fileName);

        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                return json;
            }

        }
        else
        {
            Debug.Log("File not FOUND");
            return "";
        }
    }

    public static string GetJsonFileName()
    {
        return saveFileName;
    }


}
