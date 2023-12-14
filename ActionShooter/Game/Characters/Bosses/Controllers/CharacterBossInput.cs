using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterBossInput : CharacterInputController {

	public Boss boss;
	public CharacterBossData bossData;
	public TargetData targetData;
	private CharacterMotor characterMotor;

	// States
	public enum States{IDLE, MOVE, DODGE, ATTACK, SPAWNENEMIES, STAGESWITCH, TAUNT, WAIT}
	public States state = States.IDLE;
	public States previousState = States.IDLE; // This is used to return to the previous state after dodging

	private Ray ray;
	private RaycastHit  rayCastHit;
	private LayerMask layerMask;

	public float deltaTime; // easy access

	/////////////////////////////////////////////////////////////////////
	/// Initialize
	/////////////////////////////////////////////////////////////////////
	protected override void InitializeSpecific()
	{
		boss = gameObject.GetComponent<Boss> ();
		bossData = boss.bossData;
		characterMotor = GetComponent<CharacterMotor> ();

//		bossData.tauntSound = Scripts.audioManager.PlaySFX3D ("Characters/Enemies/"+bossData.prefab+"/"+bossData.prefab+"Taunt", this.gameObject, "Aiming");
//		bossData.tauntSound.enabled = false;

		targetData = TargetManager.GetTargetData (gameObject);

		GetStages ();
		GetDummys ();
	}
	/////////////////////////////////////////////////////////////////////
	/// Update
	/////////////////////////////////////////////////////////////////////
	void Update()
	{
		if (Data.pause) return;
		if (!bossData.active) return;

		targetData = TargetManager.GetTargetData (gameObject);

		deltaTime = Time.deltaTime;

		// We need to go to the next stage
		if ((boss.characterData.health / boss.characterData.maxHealth) * 100 <= bossData.switchStageHealth) 
		{
			// If we should always be able to go to stageswitch we need to reset all the vars used by other states.
			// Taunt State
			bossData.tauntTimer = 0;
			//bossData.tauntSound.enabled = false; TEMP
			// Dodge State
			bossData.dodged = false;
			// Attack State
			primaryFire = false;
			bossData.attackTimer = 0;
			// go to the stageSwitch
			state = States.STAGESWITCH;
		}

		switch(state)
		{
		case States.IDLE: // Idle
			CheckDummy();
			break;
		case States.TAUNT:
			// sound Timer
			bossData.tauntSoundTimer += deltaTime;
			if (bossData.tauntSoundTimer > bossData.tauntSound.clip.length) bossData.tauntSound.enabled = false;
			if (bossData.tauntSoundTimer > bossData.tauntSound.clip.length + 3/bossData.tauntSound.clip.length) // Short clip = more time inbetween, longer clip = less time inbetween
			{
				bossData.tauntSoundTimer = 0;
				bossData.tauntSound.enabled = true;
			}
			// timer until taunting is done
			bossData.tauntTimer += deltaTime;
			// go to the next dummy
			if (bossData.tauntTimer > 5) // Taunting lasts 5 sec
			{
				bossData.tauntTimer = 0;
				bossData.tauntSound.enabled = false;
				bossData.currentDummy++;
				CheckDummy();
			}
			break;
		case States.WAIT:
			// look at target
			RotateToTarget(targetData.position);
			// Check if the player is in line of sight
			if (LineOfSightWithPlayer())
			{
				bossData.currentDummy++;
				CheckDummy();
			}
			break;
		case States.STAGESWITCH: // Starting next stage
			// go to next stage, if there is no next stage it will endlessly repeat the last stage
			if (bossData.currentStage + 1 < bossData.amountOfStages && 
			    (boss.characterData.health / boss.characterData.maxHealth) * 100 <= bossData.switchStageHealth) bossData.currentStage++;
			bossData.currentDummy = 0;
			GetDummys();
			CheckDummy();
			break;
		case States.MOVE: // Moving to the next dummy
			// If the target position is on an object and that object gets destroyed we need to get a new position to move to.
			bossData.targetPosition = TargetPosition(bossData.targetDummy);
			// if we are at the dummy check new behaviour
			//Debug.Log (Vector3.Distance(bossData.targetPosition, transform.position));
			if (Vector3.Distance(bossData.targetPosition, transform.position) <= 0.5f) 
			{
				Reset();
				// If it was a move dummy we should move on to the next dummy, else we should get the behaviour from the dummy.
				if (GetBehaviourFromDummy(bossData.targetDummy) == "Move") bossData.currentDummy++;
				// the jump dummy should do the same as the move dummy, but the boss will jump when he reaches the jump dummy.
				if (GetBehaviourFromDummy(bossData.targetDummy) == "Jump") { bossData.currentDummy++; jump = true; }
				CheckDummy();
				return;
			}
			// look at the dummy while moving
			RotateToTarget(bossData.targetPosition);
			// if we aren't at the location (This prevents the boss from "jiggeling" in the air when he is at the position, but jumped to high and try's to move to the position).
			if (Vector3.Distance(bossData.targetPosition, new Vector3(transform.position.x, bossData.targetPosition.y, transform.position.z)) > 0.5f)
			{
				verAxis = 1; // move towards the target 
				// Don't move forward if we are jumping
				if (jump) verAxis = 0.01f;
			}
			else verAxis = 0;
			Jump ();
			break;
		case States.DODGE: // Dodge a bullet from the hammer
			bossData.dodged = true; // we are dodging
			RotateToTarget(bossData.dodgePosition); // rotate to the dodgePosition
			verAxis = 1; // move forward to the dodgePosition
			if (Vector3.Distance(bossData.dodgePosition, transform.position) <= 0.5f) // if we are close to the dodge position go back to the previous state
			{
				verAxis = 0; // stop walking
				state = previousState; // return to previous state
				return;
			}
			if (jump) verAxis = 0.01f;
			Jump ();
			break;
		case States.SPAWNENEMIES: // Spawn enemies to attack the hammer
			// set all spawners active
			foreach (Spawner spawner in bossData.targetDummy.GetComponentsInChildren<Spawner>())
			{
				spawner.spawnerData.range = 1000;
				spawner.spawnerActive = true;
			}
			// next dummy
			bossData.currentDummy++;
			CheckDummy();
			break;
		case States.ATTACK: // Attack the hammer
			RotateToTarget(targetData.position); // look at target
			bossData.attackTimer += deltaTime;
			if (bossData.attackTimer < 5) // attack for 5 seconds
			{
				primaryFire = true;
			}
			if (bossData.attackTimer >= 5) 
			{
				primaryFire = false;
				bossData.attackTimer = 0;
				bossData.currentDummy++;
				CheckDummy();
			}
//			if (bossData.amountOfHitsTaken >= 5) // we are going to dodge after 5 hits
//			{
//				primaryFire = false;
//				bossData.amountOfHitsTaken = 0;
//				bossData.dodgePosition = DodgePosition();
//				previousState = state;
//				state = States.DODGE;
//			}
			boss.animator.SetBool("Fire1", primaryFire);
			break;
		}
		// Set the jump animator. // (DG) I think this is some sort of fallback. Disabling it for now since it errors on character without jump.
//		boss.animator.SetBool("Jump", jump);
	}
	/////////////////////////////////////////////////////////////////////
	/// check the dummy for the boss behaviour
	/////////////////////////////////////////////////////////////////////
	void CheckDummy()
	{
		// if we dodged we need to return to the dummy first (To make sure we can get to the next dummy, because if we can't the dummy position is wrong and we might need a move dummy inbetween)
		// if we are in moving state we just have moved back to the previous state and we shouldn't go futher back. 
		if (bossData.dodged && state != States.MOVE) bossData.currentDummy--;
		// set the new targetDummy
		bossData.targetDummy = bossData.dummyList[bossData.currentDummy];
		bossData.targetPosition = TargetPosition(bossData.targetDummy);
		// save the behaviour to a string
		string nextBehaviour = GetBehaviourFromDummy(bossData.targetDummy);
		// check if we are at the targetDummy to start our behaviour else we need to keep moving
		if (Vector3.Distance(bossData.targetPosition, transform.position) > 1.5f)
		{
			nextBehaviour = "Move";
		}
		else if (bossData.dodged) // we have dodged and we are back at the dummy we dodged from we can continue to the next dummy now
		{
			bossData.dodged = false;
			bossData.currentDummy++;
			CheckDummy();
			return;
		}
		SetState (nextBehaviour);
	}
	/////////////////////////////////////////////////////////////////////
	/// set the state and save the previous state
	/////////////////////////////////////////////////////////////////////
	void SetState(string aState)
	{
		Debug.Log("[BossAIInput] SetState: " + aState);
		previousState = state;
		// set the right behaviour
		switch(aState)
		{
		case "Attack":
			bossData.amountOfHitsTaken = 0;
			state = States.ATTACK;
			break;
		case "SpawnEnemies":
			// enable the spawners
			SetSpawnersInDummysActive(bossData.targetDummy, true);
			state = States.SPAWNENEMIES;
			break;
		case "Taunt":
			bossData.tauntSound.enabled = true;
			state = States.TAUNT;
			break;
		case "Wait":
			state = States.WAIT;
			break;
		case "Move":
			state = States.MOVE;
			characterMotor.jumping.baseHeight = (bossData.targetPosition.y - transform.position.y) * 1.5f;
			break;
		case "StageSwitch":
			state = States.STAGESWITCH;
			break;
		}
	}
	/////////////////////////////////////////////////////////////////////
	/// get the boss behaviour from the dummy name
	/////////////////////////////////////////////////////////////////////
	string GetBehaviourFromDummy(GameObject aDummy)
	{
		string aString;
		// save the behaviour to aString
		if (bossData.currentDummy < 9) aString = aDummy.name.Substring (6, aDummy.name.Length - 6);
		else aString = aDummy.name.Substring (7, aDummy.name.Length - 7);
		// if its an attack behaviour, change weapon accordingly 
		if (aString.Contains("Attack"))
		{
			ChangeWeapon(aString.Substring(6, aString.Length - 6));
			aString = "Attack";
		}
		return aString;
	}
	/////////////////////////////////////////////////////////////////////
	/// get the stages for the boss fight
	/////////////////////////////////////////////////////////////////////
	void GetStages()
	{
		bossData.stageList.Clear ();
		bool lastStageFound = false;
		int stageCount = 1;
		int mission = MissionManager.missionData.mission;
		while(!lastStageFound)
		{
			if (GameObject.Find ("BossMission"+mission+"/BossDummies/Stage" + stageCount) != null)
			{
				bossData.stageList.Add(GameObject.Find ("BossMission"+mission+"/BossDummies/Stage" + stageCount));
				// TEMP
				// spawners are always set to active so this is Temporary until that is changed
				SetSpawnersInDummysActive(bossData.stageList[stageCount-1], false);
				// TEMP
				stageCount++;
			}
			else { lastStageFound = true; stageCount--; }
		}
		bossData.amountOfStages = stageCount;
	}
	/////////////////////////////////////////////////////////////////////
	/// Set the spawners active or inactive
	/////////////////////////////////////////////////////////////////////
	void SetSpawnersInDummysActive(GameObject aDummy,bool aBool)
	{
		foreach(Spawner spawner in aDummy.GetComponentsInChildren<Spawner>())
		{
			spawner.spawnerActive = false;
			spawner.enabled = aBool;
		}
	}
	/////////////////////////////////////////////////////////////////////
	/// get the dummy's for the stage
	/////////////////////////////////////////////////////////////////////
	void GetDummys()
	{
		bossData.dummyList.Clear ();
		GameObject Stage = bossData.stageList [bossData.currentStage];
		int dummyCount = 1;
		foreach(Transform trans in Stage.GetComponentsInChildren<Transform>())
		{
			// dummy's
			if (trans.gameObject != Stage && trans.name.Contains("Dummy" + dummyCount)) 
			{
				bossData.dummyList.Add(trans.gameObject);
				dummyCount++;
			}
			// stageswitchhealth percentage
			if (trans.gameObject != Stage && trans.name.Contains("StageSwitchHealth"))
			{
				string getHealth = trans.name.Substring(17, trans.name.Length - 17);
				bossData.switchStageHealth = float.Parse(getHealth);
			}
		}
		bossData.amountOfDummys = dummyCount;
	}
	/////////////////////////////////////////////////////////////////////
	/// Change the boss his weapon
	/////////////////////////////////////////////////////////////////////
	void ChangeWeapon(string aWeapon)
	{
		//if (aWeapon == "") return;
		//boss.currentWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), aWeapon);
	}
	/////////////////////////////////////////////////////////////////////
	/// Rotate the boss towards the target at its own height
	/////////////////////////////////////////////////////////////////////
	void RotateToTarget(Vector3 aPosition)
	{
		Vector3 aTargetLocation = new Vector3 (aPosition.x, transform.position.y, aPosition.z);
		transform.LookAt (aTargetLocation);
	}
	/////////////////////////////////////////////////////////////////////
	/// We have to dodge the hammers attacks so we are taking a new position
	/////////////////////////////////////////////////////////////////////
	Vector3 DodgePosition()
	{
		float aRange = 10; // random position range
		bool isClear = false; // is the way to the new position clear?
		Vector3 newPosition = Vector3.zero; //  Vector3.zero; // new position
		layerMask = (1 << 0) | (1 << 13) | (1 << 14);

		int iterations = 0;
		int maxIterations = 10;

		// the way isn't clear yet
		while (!isClear)
		{
			iterations++;
			if (iterations == maxIterations){
				Debug.Log("We've reached maxIterations.");
				return transform.position;
			}

			// get the random position
			float Xpos = UnityEngine.Random.Range(-aRange, aRange);
			float Zpos = UnityEngine.Random.Range(-aRange, aRange);
			newPosition = new Vector3(transform.position.x + Xpos, transform.position.y, transform.position.z + Zpos);
			// distance to the random position
			float distance = Vector3.Distance(transform.position, newPosition);
			// set the ray towards the position
			ray = new Ray(transform.position, (newPosition - transform.position).normalized);
			if (Physics.Raycast(ray, out rayCastHit, distance, layerMask)) {}
			else
			{
				ray = new Ray(newPosition + Vector3.up, Vector3.down);
				// ray from the new position to the ground for a position where the boss can stand
				if (Physics.Raycast(ray, out rayCastHit, 100, layerMask))
				{
					if (transform.position.y - rayCastHit.point.y < 1)
					{
						newPosition = rayCastHit.point;
						isClear = true;
					}
				}
			}
		}
		return newPosition;
	}
	/////////////////////////////////////////////////////////////////////
	/// Gets the targetPosition from the dummy
	/////////////////////////////////////////////////////////////////////
	Vector3 TargetPosition(GameObject aGameObject)
	{
		ray = new Ray (aGameObject.transform.position + new Vector3(0, 1, 0), Vector3.down);
		layerMask = (1 << 0) | (1 << 13) | (1 << 14);
		if (Physics.Raycast(ray, out rayCastHit, 100, layerMask))
		{
			//Debug.Log(rayCastHit.collider + " - " + rayCastHit.point);
			return rayCastHit.point;
		}
		Debug.Log("No hit");
		return aGameObject.transform.position;
	}
	/////////////////////////////////////////////////////////////////////
	/// Wait for the player to get in Line of Sight before continueing
	/////////////////////////////////////////////////////////////////////
	bool LineOfSightWithPlayer()
	{
		ray = new Ray (transform.position + new Vector3(0, 1.7f, 0), (targetData.position - transform.position).normalized);
		Debug.DrawRay (ray.origin, ray.direction * 50);
		layerMask = (1 << 0) | (1 << 10) | (1 << 13) | (1 << 14) | (1 << 15);
		if (Physics.Raycast(ray, out rayCastHit, 50, layerMask))
		{
			if (rayCastHit.collider.gameObject == targetData.gameObject) return true;
		}
		return false;
	}
	/////////////////////////////////////////////////////////////////////
	/// Jumping behaviour will be done here
	/////////////////////////////////////////////////////////////////////
	void Jump()
	{
		characterMotor.grounded = GroundedCheck ();
		// This is just to prevent the bug that makes katie run in to a wall while she thinks she is jumping. (If she doesn't make the jump towards something).
		if (characterMotor.grounded && jump && characterMotor.movement.velocity.sqrMagnitude < 10) 
		{
			jump = false;
			return;
		}
		// If there is something infront of the boss, jump
		ray = new Ray(transform.position + new Vector3(0, 0.5f, 0) + transform.forward * 0.2f, transform.forward);
		layerMask = (1 << 0) | (1 << 13) | (1 << 14);
		if (Physics.Raycast(ray, out rayCastHit, 0.5f, layerMask))
		{
			if (characterMotor.jumping.baseHeight <= 1)
			{
				characterMotor.jumping.baseHeight = (bossData.targetPosition.y - transform.position.y) * 1.5f;
				if (characterMotor.jumping.baseHeight <= 1)
				{
					characterMotor.jumping.baseHeight = rayCastHit.collider.gameObject.GetComponent<Renderer>().bounds.size.y + 2;
				}
			}
			jump = true;
			return;
		}
		// If the boss is going to run underneath something, make the boss jump before.
		ray = new Ray(transform.position + transform.forward * 0.7f + transform.up * 0.5f, transform.up);
		layerMask = (1 << 0) | (1 << 13) | (1 << 14);
		if (Physics.Raycast(ray, out rayCastHit, 1.5f, layerMask))
		{
			if (characterMotor.jumping.baseHeight <= 1)
			{
				characterMotor.jumping.baseHeight = (bossData.targetPosition.y - transform.position.y) * 1.5f;
				if (characterMotor.jumping.baseHeight <= 1)
				{
					characterMotor.jumping.baseHeight = rayCastHit.collider.gameObject.GetComponent<Renderer>().bounds.size.y + 2;
				}
			}
			jump = true;
			return;
		}
		// If we don't run into something and our y position is close to the target position we can stop jumping
		if (bossData.targetPosition.y - transform.position.y < 1) jump = false;
	}
	/////////////////////////////////////////////////////////////////////
	/// Get the bullet direction
	/////////////////////////////////////////////////////////////////////
	public override Vector3 GetBulletDirection(Vector3 aPosition) 
	{ 
		if (!targetData.target) return gameObject.transform.forward;
		return ((targetData.position + new Vector3(0, 1, 0)) - aPosition).normalized; 		
	}
	/////////////////////////////////////////////////////////////////////
	/// Check if we are in the air (Seems to be bugging sometimes) Not sure why. Have to discuss with Pieter. This is probaly temporary
	/// What I think the problem is that with Katie the 0.01 groundedNormal.y isn't enough to check if she is on the ground, atleast not always.
	/// If you comment this function and let Stage 1 play a few times Katie will get stuck ontop of the boxes because her grounded isn't going back to true.
	/////////////////////////////////////////////////////////////////////
	bool GroundedCheck()
	{
		ray = new Ray (transform.position + Vector3.up, Vector3.down);
		Debug.DrawRay (ray.origin, ray.direction * 1.05f);
		layerMask = (1 << 0) | (1 << 13) | (1 << 14);
		if (Physics.Raycast(ray, out rayCastHit, 1.05f, layerMask))
		{
			return true;
		}
		return false;
	}
	/////////////////////////////////////////////////////////////////////
	/// Reset the BossInputController variables
	/////////////////////////////////////////////////////////////////////
	public override void Reset()
	{
		verAxis = 0;
		horAxis = 0;

		primaryFire = false;
		secondaryFire = false;
		use = false;
		jump = false;
		dive = false;
	}
}
