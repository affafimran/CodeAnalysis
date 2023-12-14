using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ShopItemManager.
/// <para>Contains functions related to anything you can buy in the shop.</para>
/// </summary>
public static class ShopItemManager {

	/// <summary>
	/// Initializes the shop items.
	/// <para>This is NOT a manager initialization. We run this function during a mission setup!</para>
	/// </summary>
	public static void InitializeShopItems()
	{
		// For some owned shopitems we need to init some stuff.
		foreach (string shopItemString in GameData.boughtShopItems){
		 switch (shopItemString){
				case "ShopItem1": CreateJetpack(); break; // Jetpack
 				case "ShopItem2": break; // UnlimitedWeaponAmmo
				case "ShopItem3": break; // UnlimitedExplosiveAmmo
				case "ShopItem4": Scripts.hammer.characterData.godMode = true; break; // Godmode
				case "ShopItem5": break; // Vehicle Generator
				case "ShopItem6": CreateOutfitSign();	break; // Outfits
				case "ShopItem7": CreateParkingSign(); break;	// Parking
				case "ShopItem8": break; // LootDetector
				case "ShopItem9": break;  // CashMultiplier
				case "ShopItem10": break; // Rabbits foot. Doesn't do that much. Makes you more lucky ;)
				case "ShopItem11": break; // ColdOneDetector Show cold ones.
				case "ShopItem12": Scripts.hammer.AddWeapon((WeaponManager.WEAPONS)1); break; // OwnWeapon Golden Gun.
				case "ShopItem13": Scripts.hammer.AddWeapon((WeaponManager.WEAPONS)2); break; // OwnWeapon
				case "ShopItem14": Scripts.hammer.AddWeapon((WeaponManager.WEAPONS)3); break; // OwnWeapon
				case "ShopItem15": Scripts.hammer.AddWeapon((WeaponManager.WEAPONS)4); break; // OwnWeapon
			}
		}
	}

	static void CreateJetpack()
	{
		Scripts.hammer.gameObject.AddComponent<Jetpack>().Initialize();
	}

	static void CreateOutfitSign()
	{
		GameObject outfitSigns = GameObject.Find("OutfitSigns");
		GameObject outfitSign = outfitSigns.transform.Find("OutfitSign").gameObject;
		outfitSign.SetActive(true);
		Scripts.map.CreateMapIcon(outfitSign, "OutfitSign");
	}

	static void CreateParkingSign()
	{
		GameObject parkingSigns = GameObject.Find("ParkingSigns");
		GameObject parkingSign = parkingSigns.transform.Find("ParkingSign").gameObject;
		parkingSign.SetActive(true);
		VehicleManager.AddVehiclesFromChildren(parkingSign, VehicleManager.CONTROLLER.Empty);
		Scripts.map.CreateMapIcon(parkingSign, "ParkingSign");
	}

	/// <summary>
	/// Buy the specified shopItem.
	/// </summary>
	/// <param name="shopItem">Shop item.</param>
	public static bool Buy(string shopItem)
	{
		if (!IsBought(shopItem)) {
			if (CanAfford(shopItem)) {
				GameData.boughtShopItems.Add(shopItem);
				GameData.cash -= Price(shopItem);
				UserData.Save();
				Debug.Log("[ShopItems] Bought: " + shopItem + ". Congratualations on your new purchase.");
				return true;
			} else {
				Debug.Log("[ShopItems] NOT bought. " + shopItem + " Price is: " + Price(shopItem).ToString() + ". You own: " + GameData.cash.ToString());
				return false;
			}
		} else {
			Debug.Log("[ShopItems] NOT bought. You already own " + shopItem + "!");
			return false;
		}
	}

	/// <summary>
	/// Determines if the specified shopItem is already bought.
	/// </summary>
	/// <returns><c>true</c> if is bought the specified shopItem; otherwise, <c>false</c>.</returns>
	/// <param name="shopItem">Shop item.</param>
	public static bool IsBought(string shopItem)
	{
		if (GameData.boughtShopItems.IndexOf(shopItem) != -1) return true;
		else return false;
	}

	/// <summary>
	/// Return the price of a specific shopitem
	/// </summary>
	/// <param name="shopItem">Shop item.</param>
	public static int Price (string shopItem)
	{
		int price = Data.Shared["ShopItems"].d[shopItem].d["CashPrice"].i;
		if (price != 0) return price;
		else {
			Debug.Log("[ShopItems] Price for: " + shopItem + " returned 0!");
			return 0;
		}
	}

	/// <summary>
	/// Return the price as a formated string.
	/// </summary>
	/// <returns>The as string.</returns>
	/// <param name="shopItem">Shop item.</param>
	public static string PriceAsString(string shopItem)
	{
		string priceAsString ="";
		priceAsString = "$" + GenericFunctionsScript.AddSeparatorInInt(Price(shopItem), ",");
		return priceAsString;
	}

	/// <summary>
	/// Determines if you can afford the specified shopItem.
	/// </summary>
	/// <returns><c>true</c> if can afford the specified shopItem; otherwise, <c>false</c>.</returns>
	/// <param name="shopItem">Shop item.</param>
	public static bool CanAfford (string shopItem)
	{
		if (Price(shopItem) <= GameData.cash) return true;
		else return false;
	}

}
