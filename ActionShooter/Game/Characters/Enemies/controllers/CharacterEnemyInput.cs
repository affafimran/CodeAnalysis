using UnityEngine;
using System.Collections;

public class CharacterEnemyInput : CharacterInputController
{
	private Enemy enemy;
	public enum States{SPAWN, IDLE, SPAWNHOME, HOME, ATTACK} // testing SPAWN
	public States state = States.SPAWN;

	public CharacterEnemyData AIData;
	public TargetData targetData;

	private Vector3 sourceLocation;
	private Vector3 targetLocation; // location to go to afterspawning
	private float speed;
	private float targetLocationTimer;

	protected override void InitializeSpecific()
	{
		enemy = gameObject.GetComponent<Enemy>();
		AIData = new CharacterEnemyData();
		AIData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["CharactersAIData"].d[enemy.characterData.AIData].d, AIData) as CharacterEnemyData;
		AIData.sourceAttackRadius = AIData.attackRadius;
		targetData = TargetManager.GetTargetData(gameObject); // init
		if (!enemy.characterController.isGrounded) sourceLocation = CheckSourceLocation(gameObject.transform.position);
		else sourceLocation = gameObject.transform.position;
		targetLocation = CheckTargetLocation(enemy.targetLocation);
		speed = enemy.characterData.speed;
		targetLocationTimer = (Vector3.Distance(gameObject.transform.position, targetLocation)/speed);
	}

	void Update()
	{
		// here we think about what to do!
		targetData = TargetManager.GetTargetData(gameObject);
		// PIETER: JE MOET DIT NIET VERGETEN, HOE LASTIG HET OOK IS!
//		targetInSight = targetData.gameObject == hitData.gameObject;
//		// This does not work. Yet.
//		if (!targetInSight)
//		{
////			if (targetData ==  null) Debug.Log("[CharacterAIInput] TargetData is null.");
////			else if (targetData.gameObject ==  null) Debug.Log("[CharacterAIInput] TargetData GameObject is null.");
////			if (hitData ==  null) Debug.Log("[CharacterAIInput] HitData is null.");
////			else if (hitData.gameObject ==  null) Debug.Log("[CharacterAIInput] HitData GameObject is null."); // It turned out this is the nasty one.
//			if (targetData.gameObject != null && hitData.gameObject != null) 
//			{
//				GameObject tempGo;
//				tempGo = GenericFunctionsScript.FindChild(targetData.gameObject, hitData.gameObject.name); // If the hitData.gameobject is null, this will error.
//				if (tempGo != null )
//				{
//					targetInSight = true;
//				}
//			}
//		}
		verticallyInRange = Mathf.Abs(targetData.verticalOffset) <= 0.5f;

		switch (state)
		{		
			case States.SPAWN: // NO VERTICAL CHECK
				RotateToPosition(targetLocation);	
				targetLocationTimer -= Time.deltaTime;
				if (Vector3.Distance(gameObject.transform.position, targetLocation) <= 0.5f || targetLocationTimer <= 0 || !CheckFutureVerticalOffset(sourceLocation)) state = States.IDLE;
				if(targetData.distance <= AIData.homeRadius) state = States.HOME; // if target comes in range
				verAxis = 1f;				
				break;

			case States.IDLE:
				if(targetData.distance <= AIData.homeRadius) state = States.HOME;
				verAxis = 0f;
				primaryFire = false;
				break;

			case States.HOME:
				RotateToPosition(targetData.position);
				if(targetData.distanceLeveled <= AIData.attackRadius) {state = States.ATTACK; break;}
				if(targetData.distance > AIData.homeRadius) {state = States.IDLE; break;}
				if(!CheckFutureVerticalOffset(gameObject.transform.position)) {state = States.IDLE; verAxis = 0f; break;}
				verAxis = 1f;
				primaryFire = false;	
				break;

			case States.ATTACK:
				if(targetData.distance > AIData.homeRadius || !targetInSight){state = States.IDLE; break;}
				else if(targetData.distanceLeveled > AIData.attackRadius){state = States.HOME; break;}	
				RotateToPosition(targetData.position);
				verAxis = 0f;	
				primaryFire = true;
				break;
		}
	}

	// Still dont like this
	public override Vector3 GetBulletDirection(Vector3 aPosition) 
	{ 
		if (!targetData.target) return gameObject.transform.forward;
		return ((targetData.position + new Vector3(0, 1, 0)) - aPosition).normalized; 

	}

	void RotateToPosition(Vector3 aPosition)
	{
		Vector3 tLookAt = Vector3.Lerp(gameObject.transform.position, aPosition, 0.9f);
		tLookAt.y = gameObject.transform.position.y;
		gameObject.transform.LookAt(tLookAt);
	}

	Vector3 CheckSourceLocation(Vector3 aSourceLocation)
	{
		Vector3 position = gameObject.transform.position+new Vector3(0, 1f, 0);
		Vector3 direction = Vector3.down;
		
		Ray ray = new Ray(position, direction);
		RaycastHit rayCastHit;
		
		if (Physics.Raycast(ray, out rayCastHit, 10f, ProjectileManager.projectileLayerMask)) return rayCastHit.point;
		else return aSourceLocation;
	}

		
	Vector3 CheckTargetLocation(Vector3 aTargetLocation)
	{
		Vector3 position = gameObject.transform.position+new Vector3(0, 1f, 0);
		Vector3 direction = (aTargetLocation+new Vector3(0, 1f, 0)) - gameObject.transform.position;
		
		Ray ray = new Ray(position, direction.normalized);
		RaycastHit rayCastHit;
		
		if (Physics.Raycast(ray, out rayCastHit, direction.magnitude, ProjectileManager.projectileLayerMask))
		{
			return gameObject.transform.position + (gameObject.transform.forward * (rayCastHit.distance-0.5f));
		} else return aTargetLocation;
	}

	bool CheckFutureVerticalOffset(Vector3 aSourceLocation)
	{
		AIData.attackRadius = AIData.sourceAttackRadius;
		Vector3 position = aSourceLocation+new Vector3(0,1f, 0);
		position = position + (gameObject.transform.forward * 1.0f);
		Ray ray = new Ray(position, Vector3.down);
		RaycastHit rayCastHit;
		bool result = Physics.Raycast(ray, out rayCastHit, 1.5f, ProjectileManager.projectileLayerMask);
		if (!result) AIData.attackRadius = AIData.sourceAttackRadius * 3;
		return result;
	}

	public override void Reset()
	{
		horAxis = 0;
		verAxis = 0;
		
		primaryFire = false;
		dive = false;
		jump = false;

		state = States.IDLE;
	}
}

