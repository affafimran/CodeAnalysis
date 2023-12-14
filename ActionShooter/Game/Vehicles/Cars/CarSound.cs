using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class CarSound : MonoBehaviour
{
	[HideInInspector]
	public CarData carData;
	
	// input info
	public VehicleInputController controller;
	public bool drift;
	
	public AudioSource driftSound;
	public AudioSource idleSound;
	public AudioSource drivingSound;
	public float currentVolume = 1.0f;
	public float currentGear = 1;
	public float gearSwitch;
	public float rpm;
	
	public void Initialize(CarData aCarData, VehicleInputController aController)
	{
		carData = aCarData;
		controller = aController;
		
		// load all sound effects
		string carSound = Data.Shared ["CarSoundSettings"].d [carData.soundSet].d ["sound"].s;
		idleSound    = Scripts.audioManager.PlaySFX3D ("Vehicles/"+carSound+"Stationair", gameObject, "CarAudio");
		drivingSound = Scripts.audioManager.PlaySFX3D ("Vehicles/"+carSound+"Driving", gameObject, "CarAudio");
		driftSound   = Scripts.audioManager.PlaySFX3D ("Vehicles/Brake", gameObject, "CarAudio");

	}
	
	void Update () 
	{
		// update sound effects (Gear switch and what not, check rally point 4).
		if(Data.pause) return;

		drift = controller.drift;
		
		gearSwitch = currentGear / 5;
		ShiftGear();
		// calculate rpm
		rpm = (Mathf.Abs(carData.currentSpeedPerc / gearSwitch)) * 1.2f;
		
		if(CrossPlatformInputManager.GetAxis("Vertical") > 0.0f)currentVolume = Mathf.Lerp( 1.0f,currentVolume,0.01f);
		else currentVolume = Mathf.Lerp( 0.5f,currentVolume,0.01f);
		
		// set the audio pitch to the percentage of RPM to the maximum RPM plus one, this makes the sound play
		// up to twice it's pitch, where it will suddenly drop when it switches gears.
		drivingSound.pitch = (rpm) * 1.6f ;
		// this line is just to ensure that the pitch does not reach a value higher than is desired.
		if ( drivingSound.pitch > 5.0f ) {
			drivingSound.pitch = 5.0f;
		}
		
		float speed = Mathf.Abs(carData.currentSpeed) * 4;
		
		drivingSound.volume = (currentVolume *  Mathf.Min (1.0f, speed / 10.0f ))* 0.7f;
		
		//			transmissionSound.volume =( 0.7f *  Mathf.Min (1.0f, speed / 10.0f )) * 0.6f ;
		//			transmissionSound.pitch = (Mathf.Min (1.0f, speed / 600.0f ) * 2.0f) + (currentGear*0.01f) + 0.5f;
		
		idleSound.volume = (1.0f - Mathf.Min (1.0f, speed / 40.0f ));
		
		if (drift) driftSound.volume = (currentVolume *  Mathf.Min (1.0f, speed / 10.0f ))* 0.35f ;
		else driftSound.volume = 0.0f;

	}
	

	void ShiftGear()
	{
		if (carData.currentSpeedPerc > gearSwitch) // Shift up
		{
			if (CrossPlatformInputManager.GetAxis("Vertical") > 0.0f)
			{
				if (currentGear < 5) currentGear++;
			}
		}
		else if (carData.currentSpeedPerc < ((currentGear - 1) * 20) / 100) // Shift down
		{
			if (currentGear > 1) currentGear--;
		}
	}
	
	public void Destroy()
	{
		Destroy (idleSound);
		Destroy (drivingSound);
		Destroy (driftSound);
		Destroy(this);
	}
}
