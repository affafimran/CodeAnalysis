using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterBossData
{
	public bool active = false;
	public string prefab = "";
	// Stage and dummy variables
	public List<GameObject> stageList = new List<GameObject> ();
	public int amountOfStages = 1;
	public int currentStage = 0;
	public float switchStageHealth = 0;
	public List<GameObject> dummyList = new List<GameObject>();
	public GameObject targetDummy;
	public Vector3 targetPosition;
	public int amountOfDummys = 1;
	public int currentDummy = 0;
	
	public float attackTimer;// Attack Variable
	// Dodge Variables
	public int amountOfHitsTaken;
	public Vector3 dodgePosition;
	public bool dodged = false;
	// Taunt Variables
	public AudioSource tauntSound = null;
	public float tauntSoundTimer = 0;
	public float tauntTimer = 0;
}
