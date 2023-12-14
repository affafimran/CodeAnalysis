// Description : Class use with InventoryGlobalPrefs.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]                                                           
public class ItemGlobalPrefs
{
	public bool 						b_TestMode = false;							// Test mode activated or deactivated

	public List<GameObject> 			list_Cars	= new List<GameObject> ();		// List of cars that could be instantiate on scene
	public List<bool> 					list_playerType	= new List<bool> ();		// Know if the cars is manage by player or CPU
	public GameObject					Canvas_TestMode;							// canvas that display a text when test mode is activated
}