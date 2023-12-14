using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class CarUserController : VehicleInputController {

	private Car car;
	private CarData carData;
	
	public override void Initialize()
	{
		// Store main component
		car = gameObject.GetComponent<Car>(); // main tank component
		carData = car.carData; // get the tankData for east access

		// Add sound component
		carData.sound = gameObject.AddComponent<CarSound>();
		carData.sound.Initialize(carData, car.controller);

		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Car);

	}

	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		// Check input
		UserInput ();
	}
	
	void UserInput()
	{
		// Save the Input
		horAxis = CrossPlatformInputManager.GetAxis ("Horizontal");
		verAxis = CrossPlatformInputManager.GetAxis ("Vertical");
		
		horAxisAsBool = CrossPlatformInputManager.GetButton("Horizontal");
		verAxisAsBool = CrossPlatformInputManager.GetButton("Vertical");
		
		// drift input
		drift = CrossPlatformInputManager.GetButton ("Jump");
		
		// user
		use = CrossPlatformInputManager.GetButtonDown("Use");
		
		// use
		if (use) car.ExitVehicle();
	}

	public override void Destroy ()
	{
		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Normal);
		carData.sound.Destroy();
		Destroy(this);
	}

}
