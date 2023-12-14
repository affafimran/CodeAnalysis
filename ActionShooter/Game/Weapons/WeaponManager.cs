using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WeaponManager.
/// <para>Weapons for Characters</para>
/// </summary>
public static class WeaponManager
{
	public enum WEAPONS{DesertEagle, Magnum, Remington, AK47, Rpg}; // available weapons for easy access
	public static List<string> availableWeapons; // list of same weapons as string
	public static bool rapidFire; // rapidFire toggle (ShopItem)

	/// <summary>
	/// Initialize the WeaponManager
	/// </summary>
	public static void Initialize()
	{
		rapidFire = false; // false per default (ShopItem overrides)
		// fill the list
		availableWeapons = new List<string>(); 
		int length = System.Enum.GetValues(typeof(WEAPONS)).Length;
		for (int i = 0; i < length; i++) {
			availableWeapons.Add(System.Enum.GetValues(typeof(WEAPONS)).GetValue(i).ToString());
		}
		// Replace weapon
		// Last minute [HACK] to have an ultimate golden gun as a ShopItem
		if (!ShopItemManager.IsBought("ShopItem12")) availableWeapons.Remove("Magnum");
		else availableWeapons.Remove("DesertEagle");
	}
}

