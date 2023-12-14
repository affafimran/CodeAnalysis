using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class HelicopterUserController : VehicleInputController
{
	private Helicopter helicopter;
	private HelicopterData helicopterData;
	
	// Movement Vars
	//public bool up = false;
	//public bool down = false;

	//private float maxSpeed = 50f;
	//private Vector3 accelerationMultiplier = new Vector3(1.4f, 1.4f, 1.4f);
	//private Vector3 dragMultiplier = new Vector3(0.7f, 0.75f, 0.7f);

	//private float maxPitchAngle = 25f;
	//private float maxRollAngle  = 25f; 

	private RaycastHit rayCastHit;
	private int currentBarrel = 0; 

	private float deltaTime;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 25f;
	private float lerpFactor = 0.7f;

	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		helicopter = gameObject.GetComponent<Helicopter>(); // main helicopter component
		helicopterData = helicopter.helicopterData; // get the helicopterData for easy access

		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0); // Reset the rotation

		helicopterData.rigidBody.useGravity = false;
		helicopterData.rigidBody.constraints = RigidbodyConstraints.FreezeRotation;

		//activate sound
		helicopterData.rotorSound.enabled = true;

		// [MOBILE] ContinousX or not?
		continuesX = !GameData.mobile;
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

		// UserInput
		UserInput ();

		// Firing
		if (primaryFire) 
		{
			helicopterData.currentRof -= deltaTime; 
			if (helicopterData.currentRof < 0)
			{
				helicopterData.currentRof = helicopterData.rof;
				currentBarrel++;
				if (currentBarrel % 2 == 0)	helicopter.Fire(helicopterData.muzzleFlash1);
				else helicopter.Fire(helicopterData.muzzleFlash2);
			}
		} else helicopterData.currentRof = 0;
	}
	
	void UserInput()
	{
		// Save the Input
		horAxis = CrossPlatformInputManager.GetAxis ("Horizontal");
		verAxis = CrossPlatformInputManager.GetAxis ("Vertical");
		up      = CrossPlatformInputManager.GetButton("Jump");
		down    = CrossPlatformInputManager.GetButton("Crouch");
		primaryFire = CrossPlatformInputManager.GetButton ("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0); 
		use = CrossPlatformInputManager.GetButtonDown ("Use");

		// Rotate the helicopter left, right
		// We're rotating the rigidbody using MoveRotation
		// This uses the interpolation setting as well. Preventing jittering with the camera!

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = gameObject.transform.localEulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);helicopterData.rigidBody.MoveRotation(Quaternion.Euler(euler));}
		} else {
			Vector3 rotation = gameObject.transform.localEulerAngles + new Vector3(0, x,  0);
			Quaternion euler = Quaternion.Euler(rotation);
			helicopterData.rigidBody.MoveRotation(euler);
		}

//		// Movement
//		Vector3 velocity = gameObject.transform.InverseTransformDirection(helicopterData.rigidBody.velocity);
//		float speedDifference;
//
//		Vector3 force;
//		Vector3 drag;
//
//		// X: left/right
//		if (horAxis > 0){
//			speedDifference = maxSpeed - velocity.x; // forward speed difference
//			force = Vector3.right * (horAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		} else if (horAxis < 0) {
//			speedDifference = -maxSpeed - velocity.x; // forward speed difference
//			force = Vector3.left * (horAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		}
//
//		// Y: up/down
//		if (up){
//			speedDifference = maxSpeed - velocity.y; // upward speed difference
//			force = Vector3.up * (helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		}
//
//		if (down) {
//			speedDifference = -maxSpeed - velocity.y; // upward speed difference
//			force = Vector3.up * (helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		}
//
//		// Z: forward/backward
//		if (verAxis > 0){
//			speedDifference = maxSpeed - velocity.z; // forward speed difference
//			force = Vector3.forward * (verAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		} else if (verAxis < 0) {
//			speedDifference = -maxSpeed - velocity.z; // forward speed difference
//			force = Vector3.back * (verAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
//			helicopterData.rigidBody.AddRelativeForce(force);
//		}
//
//		// Apply drag in all positions.
//		// You can also opt to get the new velocity, invert it and have one force to add
//		// But I would like to have different drag settings per axis.
//
//		// left/right
//		drag    = Vector3.left * (velocity.x * dragMultiplier.x * helicopterData.rigidBody.mass);
//		helicopterData.rigidBody.AddRelativeForce(drag);
//
//		// up/down
//		drag    = Vector3.down * (velocity.y * dragMultiplier.y * helicopterData.rigidBody.mass);
//		helicopterData.rigidBody.AddRelativeForce(drag);
//
//		// forward/backward
//		drag    = Vector3.back * (velocity.z * dragMultiplier.z * helicopterData.rigidBody.mass);
//		helicopterData.rigidBody.AddRelativeForce(drag);
//
//		// pitch and roll, based upon speed
//		float roll  = Mathf.Max (-1f, -velocity.x/maxSpeed);
//		float pitch = Mathf.Min(1f, velocity.z/maxSpeed);
//		Vector3 localEuler = helicopterData.body.transform.localEulerAngles;
//		helicopterData.body.transform.localEulerAngles = new Vector3(maxPitchAngle*pitch, localEuler.y, maxRollAngle*roll);
//
//		// sound update
//		float perc = velocity.magnitude/maxSpeed;
//		helicopterData.rotorSound.pitch = Mathf.Min(1f, 0.6f + perc);
//		helicopterData.rotorSpeed = Mathf.Min(1200, 700 + (1200*perc));

		// aiming
		GameObject camera = CameraManager.activeCamera;
		float distance  = (camera.transform.position - gameObject.transform.position).magnitude + 3f;
		Vector3 position = camera.transform.position + (camera.transform.forward * distance); // this works okay for now
		Ray ray = new Ray(position, camera.transform.forward);
		Vector3 aimTarget = Vector3.zero;
		
		// Raycast forward to crosshair
		if (Physics.Raycast(ray, out rayCastHit, 1000, ProjectileManager.projectileLayerMask)){
			aimTarget = rayCastHit.point;
		} else aimTarget = ray.origin + ray.direction * 1000;
		helicopter.Aim(aimTarget);

		// if we 'use' we exit the vehicle
		if (use)
		{
			helicopterData.rigidBody.constraints = RigidbodyConstraints.None;
			helicopterData.rigidBody.useGravity = true;
			helicopterData.rotorSpeed = 0;
			helicopterData.rotorSound.enabled = false;
			helicopter.ExitVehicle();
		}
	}
}