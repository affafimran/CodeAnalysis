using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class CharacterUserInput : CharacterInputController
{
	private bool dpadLeft = false;
	private bool dpadRight = false;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 45f;
	private float lerpFactor = 0.7f;

	protected override void InitializeSpecific(){
		continuesX = !GameData.mobile;
	}

	void Update()
	{
		if (Data.pause) return;
		horAxis = CrossPlatformInputManager.GetAxis("Horizontal"); 
		verAxis = CrossPlatformInputManager.GetAxis("Vertical"); 

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = gameObject.transform.eulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);gameObject.transform.eulerAngles = euler;}
		} else gameObject.transform.Rotate(0, x * (10f*GameData.mouseSensitivity), 0);

		// fire & power
		primaryFire = CrossPlatformInputManager.GetButton("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0);
		secondaryFire = CrossPlatformInputManager.GetButton("Fire2") || (CrossPlatformInputManager.GetAxis("Fire2 Joystick Trigger") > 0);
		//powerOn = CrossPlatformInputManager.GetKey(KeyCode.Z); // TEMP!!

		// special code for d-pad
		bool moveRight = false;
		if (CrossPlatformInputManager.GetAxis("D-Pad Horizontal") > 0 && !dpadRight) 
		{
			moveRight = true;
			dpadRight = true;
		} else if (CrossPlatformInputManager.GetAxis("D-Pad Horizontal") == 0) dpadRight = false;
		
		bool moveLeft = false;
		if (CrossPlatformInputManager.GetAxis("D-Pad Horizontal") < 0 && !dpadLeft) 
		{
			moveLeft = true;
			dpadLeft = true;
		} else if (CrossPlatformInputManager.GetAxis("D-Pad Horizontal") == 0) dpadLeft = false;

		nextPrimary = (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0) || moveLeft || CrossPlatformInputManager.GetButtonDown("NextPrimaryWeapon");
		previousPrimary = (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0) || moveRight || CrossPlatformInputManager.GetButtonDown("PreviousPrimaryWeapon");


		// Vehicle Generator
		if (ShopItemManager.IsBought("ShopItem5"))
		{
			buildVehicle = false;
			selectedVehicle = -1;
			if (Input.GetKeyUp(KeyCode.Alpha1)) selectedVehicle = 1;
			if (Input.GetKeyUp(KeyCode.Alpha2)) selectedVehicle = 2;
			if (Input.GetKeyUp(KeyCode.Alpha3)) selectedVehicle = 3;
			if (Input.GetKeyUp(KeyCode.Alpha4)) selectedVehicle = 4;
			if (Input.GetKeyUp(KeyCode.Alpha5)) selectedVehicle = 5;
			if (Input.GetKeyUp(KeyCode.Alpha6)) selectedVehicle = 6;
			if (Input.GetKeyUp(KeyCode.Alpha7)) selectedVehicle = 7;
			if (Input.GetKeyUp(KeyCode.Alpha8)) selectedVehicle = 8;
			if (Input.GetKeyUp(KeyCode.Alpha9)) selectedVehicle = 9;
			if (Input.GetKeyUp(KeyCode.Alpha0)) selectedVehicle = 0;

			if (selectedVehicle != -1)
			{
				buildVehicle = true;
				Scripts.audioManager.PlaySFX("Interface/Equip");
			}
		}

		// use
		use = CrossPlatformInputManager.GetButtonDown("Use");


		// misc.
//		dive = Input.GetButtonDown("Crouch") && verAxis > 0.9f;
		jump = CrossPlatformInputManager.GetButtonDown("Jump");

		// bullet time
		bulletTime = CrossPlatformInputManager.GetButton("HammerTime") || CrossPlatformInputManager.GetButton("LeftAnalogButton"); //;

	}

	public override void Reset()
	{
		horAxis = 0;
		verAxis = 0;

		primaryFire   = false;
		secondaryFire = false;
		powerOn = false;

		use = false;

		dive = false;
		jump = false;
		bulletTime = false;

		nextPrimary = false;
		previousPrimary = false;
	}

}

