using UnityEngine;
using System.Collections;

public class ApacheAIController : VehicleInputController {
	
	private Apache apache;
	private ApacheData apacheData;

	public TargetData targetData;

	// states
	public enum State {INACTIVE, IDLE, AIM, FIRE, FIRE2, COOLDOWN, MOVING, LIFTOFF} // These are the AI states
	public State state = State.LIFTOFF; // current state

	// Rocket launcher vars
	private float rof2				= 0;
	private float currentRof2		= 0;

	// Firing vars
	public int fired = 0;
	public int currentLauncher = -1; // 1 = left side, -1 = right side
	public int cooldown = 5; // this is the amount of shots fired before going to cooldown
	public float cooldownTimer = 1;

	// Movement Vars
	public bool clearPath = false;
	public int currentPosition;
	public Vector3[] movingPositions;
	public float movementTimer = 0;
	public float upAndDownTimer = 0;
	public float upAndDownMovement = 0.25f;

	private Vector3 startPosition = Vector3.zero;

	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		apache = GetComponent<Apache> ();
		apacheData = apache.apacheData;

		apacheData.rigidBody.drag = 2;
		apacheData.rigidBody.angularDrag = 2;
		apacheData.rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		apacheData.rigidBody.useGravity = false;

		// activate sound
		apacheData.rotorSound.enabled = true;
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
		apacheData.currentRof -= deltaTime;
		currentRof2 -= deltaTime;

		switch(state)
		{
		case State.INACTIVE:
			break;
		case State.LIFTOFF: // flying up to the baseheight as soon as the rotorspeed is high enough
			// Activate the rotors speed
			if (apacheData.rotorSpeed < 20) 
			{ 
				apacheData.rotorSpeed += 0.1f;
				apacheData.baseHeight = transform.position.y + 25;
				return; 
			}
			// Apache hasn't reached baseHeight yet
			if (transform.position.y < apacheData.baseHeight)
			{
//				apacheData.rigidBody.useGravity = false;
				// Move towards baseHeight
				transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, 25, 0), 0.25f * deltaTime);
			}
			else // Reach baseHeight
			{
				SetPositions();
				state = State.IDLE;
			}
			break;
			// Cooling down
		case State.COOLDOWN:
			// Wait time until next fire round
			cooldownTimer -= deltaTime;
			if (cooldownTimer <= 0)
			{
				// Reset the amount of shots fired untill cooldown
				cooldown = 5;
				// Cooldowntimer reset
				cooldownTimer = 1;
				// Go to IDLE state
				state = State.IDLE;
			}
			break;
			// Idle
		case State.IDLE:
			// go to aim state
			if (targetData.target && targetData.distance <= apacheData.idleToAimRange) state = State.AIM;
			break;
			// Move to new position
		case State.MOVING:
			// Check if there is a clear path to the next position
			if (!clearPath) CalculateNewPosition();
			if (clearPath) // Path is clear
			{
				// timer to move
				movementTimer += deltaTime / 2;
				// Lerp towards the new position
				transform.position = Vector3.Lerp(startPosition, movingPositions[currentPosition], movementTimer);
				// Keep aiming at the target while moving
				Aim();
				// Have we arrive at the new position reset all the variables and go to the IDLE state
				if (Vector3.Distance(transform.position, movingPositions[currentPosition]) < 1)
				{
					movementTimer = 0;
					cooldownTimer = 1;
					fired = 0;
					cooldown = 5;
					state = State.IDLE;
				}
			}
			break;
		case State.AIM: // Aim at target
			Aim(); // well, aim it!
			if (targetData.target && targetData.distance > apacheData.idleToAimRange) state = State.IDLE; // return to idle
			break;
			// Fire at target
		case State.FIRE:
			// To make sure the bullets are fired in the right direction
			apacheData.gun.transform.LookAt(targetData.position + new Vector3(0, 0.5f, 0));
			Fire1();
			// Count down until cooling down
			cooldown--;
			// Reset the Rate of Fire
			apacheData.currentRof = apacheData.rof;
			// Cooldown yet?
			if (cooldown <= 0) 
			{
				fired++;
				// Go to cooldown
				state = State.COOLDOWN;
				return;
			}
			// Return to aiming state
			state = State.AIM;
			break;
		case State.FIRE2:
			// To make sure the rockets are fired in the right direction
			apacheData.gun.transform.LookAt(targetData.position + new Vector3(0, -1, 0));
			currentRof2 = rof2;// Reset the Rate of Fire
			currentLauncher = -currentLauncher; // switch fire side
			// Fire left launchers
			if (currentLauncher == 1)
			{
				Fire2(apacheData.muzzleFlash1);
				Fire2(apacheData.muzzleFlash2);
			}
			else // Fire right launchers
			{
				Fire2(apacheData.muzzleFlash3);
				Fire2(apacheData.muzzleFlash4);

				fired++;
				// Have we reached the amount of shots that should be fired?
				if (fired >= 3) 
				{
					clearPath = false; // Reset the clearPath var
					startPosition = transform.position; // Save the current position
					state = State.MOVING; // Go to moving state
				}
				return;
			}
			// Return to aiming state
			state = State.AIM;
			break;
		}
		if (state != State.LIFTOFF)
		{
			upAndDownTimer += deltaTime * 4;
			// Move the helicopter up and down constantly
			transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, apacheData.baseHeight + upAndDownMovement, transform.position.z), upAndDownTimer);
			if (upAndDownTimer >= 0.25f)
			{
				// Move up or down
				upAndDownTimer = 0;
				upAndDownMovement = -upAndDownMovement;
			}
		}
	}
	
	void Aim()
	{

		// calculate direction and set a LookRotation
		Vector3 lookDirection =  targetData.position; //  new Vector3(targetData.position.x, tankData.turret.transform.position.y, targetData.position.z) - tankData.turret.transform.position;
		lookDirection.y = apacheData.body.transform.position.y;
		lookDirection = (lookDirection-apacheData.body.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
		lookRotation.eulerAngles = lookRotation.eulerAngles + new Vector3 (30, 0, 0);	
		Quaternion lookRotationTurret = lookRotation;
		// slerp the turret
		apacheData.body.transform.rotation = Quaternion.Slerp(apacheData.body.transform.rotation, lookRotation,  apacheData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = (targetData.position - apacheData.gun.transform.position).normalized;
		lookDirection.y = Mathf.Max(-0.3f, Mathf.Min(0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(apacheData.body.transform.forward.x, lookDirection.y, apacheData.body.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		apacheData.gun.transform.rotation = Quaternion.Slerp(apacheData.gun.transform.rotation, lookRotation, apacheData.aimSpeed * deltaTime);	
		// Is the target in sight
		if (Quaternion.Angle(lookRotationTurret, apacheData.body.transform.rotation) < apacheData.aimToFireAngle && state == State.AIM)
		{
			apacheData.currentRof -= deltaTime; // calc rof
			if (fired < 2)
			{
				if (apacheData.currentRof < 0) state = State.FIRE; // Rate of Fire lower then 0
			}
			else
			{
				if (currentRof2 < 0) state = State.FIRE2;
			}
		}
	}

	public void Fire1()
	{
		// Play first weapon sound
		Scripts.audioManager.PlaySFX3D("Weapons/M4Fire", apacheData.gun, "FireGun");
		ProjectileManager.AddProjectile(apacheData.projectile, apacheData.muzzleFlash5.transform.position, apacheData.gun.transform.forward, new HitData(), new HitData().gameObject);
		apacheData.muzzleFlash5.PlayInChildren();
	}
	
	public void Fire2(GameObject muzzleFlash)
	{
		// Play second weapon sound
		Scripts.audioManager.PlaySFX3D("Weapons/TankShot", apacheData.rocketLauncher, "FireGun");
		ProjectileManager.AddProjectile(apacheData.projectile2, muzzleFlash.transform.position, apacheData.gun.transform.forward, new HitData(), new HitData().gameObject, this.gameObject);
		muzzleFlash.EmitInChildren (20);
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
		ray = new Ray(transform.position, tempDirection);		
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