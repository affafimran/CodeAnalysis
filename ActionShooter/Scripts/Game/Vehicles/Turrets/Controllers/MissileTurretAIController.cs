using UnityEngine;
using System.Collections;

public class MissileTurretAIController : VehicleInputController {
	
	// we need this
	private MissileTurret missileTurret;
	private MissileTurretData missileTurretData; // this one is private since we can already view/edit them on the main prefab
	
	// targetdata
	public TargetData targetData;
	
	// states
	public enum State {INACTIVE, IDLE, AIM, FIRE, COOLDOWN} // these are all the AI states
	public State state = State.IDLE; // current state
	
	// activebarrel, cooldown, rotation, deltatime;
	public int currentBarrel = -1; // 1 = left side, -1 = right side
	public int cooldown = 4; // this is the amount of shots fired before going to cooldown
	public float cooldownTimer = 2;
	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		missileTurret = gameObject.GetComponent<MissileTurret> (); // main missileTurret component
		missileTurretData = missileTurret.missileTurretData; // get the turretData for easy access
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
		missileTurretData.currentRof -= deltaTime;
		
		switch(state)
		{
		case State.INACTIVE: // Never know when you would need an inactive state....
			break;
		case State.COOLDOWN: // cooling down
			cooldownTimer -= deltaTime;
			// Rotate the turret back
			Quaternion tempRotation = missileTurretData.turret.transform.rotation;
			missileTurretData.turret.transform.rotation = Quaternion.Slerp(tempRotation, Quaternion.identity, missileTurretData.aimSpeed * deltaTime);
			// End aiming sound
			if (Quaternion.Angle(tempRotation, missileTurretData.turret.transform.rotation) < 1) missileTurretData.aimingSound.enabled = false;
			// Cooling down done?
			if (cooldownTimer <= 0)
			{
				cooldownTimer = 2;
				cooldown = 4;
				state = State.IDLE;
			}
			break;
			// Idle
		case State.IDLE:
			// go to aim state
			if (targetData.target && targetData.distance <= missileTurretData.idleToAimRange) state = State.AIM;
			break;
			// Aim at target
		case State.AIM:
			// Rotate Turret
			Aim();
			if (targetData.target && targetData.distance > missileTurretData.idleToAimRange) state = State.IDLE; // return to idle
			break;
			// Fire at target
		case State.FIRE:
			missileTurretData.currentRof = missileTurretData.rof; // Reset Rate of Fire
			currentBarrel = -currentBarrel; // barrel switch side
			// Fire left barrel
			if (currentBarrel == 1)
			{
				Fire(missileTurretData.muzzleFlash1, targetData.gameObject);
			}
			else // Fire Right barrel
			{
				Fire(missileTurretData.muzzleFlash2, targetData.gameObject);
				// Count down the amount of shots allowed until cooldown
				cooldown--;
				// Cooldown time yet?
				if (cooldown <= 0) 
				{
					// Start aimingSound
					missileTurretData.aimingSound.enabled = true;
					// Set cooldown
					state = State.COOLDOWN;
					return;
				}
			}
			// Set the turret back to aiming
			state = State.AIM;
			break;
		}
	}
	
	void Aim()
	{
		// calculate direction and set a LookRotation
		Vector3 lookDirection = targetData.position;
		lookDirection.y = missileTurretData.turret.transform.position.y;
		lookDirection = (lookDirection - missileTurretData.turret.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation (lookDirection);
		Quaternion lookRotationTurret = lookRotation;
		// slerp the turret
		missileTurretData.turret.transform.rotation = Quaternion.Slerp (missileTurretData.turret.transform.rotation, lookRotation, missileTurretData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = (targetData.position - missileTurretData.barrel.transform.position).normalized;
		lookDirection.y = Mathf.Max (-0.3f, Mathf.Min (0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(missileTurretData.turret.transform.forward.x, lookDirection.y, missileTurretData.turret.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		missileTurretData.barrel.transform.rotation = Quaternion.Slerp(missileTurretData.barrel.transform.rotation, lookRotation, missileTurretData.aimSpeed * deltaTime);	
		// Is the target in sight
		if (Quaternion.Angle(lookRotationTurret, missileTurretData.turret.transform.rotation) < missileTurretData.aimToFireAngle)
		{
			missileTurretData.aimingSound.enabled = false; // disable aim sound
			if (missileTurretData.currentRof < 0) state = State.FIRE; // Rate of Fire lower then 0
		}
		else missileTurretData.aimingSound.enabled = true; // Start aimingSound
	}
	
	public void Fire(GameObject muzzleFlash, GameObject target)
	{
		// Play weapon sound 
		Scripts.audioManager.PlaySFX3D("Weapons/MissileTurretShot", missileTurretData.barrel, "FireGun");
		// Fire barrel and activate the muzzleflash from barrel
		ProjectileManager.AddProjectile(missileTurretData.projectile, muzzleFlash.transform.position, missileTurretData.barrel.transform.forward, new HitData(), target, this.gameObject);
		
	}
	
	public override void Reset()
	{}
}