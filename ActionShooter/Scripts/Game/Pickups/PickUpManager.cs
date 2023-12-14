using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// PickUpManager
/// <para>Static class which manages everything related to pickups.</para>
/// </summary>
public class PickUpManager : MonoBehaviour
{
	public static int pickUpCounter = 0; // amount of pickups created
	public static GameObject pickupHolder; // holder to parent pickups to 
	public static List<GameObject> pickUps = new List<GameObject>(); // list of pickups
	public static int pickupLayer = LayerMask.NameToLayer("PickUps"); // layer
	public static bool pickUpMagnetActive; // global bool to see if the magnet is active

	/// <summary>
	/// Initialize the PickUpManager
	/// </summary>
	public static void Initialize()
	{
		GameObject tPickups = GameObject.Find("PickUps"); // find holder object
		if (tPickups == null) pickupHolder = new GameObject("PickUps"); // create one if not found
		else pickupHolder = tPickups; // store if found
		pickUpMagnetActive = false; // default false
	}

	/// <summary>
	/// Add a PickUp.
	/// </summary>
	/// <returns>The pick up.</returns>
	/// <param name="aType">A type.</param>
	/// <param name="aPosition">A position.</param>
	public static GameObject AddPickUp(string aType, Vector3 aPosition) {return AddPickUp(aType, aPosition, false);}
	public static GameObject AddPickUp(string aType, Vector3 aPosition, bool aFallingPickup)
	{
		++pickUpCounter; // coutn for whatever reason
		if (aType == "None") return null; // Not too happy with this, but alas...
		PickUpData pickUpData = new PickUpData(); // new data
		pickUpData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["PickUps"].d[aType].d, pickUpData) as PickUpData; // fill data
		GameObject tPickUp = Loader.LoadGameObject("PickUps/" + pickUpData.prefab + "_Prefab"); // create object
		if (tPickUp == null) return null; // just in case
		tPickUp.AddComponent<PickUp>().Initialize(pickUpData, aPosition, aFallingPickup); // add component
		tPickUp.transform.parent = pickupHolder.transform; // set to holder
		pickUps.Add(tPickUp); // add to list
		Scripts.map.CreateMapIcon(tPickUp, aType); // Add to minimap
		return tPickUp; // return the pickup object
	}

	/// <summary>
	/// Adds multiple pickups from list.
	/// </summary>
	/// <param name="aPosition">A position.</param>
	/// <param name="aPickups">A pickups.</param>
	public static void AddPickUps(Vector3 aPosition, List<string> aPickups) {AddPickUps(aPosition, aPickups, 1f);}
	public static void AddPickUps(Vector3 aPosition, List<string> aPickups, float aRadius)
	{
		if (aPickups.Count == 0) return; // empty list do nothing
		if (aPickups.Contains("Random")) AddPickUp(GetRandomPickupFromList(aPickups), aPosition); // get random pickup
		else{
			bool offset = false; // toggle
			if (aPickups.Count > 1) offset = true; // if we have more than 1 pickup in the list we're going to offset spawn them so they don't spawn ineach other
			foreach(string pickup in aPickups){ // go through list
				if (offset)	{ // create offset
					Vector2 offset2D = Random.insideUnitCircle * aRadius;
					Vector3 position = aPosition + new Vector3(offset2D.x, 0, offset2D.y);
					AddPickUp(pickup, position); // add pickup
				} else AddPickUp(pickup, aPosition); // one pickup
			}	
		}
	}

	/// <summary>
	/// Gets a random pickup from a list.
	/// </summary>
	/// <returns>The random pickup from list.</returns>
	/// <param name="aPickups">A pickups.</param>
	public static string GetRandomPickupFromList(List<string> aPickups)
	{
		aPickups.Remove("Random");
		int random = Random.Range(0, aPickups.Count);
		return aPickups[random];
	}

	/// <summary>
	/// Gets a random pickup from all pickups available in SharedData.txt.
	/// </summary>
	/// <returns>The random pickup.</returns>
	public static string GetRandomPickup()
	{
		int random = Random.Range(0, Data.Shared["PickUps"].d.Count);
		List<string> keys = new List<string>(Data.Shared["PickUps"].d.Keys);
		return keys[random];
	}

	/// <summary>
	/// Pickup up reward
	/// <para>This function actually awards the user with whatever the pickup is intended to do</para>
	/// </summary>
	/// <param name="aGameObject">A game object.</param>
	/// <param name="aType">A type.</param>
	/// <param name="aPickupData">A pickup data.</param>
	public static void PickUpReward(GameObject aGameObject, string aType, PickUpData aPickupData)
	{
		string interfaceType = aType; // Variable added so we can override what we  send to the interface.

		// [HARDCODED] this is all hardcoded to the Hammer
		// Ideally you chould check for Character and then award them. Could be challenging because not every character can be awarded the same pickups

		switch (aType){
			// Health
			case "HealthSmall": Scripts.hammer.characterData.health = Mathf.Min(Scripts.hammer.characterData.maxHealth, Scripts.hammer.characterData.health + 10.0f); break;
			case "HealthBig": Scripts.hammer.characterData.health = Scripts.hammer.characterData.maxHealth; break;
			
			// Primary weapon (awards random weapon of available weapons)
			case "Weapon": 
				int weaponsCount = WeaponManager.availableWeapons.Count;
				string randomWeapon = WeaponManager.availableWeapons[Random.Range(1, weaponsCount)];
				WeaponManager.WEAPONS aWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), randomWeapon); //(WeaponManager.WEAPONS)randomWeapon;
				Scripts.hammer.AddWeapon(aWeapon);
				interfaceType = aWeapon.ToString();
				break;
			
			// Secondary (currently there is only one explosive type, so when picked up we add ammo)
			case "Explosive": Scripts.hammer.explosive.explosiveData.ammo += Data.Shared["Explosives"].d["Grenade"].d["ammo"].i; break; // add ammo, but needs to be new...
			
			// Gadgets here (awards a random gadget, but only one you don't have yet)
			case "Gadget":				
				List<string> allGadgets = System.Enum.GetNames(typeof(GadgetManager.GADGETS)).ToList<string>();
				List<string> unEquippedGadgets = System.Enum.GetNames(typeof(GadgetManager.GADGETS)).ToList<string>();
				List<Gadget> equippedGadgets = Scripts.hammer.gadgets;
				foreach(Gadget gadget in equippedGadgets){
						unEquippedGadgets.Remove(gadget.type);
				}
				if (unEquippedGadgets.Count == 0) interfaceType = "GadgetBonus";
				else{
					int randomGadgetNumber = Random.Range(0, unEquippedGadgets.Count);
					interfaceType = unEquippedGadgets[randomGadgetNumber]; // Determine which one it is for the interface
				    int foundGadgetNumber = allGadgets.IndexOf(unEquippedGadgets[randomGadgetNumber]);
					Scripts.hammer.AddGadget((GadgetManager.GADGETS) foundGadgetNumber);
				}
			break;

			// Skins (texture changes for fun)
			case "SkinTommy": Scripts.hammer.ChangeSkin("SkinTommy"); break;
			case "SkinSonny": Scripts.hammer.ChangeSkin("SkinSonny"); break;
			case "SkinArnold": Scripts.hammer.ChangeSkin("SkinArnold"); break;
			case "SkinBruce": Scripts.hammer.ChangeSkin("SkinBruce"); break;
		}

		// Here we send it through to the interface!
		// NOTE: THE INTERFACE ONLY HANDLES TEXT, SCORE, VISUALS AND SOUND
		// THE INTERFACE DOES NOT(!) AWARD WEAPONS, HEALTH AND SUCH THAT IS TO BE DONE HERE
		Scripts.interfaceScript.gamePanelScript.PickUp(interfaceType, aPickupData.value);
	}

	/// <summary>
	/// Sets all pickups active or not
	/// </summary>
	/// <param name="aBool">If set to <c>true</c> a bool.</param>
	public static void SetPickupsActive(bool aBool)
	{
		PickUp tComponent;
		foreach (GameObject pickUp in pickUps){
			tComponent = pickUp.GetComponent<PickUp>();
			tComponent.pickUpActive = aBool;
		}	
	}

	/// <summary>
	/// Reset the PickupManager
	/// </summary>
	public static void Reset()
	{
		pickUpCounter = 0;
		pickUps = new List<GameObject>();
	}
}
