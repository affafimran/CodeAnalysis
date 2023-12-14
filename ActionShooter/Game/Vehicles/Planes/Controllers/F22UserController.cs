using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class F22UserController : VehicleInputController
{
	private F22 f22;
	private F22Data f22Data;
	
	private bool boost;
	private bool brake;
	public float speed = 600;
	public float multiply = 1;
	
	private float horAxisSmooth;
	private float verAxisSmooth;
	
	// Firing vars
	public float rof2 = 2.0f;
	public float currentRof2;
	
	private int currentLauncher = 1;
	
	
	private float maxSpeed = 65f;
	
	
	private float rotX, rotY = 0f;
	
	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		f22 = gameObject.GetComponent<F22>(); // main tank component
		f22Data = f22.f22Data; // get the tankData for east access
		
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0); // Reset the rotation
		f22Data.rigidBody.useGravity = false;
		f22Data.rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		
		f22.ActivateRearThrust ();
		
		//activate sound
		f22Data.flyingSound.enabled = true;

		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Plane);
	}
	
	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		
		// store deltaTime for easy access
		deltaTime = Time.deltaTime;
		
		// calc rof
		f22Data.currentRof -= deltaTime;
		currentRof2 -= deltaTime;
		
		f22Data.flyingSound.pitch = multiply;

		UserInput ();

		// Firing
		if (primaryFire) 
		{
			if (f22Data.currentRof < 0)
			{
				f22Data.currentRof = f22Data.rof;
				GameObject tempMuzzleFlash = GetCurrentLauncher();
				Fire1(tempMuzzleFlash);
			}
		} else f22Data.currentRof = 0;
		
		if (secondaryFire) 
		{
			if (currentRof2 < 0)
			{
				currentRof2 = rof2;
				Fire2();
			}
		} else currentRof2 = 0;
	}



	
	void UserInput()
	{
		// Save the Input
		horAxis = CrossPlatformInputManager.GetAxis ("Horizontal");
		verAxis = CrossPlatformInputManager.GetAxis ("Vertical"); 

		horAxisAsBool = CrossPlatformInputManager.GetButton ("Horizontal") || (CrossPlatformInputManager.GetAxis ("Horizontal") != 0);
		verAxisAsBool = CrossPlatformInputManager.GetButton ("Vertical") || (CrossPlatformInputManager.GetAxis ("Vertical") != 0);
		
		primaryFire = CrossPlatformInputManager.GetButton("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0);
		secondaryFire = CrossPlatformInputManager.GetButton("Fire2") || (CrossPlatformInputManager.GetAxis("Fire2 Joystick Trigger") > 0);
		
		boost = CrossPlatformInputManager.GetButton("Jump");
		brake = CrossPlatformInputManager.GetButton("Crouch");
		
		use = CrossPlatformInputManager.GetButtonDown ("Use");
		
		rotX *= 0.25f; //0.96f;
		rotY *= 0.25f; //0.96f;
		
		if (verAxisAsBool) rotY = (verAxis<0) ? -1.0f :  1.0f;   //-1f*GameData.mouseSensitivity :  1f*GameData.mouseSensitivity ;
		if (horAxisAsBool) rotX = (horAxis<0) ?  1.5f : -1.5f; //1.5f*GameData.mouseSensitivity : -1.5f*GameData.mouseSensitivity ;
		
		Quaternion rotation = f22Data.rigidBody.rotation;
		rotation *= Quaternion.AngleAxis(rotY, Vector3.right);
		rotation *= Quaternion.AngleAxis(rotX, Vector3.forward);
		
		// level fighter
		//if (!horAxisAsBool) rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(gameObject.transform.forward, Vector3.up), 0.05f);
		f22Data.rigidBody.MoveRotation(rotation);
		
		//  adjusted maxSpeed
		float correctedMaxSpeed = maxSpeed;
		
		if (boost) correctedMaxSpeed += 35f;
		else if (brake) correctedMaxSpeed -= 35f;
		
		// Z: forward/backward

		Vector3 velocity = gameObject.transform.InverseTransformDirection(f22Data.rigidBody.velocity);
		float speedDifference = correctedMaxSpeed - velocity.z; // forward speed difference
		Vector3 force = Vector3.forward * (f22Data.rigidBody.mass * speedDifference * 2f); //* (f22Data.rigidBody.mass * speedDifference * 2f);
		f22Data.rigidBody.AddRelativeForce(force);
		
		// left/right
		Vector3 drag    = Vector3.left * (velocity.x * f22Data.rigidBody.mass* 5);
		f22Data.rigidBody.AddRelativeForce(drag);
		
		// up/down
		drag    = Vector3.down * (velocity.y * f22Data.rigidBody.mass * 5);
		f22Data.rigidBody.AddRelativeForce(drag);
		
		if (use)
		{
			f22Data.rigidBody.drag = 0.5f;
			f22Data.rigidBody.angularDrag = 0.5f;
			f22Data.rigidBody.constraints = RigidbodyConstraints.None;
			f22Data.rigidBody.useGravity = true;
			
			f22Data.flyingSound.enabled = false;
			f22.DeactivateRearThrust ();
			f22.ExitVehicle();
		}
	}
	
	public void Fire1(GameObject aMuzzleFlash)
	{
		GameObject camera = CameraManager.activeCamera;
		Vector3 cameraTargetForward = camera.transform.forward + new Vector3 (0, 0.05f, 0); // missile will go to the forward of the f22 if were not shooting at a unit, traffic or the hammer
		// Play first weapon sound
		Scripts.audioManager.PlaySFX3D(f22Data.sound, f22Data.barrel, "FireGun");
		// fire missiles
		ProjectileManager.AddProjectile(f22Data.projectile, aMuzzleFlash.transform.position, cameraTargetForward, new HitData(), GetMissileTarget(), this.gameObject);
	}
	
	public void Fire2()
	{
		// Play weapon sound
		Scripts.audioManager.PlaySFX3D(f22Data.sound2, f22Data.barrel, "FireGun");
		// Bombs away!
		ProjectileManager.AddProjectile(f22Data.projectile2, f22Data.turret.transform.position, f22Data.turret.transform.forward, new HitData(), GetBombTarget(), this.gameObject);
	}
	
	GameObject GetCurrentLauncher()
	{
		GameObject tempMuzzleFlash = null;
		if (currentLauncher == 1) tempMuzzleFlash = f22Data.muzzleFlash1; 
		if (currentLauncher == 2) tempMuzzleFlash = f22Data.muzzleFlash2; 
		if (currentLauncher == 3) tempMuzzleFlash = f22Data.muzzleFlash3; 
		if (currentLauncher == 4) tempMuzzleFlash = f22Data.muzzleFlash4; 
		
		currentLauncher++; 
		if (currentLauncher > 4) currentLauncher = 1;
		
		return tempMuzzleFlash;
	}
	
	// Get the bomb target
	GameObject GetBombTarget()
	{
		Ray ray = new Ray (transform.position, Vector3.down);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
		{
			return rayCastHit.collider.gameObject;
		}
		return null;
	}
	
	GameObject GetMissileTarget()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
		{
			if (rayCastHit.collider.gameObject != gameObject) return rayCastHit.collider.gameObject;
		}
		return null;
	}

	public override void Destroy()
	{
		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Normal);
		// Destroy this instance
		Destroy(this);
	}

}

