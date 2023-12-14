using UnityEngine;
using System.Collections;

public class MammothTankAIController : VehicleInputController
{
	// we need this
	private MammothTank mammothTank;
	private MammothTankData mammothTankData; // this one is private since we can already view/edit them on the main pref
	
	// targetdata
	public TargetData targetData;
	
	// states
	public enum State {INACTIVE, IDLE, AIM, FIRE1, FIRE2, COOLDOWN} // these are the AI states
	public State state = State.IDLE; // current state
	
	// drive related vars
	public float driveTimer = 5;
	public int driveDirection = 1;
	
	// weaponside, cooldownTimer, cooldown, deltatime
	public int currentWeaponSide = -1; // 1 = left side, -1 = right side
	public float cooldownTimer = 5;
	public int cooldown = 6; // this is the amount of shots fired before going to cooldown
	private float deltaTime;
	
	// Grenade launcher vars

	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		mammothTank = gameObject.GetComponent<MammothTank>(); // main tank component
		mammothTankData = mammothTank.mammothTankData; // get the tankData for east access
	
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
		mammothTankData.currentRof -= deltaTime;
		mammothTankData.currentRof2 -= deltaTime;
		
		// Keep moving constantly
		//		driveTimer -= deltaTime;
		//		mammothTankData.rigidBody.AddForce((transform.forward * 30 * driveDirection) * (mammothTankData.rigidBody.mass * 15));
		//		if (driveTimer <= 0)
		//		{
		//			// return move direction
		//			driveTimer = 10;
		//			driveDirection = -driveDirection;
		//		}
		
		switch(state)
		{
		case State.INACTIVE:
			break;
			// cooling down
		case State.COOLDOWN: // cooling down
			cooldownTimer -= deltaTime;
			// Rotate the turret back
			Quaternion tempRotation = mammothTankData.turret.transform.rotation;
			mammothTankData.turret.transform.rotation = Quaternion.Slerp(tempRotation, Quaternion.identity, mammothTankData.aimSpeed * deltaTime);
			// End aiming sound
			if (Quaternion.Angle(tempRotation, mammothTankData.turret.transform.rotation) < 1) mammothTankData.aimingSound.enabled = false;
			// Cooldown done?
			if (cooldownTimer <= 0)
			{
				cooldown = 6;
				cooldownTimer = 5;
				state = State.IDLE;
			}
			break;
			// Idle
		case State.IDLE:
			// go to aim state
			if (targetData.target && targetData.distance <= mammothTankData.idleToAimRange) state = State.AIM;
			break;
			// Aim at target
		case State.AIM:
			if (targetData.distance < 15) { state = State.IDLE; return; }
			Aim(); // well, aim it!
			if (targetData.target && targetData.distance > mammothTankData.idleToAimRange) state = State.IDLE; // return to idle
			break;
			// Fire at target
		case State.FIRE1:
			currentWeaponSide = -currentWeaponSide; // Switch weapon side
			mammothTankData.currentRof = mammothTankData.rof; // Reset rate of fire
			// Shoot right barrel
			if (currentWeaponSide == -1)
			{
				Fire1(mammothTankData.muzzleFlash1);
			}
			else // Shoot left barrel
			{
				Fire1(mammothTankData.muzzleFlash2);
				// Count down untill next weapon fire
				cooldown--;
			}
			state = State.AIM;
			break;
		case State.FIRE2:
			currentWeaponSide = -currentWeaponSide; // Switch weapon side
			mammothTankData.currentRof2 = mammothTankData.rof2; // Reset rate of fire
			// Shoot right grenadeLauncher
			if (currentWeaponSide == -1)
			{
				Fire2(mammothTankData.muzzleFlash3, targetData.gameObject);
			}
			else // Shoot left grenadeLauncher
			{
				Fire2(mammothTankData.muzzleFlash4, targetData.gameObject);
				// Count down untill cooldown
				cooldown--;
				// Cooldown yet?
				if (cooldown <= 0)
				{
					// Start aimingSound
					mammothTankData.aimingSound.enabled = true;
					state = State.COOLDOWN;
					return;
				}
			}
			// Return to aiming
			state = State.AIM;
			break;
		}
	}
	
	void Aim()
	{
		// calculate direction and set a LookRotation
		Vector3 lookDirection =  targetData.position; //  new Vector3(targetData.position.x, tankData.turret.transform.position.y, targetData.position.z) - tankData.turret.transform.position;
		lookDirection.y = mammothTankData.turret.transform.position.y;
		lookDirection = (lookDirection-mammothTankData.turret.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
		Quaternion lookRotationTurret = lookRotation;
		// slerp the turret
		mammothTankData.turret.transform.rotation = Quaternion.Slerp(mammothTankData.turret.transform.rotation, lookRotation,  mammothTankData.aimSpeed * deltaTime);
		// reuse lookDirection for barrel
		lookDirection = (targetData.position - mammothTankData.barrel.transform.position).normalized;
		lookDirection.y = Mathf.Max(-0.3f, Mathf.Min(0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(mammothTankData.turret.transform.forward.x, lookDirection.y, mammothTankData.turret.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		// Lerp towards the end rotation from current rotation
		mammothTankData.barrel.transform.rotation = Quaternion.Slerp(mammothTankData.barrel.transform.rotation, lookRotation, mammothTankData.aimSpeed * deltaTime);	
		// reuse lookDirection for launcher
		lookDirection = (targetData.position - mammothTankData.launcher.transform.position).normalized;
		lookDirection.y = Mathf.Max(-0.3f, Mathf.Min(0.7f, lookDirection.y)); // limit the lookDirection up/down
		lookDirection = new Vector3(mammothTankData.turret.transform.forward.x, lookDirection.y, mammothTankData.turret.transform.forward.z); // reuse again
		lookRotation = Quaternion.LookRotation(lookDirection);
		lookRotation.eulerAngles = lookRotation.eulerAngles + new Vector3 (-60, 0, 0);
		// Lerp towards the end rotation from current rotation
		mammothTankData.launcher.transform.rotation = Quaternion.Slerp(mammothTankData.launcher.transform.rotation, lookRotation, mammothTankData.aimSpeed * deltaTime);
		// Is the target in sight
		if (Quaternion.Angle(lookRotationTurret, mammothTankData.turret.transform.rotation) < mammothTankData.aimToFireAngle)
		{
			mammothTankData.aimingSound.enabled = false; // disable aim sound
			if (cooldown > 3)
			{
				if (mammothTankData.currentRof < 0) state = State.FIRE1; // Rate of Fire lower then 0
			}
			else if (cooldown > 0)
			{
				if (mammothTankData.currentRof2 < 0) state = State.FIRE2;
			}
		}
		else mammothTankData.aimingSound.enabled = true; // Start aimingSound
	}
	
	public void Fire1(GameObject muzzleFlash)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(mammothTankData.sound, mammothTankData.barrel, "FireGun");
		// Add projectile and activate the particleSystem
		ProjectileManager.AddProjectile (mammothTankData.projectile, muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), new HitData().gameObject);
		muzzleFlash.EmitInChildren(20);
	}
	
	public void Fire2(GameObject muzzleFlash, GameObject target)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(mammothTankData.sound2, mammothTankData.launcher, "FireGun");
		// Add projectile and activate the particleSystem
		ProjectileManager.AddProjectile ("GrenadeShell", muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), target, this.gameObject);
		muzzleFlash.EmitInChildren(20);
	}
	
	public override void Reset()
	{}
	
}


