using UnityEngine;
using System.Collections;

public class HelicopterAIController : VehicleInputController {

	private Helicopter helicopter;
	private HelicopterData helicopterData;

	public TargetData targetData;

	// states
	public enum State {INACTIVE, IDLE, AIM, FIRE, COOLDOWN, MOVING, LIFTOFF} // These are the AI states
	public State state = State.LIFTOFF; // current state

	// Firing vars
	public int fired = 0;
	public int cooldown = 5; // this is the amount of shots fired before going to cooldown
	public float cooldownTimer = 2;

	// Movement Vars
	public bool clearPath = false;
	public int currentPosition = 0;
	public Vector3[] movingPositions;
	public float movementTimer = 0;
	public float upAndDownTimer = 0;
	public float upAndDownMovement = 0.25f;

	public Vector3 startPosition = Vector3.zero;

	private float deltaTime;

	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		helicopter = GetComponent<Helicopter> ();
		helicopterData = helicopter.helicopterData;

		helicopterData.rigidBody.drag = 2;
		helicopterData.rigidBody.angularDrag = 2;
		helicopterData.rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		helicopterData.rigidBody.useGravity = false;

		//activate sound
		helicopterData.rotorSound.enabled = true;
	}
	
	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		
		// get target, no target exit
		targetData = TargetManager.GetTargetData(gameObject);
		if (!targetData.target) return; // return when no target. However, we could comment this line. If we use the bool to check target we can still run other code without aiming.
		
		// store deltaTime for easy access
		deltaTime = Time.deltaTime;

		// calc rof
		helicopterData.currentRof -= deltaTime;

		switch(state)
		{
		case State.INACTIVE:
			break;
		case State.LIFTOFF: // flying up to the baseheight as soon as the rotorspeed is high enough

			// WAT EEN TROEP!!!!

			// Start the rotors to turn
			if (helicopterData.rotorSpeed < 1200) 
			{ 
				helicopterData.rotorSpeed += 6f; 
				helicopterData.baseHeight = transform.position.y + 25; // set baseheight
				return;
			}
			// baseHeight has not been reached yet
			if (transform.position.y < helicopterData.baseHeight)
			{
//				helicopterData.rigidBody.useGravity = false;
				// Keep going up untill the baseHeight is reached
				transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, 25, 0), 0.25f * deltaTime);
			}
			else // baseHeight has been reached
			{
				SetPositions();
				state = State.IDLE;
			}
			break;
			// cooling down
		case State.COOLDOWN:
			cooldownTimer -= deltaTime;
			if (cooldownTimer <= 0)
			{
				cooldown = 5;
				cooldownTimer = 3;
				state = State.IDLE;
			}
			break;
			// Idle
		case State.IDLE:
			// go to aim state
			if (targetData.target && targetData.distance <= helicopterData.idleToAimRange) state = State.AIM;
			break;
			// Move to new position
		case State.MOVING:
			// Check if there is a clear path to the next position
			if (!clearPath) CalculateNewPosition();
			if (clearPath) // There is a clear path
			{
				// timer to move
				movementTimer += deltaTime / 2;
				// Lerp towards the new position
				transform.position = Vector3.Lerp(startPosition, movingPositions[currentPosition], movementTimer);
				// Aim the helicopter towards the target while moving
				AimAI();
				// We have reached the new position reset all the variables and go to IDLE state
				if (Vector3.Distance(transform.position, movingPositions[currentPosition]) < 1)
				{
					movementTimer = 0;
					cooldownTimer = 3;
					fired = 0;
					cooldown = 5;
					state = State.IDLE;
				}
			}
			break;
		case State.AIM: // Aim at target
			AimAI(); // well, aim it!
			if (targetData.target && targetData.distance > helicopterData.idleToAimRange) state = State.IDLE; // return to idle
			break;
			// Fire at target
		case State.FIRE:
			helicopterData.barrel.transform.LookAt(targetData.position);
			Fire(helicopterData.muzzleFlash1, targetData.gameObject);
			Fire(helicopterData.muzzleFlash2, targetData.gameObject);
			// Count down untill cooldown
			cooldown--;
			// Reset Rate of Fire
			helicopterData.currentRof = helicopterData.rof;
			// Cooldown yet?
			if (cooldown <= 0) 
			{
				fired++;
				// Start cooldown
				state = State.COOLDOWN;
				if (fired == 2) 
				{
					clearPath = false; // Reset the clearPath var
					startPosition = transform.position; // Save the current position
					state = State.MOVING; // Go to moving state
				}
				return;
			}
			// Return to the aim state
			state = State.AIM;
			break;
		}
//		if (state != State.LIFTOFF)
//		{
//			upAndDownTimer += deltaTime * 4;
//			// Move the helicopter up and down constantly
//			transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, helicopterData.baseHeight + upAndDownMovement, transform.position.z), upAndDownTimer);
//			if (upAndDownTimer >= 0.25f)
//			{
//				// Move up or down
//				upAndDownTimer = 0;
//				upAndDownMovement = -upAndDownMovement;
//			}
//		}
	}
	
	void AimAI()
	{
		// calculate direction and set a LookRotation
		Vector3 lookDirection =  targetData.position; //  new Vector3(targetData.position.x, tankData.turret.transform.position.y, targetData.position.z) - tankData.turret.transform.position;
		lookDirection.y = helicopterData.body.transform.position.y;
		lookDirection = (lookDirection-helicopterData.body.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
		lookRotation.eulerAngles = lookRotation.eulerAngles + new Vector3 (30, 0, 0);
		// slerp the turret
		helicopterData.body.transform.rotation = Quaternion.Slerp(helicopterData.body.transform.rotation, lookRotation,  helicopterData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = (targetData.position - helicopterData.barrel.transform.position).normalized;
		lookDirection.y = Mathf.Max(-0.3f, Mathf.Min(0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(helicopterData.body.transform.forward.x, lookDirection.y, helicopterData.body.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		helicopterData.barrel.transform.rotation = Quaternion.Slerp(helicopterData.barrel.transform.rotation, lookRotation, helicopterData.aimSpeed * deltaTime);	
		// Is the target in sight
		if (Quaternion.Angle(lookRotation, helicopterData.body.transform.rotation) < helicopterData.aimToFireAngle && state == State.AIM)
		{
			if (helicopterData.currentRof <= 0) state = State.FIRE; // Rate of Fire lower then 0
		}
	}

	public void Fire(GameObject muzzleFlash, GameObject target)
	{
		// Play weapon sound
		Scripts.audioManager.PlaySFX3D("Weapons/M4Fire", helicopterData.barrel, "FireGun");
		// Fire both guns at the target and activate the particle system
		ProjectileManager.AddProjectile(helicopterData.projectile, muzzleFlash.transform.position, helicopterData.barrel.transform.forward, new HitData(), target);
		muzzleFlash.EmitInChildren(20);
	}

	void SetPositions()
	{
		// Positions the helicopter will use to fly around with
		int movingRange = 25;
		movingPositions = new Vector3[5];
		movingPositions[0] = transform.position + new Vector3(			 0, 0, 			  0);
		movingPositions[1] = transform.position + new Vector3( movingRange, 0, 			  0);
		movingPositions[2] = transform.position + new Vector3(			 0, 0,  movingRange);
		movingPositions[3] = transform.position + new Vector3(-movingRange, 0, 			  0);
		movingPositions[4] = transform.position + new Vector3(			 0, 0, -movingRange);
	}

	// Check if we can see the new position
	public void CalculateNewPosition()
	{
		Ray ray;
		RaycastHit rayCastHit;
		LayerMask layerMask = ProjectileManager.projectileLayerMask;
		// Next position
		currentPosition++;
		// Check if we aren't at the last position
		if (currentPosition > 4) currentPosition = 0;
		// Normalize direction
		Vector3 tempDirection = Vector3.Normalize((movingPositions[currentPosition] - transform.position));
		// Distance between next position and current position
		float tempDistance = Vector3.Distance(transform.position, movingPositions[currentPosition]);
		// Add ray
		ray = new Ray(transform.position + tempDirection, tempDirection);		
		// Cast ray
		if(Physics.Raycast(ray, out rayCastHit, tempDistance, layerMask)) 
		{
			clearPath = false;
			return;
		}
		clearPath = true;
	}
	
	public override void Reset()
	{}
}