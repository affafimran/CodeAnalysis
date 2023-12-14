// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserData
{
	//private static bool pDebug = true;

	public static void Save()
	{
		Data.CopyFromDataToGlobals();  // makes sure all changes in data are now in globals

		// The TextLoader will create a string out of each of the properties mentioned in the savelist
		string tSaveString = TextLoader.SaveText(Data.GetGlobals());
		Debug.Log("[UserData] Saved string:\n" + tSaveString);
		PlayerPrefs.SetString("Save", tSaveString);
	}

	static public void Load()
	{
		string tLoadString = PlayerPrefs.GetString("Save");
		if (tLoadString == "")
		{
			Debug.Log("[UserData] Loading: failed.");
			Reset();
			return;
		}

		Debug.Log("[UserData] Loaded: \n" + tLoadString);

		Dictionary<string, DicEntry> tLoadDic = new Dictionary<string, DicEntry>();
		TextLoader.LoadText(tLoadString, tLoadDic);

		if (Data.versionNumber != tLoadDic["VersionNumber"].s)
		{
			Debug.Log("[UserData] Loading: Version number did not match. Should be " + Data.versionNumber + " but is " + tLoadDic["VersionNumber"].s);
			Reset();
			return;
		}

		// we're good to load
		foreach (string tSaveProp in Data.saveList)
		{
			// copy each prop from the save list into the globals (replace)
			if (!tLoadDic.ContainsKey(tSaveProp))
			{
				Debug.Log("[UserData] Loading: Property " + tSaveProp + " not found in loaded text");
				Reset();
				return;
			}

			//Debug.Log(tSaveProp + " - " + tLoadDic[tSaveProp]);
			Data.GetGlobals()[tSaveProp] = tLoadDic[tSaveProp];  // copy (referenced)
		}

		Data.CopyFromGlobalsToData();
	}

	static public void Reset()
	{
		PlayerPrefs.DeleteKey("Save");
		Save();
	}
}
