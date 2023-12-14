using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class VehicleData {
	
	public VehicleManager.TYPES type = VehicleManager.TYPES.Unkown;
	public string prefab 			 = "None";
	public string typeSetting 		 = "None";
	public string cameraSetting      = "Default";
	//public string audioSetting		 = "None";	

	public int score 				 = 0;
	public float health 			 = 0;
	public bool destroyed			 = false;

	public bool dynamic              = false;
	public int mass                  = 0;

	public string impact			 = "";
	public string explosion			 = "";
	public float lifeTime			 = 0;

	internal bool damageEffect       = false;
	internal VehicleDamageEffect vehicleDamageEffect = null;
	
	public List<string> spawners = new List<string>(); // list of spawners to be created when destroyed (think bonusses, enemies ;))
	public List<string> pickUps  = new List<string>(); // list of pickups to be created when destroyed, easier/faster then through spwaners
}