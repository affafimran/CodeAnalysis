using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MedalsManager : MedalsBaseManager
{
	public enum Medal {FIRSTBLOOD = 1, MANDOWN, REDBARRELRAGE, THEPROFESSIONAL, TANKBUSTER, THEBUTCHER, DESTROYEROFWORLDS, POSTMAN, MISSIONMASTER, MONEYMAN};

	// For the description of each medal, please see the English (US).txt (Achievements descriptions).
	// For the data / amounts that need to be collected, see the SharedData.txt under Medals.
	// For the technical implementation / functionality of each specific medal, please search for MedalsManager in the project.
	// Testing if a value needs to be added to a specific medal is done in areas where the rest of the functionality also sits.
	// For example, calling the addition of a completed mission for the MissionMaster medal is done in the MissionManager.

	public MedalsManager() : base()
	{
	}

	// implement required functions as desired by MedalsBaseManager
	protected override bool? IsMedalObtainedSpecific(int aMedal)
	{  // returning null will let the MedalsBaseManager take care of it
		return null;
	}

	protected override string GetMedalProgressionSpecific(int aMedal)
	{  // returning null will let the MedalsBaseManager take care of it
		return null;
	}

	protected override float? GetMedalProgressionFloatSpecific(int aMedal)
	{  // returning null will let the MedalsBaseManager take care of it
		return null;
	}

	protected override void CheatNearlyGetAllMedalsSpecific()
	{  // set specific values
	}

	protected override bool? UpdateMedalSpecific(int aMedal, int aValue, string aString)
	{  // returning null will let the MedalsBaseManager take care of it
		switch(aMedal)
		{
			case (int)Medal.MONEYMAN:  // perform max op
				DicEntry tMedalDicMan = GameData.medalProgression["Medal" + aMedal];
			tMedalDicMan.i = Mathf.Max(tMedalDicMan.i, aValue);
			if (tMedalDicMan.i > Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i)
				tMedalDicMan.i = Data.Shared["Medals"].d["Medal" + aMedal].d["Data"].i;
				break;
			default:
				return null;  // let it be handled by the MedalsBaseManager
		}
		return true;
	}

	// used for the SHOPAHOLIC medal: get all bought items (including upgrades)
	private int GetAllBoughtItemsCount()
	{
//		int tBoughtCount = 0;
//		tBoughtCount += GameData.unlockedUnits.Count;  // units
//
//		// upgrades
////		foreach (KeyValuePair<string, int> tPair in GameData.upgradedLevels)
////			tBoughtCount += tPair.Value;
		return 0;
	}

	// optional:

	// when obtaining a medal, we may have to do some external stuff: add progression, add score
	protected override void MedalObtained(int aMedal)
	{
		// Send stuff out of the game here!
	}
}
