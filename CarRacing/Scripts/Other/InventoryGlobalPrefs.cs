// Description : Inventory use on script MCRTestingTrack.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "Inventory/List TestCar", order = 1)]
public class InventoryGlobalPrefs : ScriptableObject {
	public List<ItemGlobalPrefs> 				inventoryItem = new List<ItemGlobalPrefs> ();

}