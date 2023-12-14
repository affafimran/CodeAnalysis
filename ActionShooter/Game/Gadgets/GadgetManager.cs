using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GadgetManager.
/// <para>Nothing much here... You can add all kinds of gadget related stuff here</para>
/// </summary>
public static class GadgetManager
{
	public enum GADGETS{TimeFreeze, Pigeon, Magnet, CashForKills, RapidFire};

	/// <summary>
	/// Initialize the GadgetManager
	/// <para>Although it doesn't do anything (yet)</para>
	/// </summary>
	public static void Initialize(){}

	/// <summary>
	/// Check if you already have a gagdet
	/// <para>This is [HARDCODED] to Hammer. Ideally add Character to params to check upon specific character</para>
	/// </summary>
	/// <returns><c>true</c>, if owned was gadgeted, <c>false</c> otherwise.</returns>
	/// <param name="aGadgetname">A gadgetname.</param>
	public static bool GadgetOwned(string aGadgetname)
	{
		foreach (Gadget aGadget in Scripts.hammer.gadgets){
			if (aGadget.type == aGadgetname) return true;
		}
		return false;
	}
}