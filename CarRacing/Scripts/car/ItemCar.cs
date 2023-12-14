// Descrition : Use to create list of car that could be used by players in a race
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]    
public class ItemCar{
	public List<GameObject> Cars = new List<GameObject>();												// List of cars for players or CPUs
}
