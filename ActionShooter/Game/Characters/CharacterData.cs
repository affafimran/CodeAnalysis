using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// NEEDS WORK, more data!!!
[System.Serializable]
public class CharacterData
{
	public bool AI          = false;
	public string AIData    = "None";

	public bool playerDead  = false;
	public string prefab    = "";
	public string weapon    = "";
	public string explosive = "";
	public string power     = "";
	public float health     = 100f;
	[HideInInspector] public float maxHealth  = 100f;
	public float healthAsPercentage = 1.0f;
	public float bulletTimeAsPercentage = 1.0f;
	public bool godMode     = false;
	public bool autoAim     = false;
	public float speed      = 13f;
	public float jumpHeight = 10f;
	public int score        = 0;

	// Spawners & pickups
	public List<string> spawners = new List<string>(); // list of spawners to be created when destroyed (think bonusses, enemies ;))
	public List<string> pickUps  = new List<string>(); // list of pickups to be created when destroyed, easier/faster then through spwaners
}