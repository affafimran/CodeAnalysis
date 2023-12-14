// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE
// Stijn 03/12/2012
// This class was made to facilitate medal managing by providing a standard way of dealing with medals (a tiny framework if you will)
// The reason this is not an interface is because some standard functionality can be handled by this class
// Your game's medal manager should derive from this class
// The game code can only make calls to this class, not to your underlying child class (in this example named MedalsManager)
// It is advised to make your child implement an enumerator such as:
// public enum Medal {UNLOCKSNOWBOARDS = 1, UNLOCKCLOTHES, ...}
// and then call Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.UNLOCKSNOWBOARDS, 1);
// This is to keep your code clear -> so you know what medal you're updating instead of calling Scripts.medalsManager.UpdateMedal(1, 1)
// Data is obtained from SharedData.txt -> Medals.Medal<i>.Data

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MedalsBaseManager  // abstract -> so it can't be instantiated on its own (it needs to be instantiated by a child)
{
	public MedalsBaseManager()
	{
	}

	public bool IsMedalObtained(int aMedal)
	{
		bool? tReturn = IsMedalObtainedSpecific(aMedal);
		if (tReturn.HasValue)
		{
			return tReturn.Value;
		} else
		{  // yes, technically else statement is not needed (cause of return), but just for clarity
			return (GameData.medalProgression["Medal" + aMedal].i == Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i);
		}
	}

	public string GetMedalProgression(int aMedal)
	{
		string tReturn = GetMedalProgressionSpecific(aMedal);
		if (tReturn != null)
		{
			return tReturn;
		} else
		{  // yes, technically else statement is not needed (cause of return), but just for clarity
			return GameData.medalProgression["Medal" + aMedal].i.ToString() + "/" + Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i.ToString();
		}
	}

	public float GetMedalProgressionFloat(int aMedal)
	{
		float? tReturn = GetMedalProgressionFloatSpecific(aMedal);
		if (tReturn.HasValue)
		{
			return tReturn.Value;
		} else
		{  // yes, technically else statement is not needed (cause of return), but just for clarity
			return GameData.medalProgression["Medal" + aMedal].i / (float)(Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i);
		}
	}

	public void CheatNearlyGetAllMedals()
	{
		foreach (KeyValuePair<string, DicEntry> tPair in GameData.medalProgression)
		{
			if (tPair.Value.type == DicEntry.EntryType.INT)
			{
				tPair.Value.i = Data.Shared["Medals"].d[tPair.Key].d["Data"].i - 1;
			}
		}
		CheatNearlyGetAllMedalsSpecific();
	}

	public void UpdateMedal(int aMedal, int aValue)
	{
		UpdateMedal(aMedal, aValue, null);
	}

	public void UpdateMedal(int aMedal, string aString)
	{
		UpdateMedal(aMedal, -999, aString);
	}

	private void UpdateMedal(int aMedal, int aValue, string aString)
	{
//		Debug.Log("MedalsBaseManager UpdateMedal called: " + aMedal + ", value: " + aValue.ToString());
		// check if the medal was already obtained
		if (IsMedalObtained(aMedal)) return;

		// medal not obtained, update medal, we'll handle it when null is returned
		if (UpdateMedalSpecific(aMedal, aValue, aString) == null)
		{
			// implement default behaviour: for a string we have no idea what is the default behaviour, child must implement it
			if (aString != null)
			{
				throw new UnityException("UpdateMedal called with string value must be handled by child!");
			} else
			{  // got an int value, do default action: add aValue to medal variable, if any other action is desired then child must implement it
				GameData.medalProgression["Medal" + aMedal].i += aValue;
				if (GameData.medalProgression["Medal" + aMedal].i > Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i)
					GameData.medalProgression["Medal" + aMedal].i = Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i;
			}
		}

		// variables updated -> now check again if medal has been obtained, if so show it
		if (IsMedalObtained(aMedal))
		{
			MedalObtained(aMedal);
			if (Scripts.interfaceScript != null)
			{
				Debug.Log ("[MedalsBaseManager] Trying to notify InterfaceScript! Medal: " + aMedal.ToString());
				Scripts.interfaceScript.DisplayAchievement(aMedal);
			}
			else Debug.LogError("[MedalsBaseManager] InterfaceScript not existent while awarding medal");
		}
	}

	// The following functions need to be implemented by the child:
	// if child wants to use default behaviour it can just return null for these functions
	protected abstract bool? IsMedalObtainedSpecific(int aMedal);  // first called, if child returns null then we'll fill in the value
	protected abstract string GetMedalProgressionSpecific(int aMedal);  // first called, if child returns null then we'll fill in the value
	protected abstract float? GetMedalProgressionFloatSpecific(int aMedal);  // first called, if child returns null then we'll fill in the value
	protected abstract void CheatNearlyGetAllMedalsSpecific();  // a specific cheat for medals where the main cheatNearlyGetAllMedals did not fill in values
	protected abstract bool? UpdateMedalSpecific(int aMedal, int aValue, string aString);  // first called, if child returns null then we'll handle it, if child implements this it should assign to GameData.medalProgression["Medal" + <some_medal>]

	// The following functions do not have to be implemented by the child, but can be
	protected virtual void MedalObtained(int aMedal) {}  // called when medal has been obtained
}
