using UnityEngine;
using System.Collections;

public class TurretAIController : VehicleInputController {
	
	// we need this
	private Turret turret;
	private TurretData turretData; // this one is private since we can already view/edit them on the main prefab
	
	// targetdata
	public TargetData targetData;
	
	// states 
	public enum State {INACTIVE, IDLE, AIM, FIRE, COOLDOWN} // these are all the AI states
	public State state = State.IDLE; // current state
	
	// activebarrel, cooldown, rotation, deltatime;
	public int currentBarrel = -1; // 1 = left side, -1 = right side
	public int cooldown = 3; // this is the amount of shots fired before going to cooldown
	public float cooldownTimer = 1;
	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		turret = gameObject.GetComponent<Turret> (); // main turret component
		turretData = turret.turretData; // get the turretData for easy access
	}
	
	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		
		// get target, no target exit
		targetData = TargetManager.GetTargetData (gameObject);
		if (!targetData.target) return; // return when no target. However, we could comment this line. If we use the bool to check target we can still run other code without aiming.
		
		// store deltaTime for easy access
		deltaTime = Time.deltaTime;

		// calc rof
		turretData.currentRof -= deltaTime;
		
		switch(state)
		{
		case State.INACTIVE: // Never know when you would need an inactive state....
			break;
		case State.COOLDOWN: // cooling down
			cooldownTimer -= deltaTime;
			// Rotate the turret back
			Quaternion tempRotation = turretData.turret.transform.rotation;

			turretData.turret.transform.rotation = Quaternion.Slerp(tempRotation, Quaternion.identity, turretData.aimSpeed * deltaTime);
			// End aiming sound
			if (Quaternion.Angle(tempRotation, turretData.turret.transform.rotation) < 1) turretData.aimingSound.enabled = false;
			// done cooling down
			if (cooldownTimer <= 0)
			{
				cooldownTimer = 1;
				cooldown = 3;
				state = State.IDLE;
			}
			break;
		case State.IDLE: // Idle
			// go to aim state
			if (targetData.target && targetData.distance <= turretData.idleToAimRange) state = State.AIM;
			break;
		case State.AIM: // Aim at target
			// Aim the Turret
			Aim();
			if (targetData.target && targetData.distance > turretData.idleToAimRange) state = State.IDLE; // return to idle
			break;
		case State.FIRE: // Fire at target
			turretData.currentRof = turretData.rof; // Reset Rate of Fire
			currentBarrel = -currentBarrel; // Switch barrel side
			if (currentBarrel == 1) // Fire left barrel
			{
				Fire(turretData.muzzleFlash1, new HitData().gameObject);
			}
			else // Fire Right barrel
			{
				Fire(turretData.muzzleFlash2, new HitData().gameObject);
				// Count down the amount of shots allowed until cooldown
				cooldown--;
				// Cooldown time yet?
				if (cooldown <= 0) 
				{
					turretData.aimingSound.enabled = true; // Start aimingSound
					state = State.COOLDOWN; // Set cooldown
					return;
				}
			}
			state = State.AIM; // Set the turret back to aiming
			break;
		}
	}
	
	void Aim()
	{
		// calculate direction and set a LookRotation
		Vector3 lookDirection = targetData.position;
		lookDirection.y = turretData.turret.transform.position.y;
		lookDirection = (lookDirection - turretData.turret.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation (lookDirection);
		Quaternion lookRotationTurret = lookRotation;
		// slerp the turret
		turretData.turret.transform.rotation = Quaternion.Slerp (turretData.turret.transform.rotation, lookRotation, turretData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = ((targetData.position + new Vector3(0, 1, 0)) - turretData.barrel.transform.position).normalized;
		lookDirection.y = Mathf.Max (-0.3f, Mathf.Min (0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(turretData.turret.transform.forward.x, lookDirection.y, turretData.turret.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		turretData.barrel.transform.rotation = Quaternion.Slerp(turretData.barrel.transform.rotation, lookRotation, turretData.aimSpeed * deltaTime);	
		// Is the target in sight
		if (Quaternion.Angle(lookRotationTurret, turretData.turret.transform.rotation) < turretData.aimToFireAngle)
		{
			turretData.aimingSound.enabled = false; // disable aim sound
			if (turretData.currentRof < 0) state = State.FIRE; // Rate of Fire lower then 0
		}
		else turretData.aimingSound.enabled = true; // Start aimingSound
	}
	
	public void Fire(GameObject muzzleFlash, GameObject target)
	{
		// Play weapon sound 
		Scripts.audioManager.PlaySFX3D("Weapons/M4Fire", turretData.barrel, "FireGun");
		
		// Fire left barrel and activate the muzzleflash from left barrel
		ProjectileManager.AddProjectile(turretData.projectile, muzzleFlash.transform.position, turretData.barrel.transform.forward, new HitData(), target);
		muzzleFlash.PlayInChildren();
	}
	
	public override void Reset()
	{}
}