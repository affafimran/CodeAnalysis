// Descrition : Use to create list of car that could be used by players in a race
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Inventory/ListCar", order = 1)]
public class InventoryCar : ScriptableObject {
	public List<ItemCar> 	inventoryItem = new List<ItemCar> ();
	public bool				b_mobile = false;
	public float 			mobileMaxSpeedOffset = 0;
	public float 			mobileWheelStearingOffsetReactivity = 0;
	public bool				b_Countdown = true;
	public bool				b_LapCounter = true;
}
