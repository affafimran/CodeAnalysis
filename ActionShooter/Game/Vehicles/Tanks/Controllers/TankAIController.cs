using UnityEngine;
using System.Collections;

public class TankAIController : VehicleInputController
{
	// we need this
	private Tank tank;
	private TankData tankData; // this one is private since we can already view/edit them on the main prefab
	
	// targetdata
	public TargetData targetData;
	
	// states
	public enum State {INACTIVE, IDLE, AIM, FIRE, COOLDOWN} // these are all the AI states
	public State state = State.IDLE; // current state
	
	// drive related vars
	public float driveTimer = 5;
	public int driveDirection = 1;
	
	// cooldown, deltatime
	public int cooldown = 5; // this is the amount of shots fired before going to cooldown
	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		tank = gameObject.GetComponent<Tank>(); // main tank component
		tankData = tank.tankData; // get the tankData for east access
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
		tankData.currentRof -= deltaTime; 
		
		switch(state)
		{
		case State.INACTIVE: // Never know when you would need an inactive state....
			break;		
		case State.COOLDOWN: // cooling down/driving up and down
			// rotate turret back (how about barrel)
			Quaternion targetRotation = gameObject.transform.rotation;
			tankData.turret.transform.rotation = Quaternion.Slerp(tankData.turret.transform.rotation,targetRotation, tankData.aimSpeed * deltaTime);
			if (Quaternion.Angle(tankData.turret.transform.rotation, targetRotation) < tankData.aimToFireAngle) tankData.aimingSound.enabled = false;
			
			// @Frank DRIVING WILL BE UPDATED!
			
			// start driving
			driveTimer -= deltaTime;
			tankData.rigidBody.AddForce(transform.forward * 10 * driveDirection, ForceMode.Acceleration);
			
			// Play the constant SFX sound the unit will have
			tankData.drivingSound.enabled = true;
			// Reset the variables when done cooling down
			if (driveTimer <= 0)
			{
				tankData.drivingSound.enabled = false;
				driveTimer = 10;
				driveDirection = -driveDirection;
				cooldown = 5;
				state = State.IDLE;
			}
			break;
			// Idle
		case State.IDLE:
			// go to aim state
			if (targetData.target && targetData.distance <= tankData.idleToAimRange) state = State.AIM;
			break;
			// Aim at target
		case State.AIM:
			if (targetData.distance < 8) { state = State.IDLE; return; }
			Aim(); // well, aim it!
			if (targetData.target && targetData.distance > tankData.idleToAimRange) state = State.IDLE; // return to idle
			break;
			// Fire at target
		case State.FIRE:
			Fire (); // Fire projectile and start the particle systems
			tankData.currentRof = tankData.rof; // Reset the Rate of Fire
			// @Frank: Not to happy about the cooldown yet, lets make note..
			cooldown--; // Count down for the cooling down
			// Cooldown yet?
			if (cooldown <= 0) 
			{
				tankData.aimingSound.enabled = true; // Start aimingSound
				state = State.COOLDOWN; // Start the cooldown
			} else state = State.AIM;
			break;
		}
	}
	
	void Aim()
	{
		// calculate direction and set a LookRotation
		Vector3 lookDirection =  targetData.position; //  new Vector3(targetData.position.x, tankData.turret.transform.position.y, targetData.position.z) - tankData.turret.transform.position;
		lookDirection.y = tankData.turret.transform.position.y;
		lookDirection = (lookDirection-tankData.turret.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
		Quaternion lookRotationTurret = lookRotation;
		// slerp the turret
		tankData.turret.transform.rotation = Quaternion.Slerp(tankData.turret.transform.rotation, lookRotation,  tankData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = (targetData.position - tankData.barrel.transform.position).normalized;
		lookDirection.y = Mathf.Max(-0.3f, Mathf.Min(0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(tankData.turret.transform.forward.x, lookDirection.y, tankData.turret.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		tankData.barrel.transform.rotation = Quaternion.Slerp(tankData.barrel.transform.rotation, lookRotation, tankData.aimSpeed * deltaTime);	
		// Is the target in sight
		if (Quaternion.Angle(lookRotationTurret, tankData.turret.transform.rotation) < tankData.aimToFireAngle)
		{
			tankData.aimingSound.enabled = false; // disable aim sound
			if (tankData.currentRof < 0) state = State.FIRE; // Rate of Fire lower then 0
		}
		else tankData.aimingSound.enabled = true; // Start aimingSound
	}
	
	public void Fire() // Still not too sure, but I think it might be better to have a separate fire for AI and User. Maybe different things will happen
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(tankData.sound, tankData.barrel, "FireGun");
		ProjectileManager.AddProjectile (tankData.projectile, tankData.muzzleFlash1.transform.position, tankData.barrel.transform.forward, new HitData (), new HitData().gameObject, this.gameObject);
		tankData.muzzleFlash1.PlayInChildren();
	}
	
	public override void Reset()
	{}
	
}

