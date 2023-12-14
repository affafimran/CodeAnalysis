using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MammothTankData
{
	// MammothTank specifics
	public GameObject turret, barrel, launcher;
	public GameObject muzzleFlash1, muzzleFlash2, muzzleFlash3, muzzleFlash4;

	public Rigidbody rigidBody;
	public MeshCollider collider;
	
	public AudioSource aimingSound, drivingSound;

	public float idleToAimRange		= 0;
	public float aimSpeed			= 0;
	public float aimToFireAngle		= 0;
	
	public float rof				= 0;
	public float currentRof			= 0;
		
	public string projectile		= "";
	public string muzzleFlash       = "";
	public string sound             = "";

	public float rof2;
	public float currentRof2;

	public string projectile2		= "";
	public string muzzleFlashImage2 = "";
	public string sound2            = "";

}

[System.Serializable]
public class MammothTankCarData
{
	public float maxSpeed =  0.0f;
	public float maxSpeedInverse =  0.0f;
	
	public  float accelerationGain = 0.0f;
	public  float decelerationGain = 0.0f;
	public float turnGain = 0.0f;
	
	public  float drag = 0.0f;
	public  float grip = 0.0f;
	
	public  float strength = 80.0f;
	public  float damping  = 0.0f;
	public  float hoverOffset = 0.08f;
	public  float mass = 0.0f;
	
	public  float maxWheelTurn = 45.0f;
	
	public float currentSpeed = 0.0f;   // this is the current speed of the car
	public float currentSpeedPerc = 0.0f;  // percentage of maximum speed, can exceed 1.0 if nitro is used (or when going down hill)
	
	public string soundSet;
	public CarSound sound;	
}


public class MammothTank : Vehicle {
	
	public MammothTankData mammothTankData;
	public MammothTankCarData mammothTankCarData;

	public bool frozen = false;
	private bool hovering = false;    // if car has one wheel on ground its considered as hovering
	
	private List<GameObject> wheelList   = new List<GameObject>();          // Contains the wheels
	private List<float> wheelRadiusList  = new List<float>();
	private List<Vector3> wheelLocalList = new List<Vector3>();
	private List<float> wheelOffsetList  = new List<float>();            // Contains wheel offsets, set in FixedUpdate(), used in Update()
	private bool[] wheelOnGroundArray;  // each wheel is either on ground or not
	
	private GameObject mammothTank;
	
	private GameObject tankTrackLeft, tankTrackRight;
	private Material trackLeft, trackRight;
	private float trackSpeedMultiplier = 0.01f;
	private float trackSpeedMultiplier2 = 0.1f;
	
	private Vector3 bodyDownVector;
	private Vector3 bodyUpVector;
	
	private Vector3 centerCom, com;
	private float upsideDownTimer = 0.0f;  // if not 0 or below, no new spark particle system is spawned
	
	private Ray ray;
	private RaycastHit rayCastHit;
	private LayerMask layerMask = (1 << 0) | (1 << 13);// | (1 << 14);

	public override void InitializeSpecific()
	{
		// new MammothTankData
		mammothTankData = new MammothTankData();
		mammothTankData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["TankSettings"].d[vehicleData.typeSetting].d, mammothTankData) as MammothTankData;
		mammothTankData.currentRof = mammothTankData.rof;
		mammothTankData.currentRof2 = mammothTankData.rof2;
				
		// new mammothTankCarData
		mammothTankCarData = new MammothTankCarData();
		mammothTankCarData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["CarSettings"].d[vehicleData.typeSetting].d, mammothTankCarData) as MammothTankCarData;

		// Get turret, barrel, launcher and muzzleflash objects
		mammothTankData.turret = transform.Find("MammothTank/MammothTankTurret").gameObject;
		mammothTankData.barrel = transform.Find("MammothTank/MammothTankTurret/MammothTankBarrels").gameObject;
		mammothTankData.launcher = transform.Find("MammothTank/MammothTankTurret/MammothTankLaunchers").gameObject;
		GameObject muzzleFlash1Dummy = transform.Find("MammothTank/MammothTankTurret/MammothTankBarrels/MammothTankMuzzleFlash1").gameObject;
		GameObject muzzleFlash2Dummy = transform.Find("MammothTank/MammothTankTurret/MammothTankBarrels/MammothTankMuzzleFlash2").gameObject;
		GameObject muzzleFlash3Dummy = transform.Find("MammothTank/MammothTankTurret/MammothTankLaunchers/MammothTankMuzzleFlash3").gameObject;
		GameObject muzzleFlash4Dummy = transform.Find("MammothTank/MammothTankTurret/MammothTankLaunchers/MammothTankMuzzleFlash4").gameObject;
		
		mammothTankData.rigidBody = gameObject.AddComponent<Rigidbody> ();
		mammothTankData.rigidBody.mass = vehicleData.mass;
			
		mammothTankData.collider = vehicle.GetComponent<MeshCollider> ();
		//mammothTankData.collider.convex = true;

		// update bounds
		boundsObject = mammothTankData.turret;

		// Add the muzzleflash effect to the gameObject
		mammothTankData.muzzleFlash1 = Loader.LoadGameObject("Effects/"+mammothTankData.muzzleFlash+"_Prefab");
		mammothTankData.muzzleFlash1.transform.ResetToParent (muzzleFlash1Dummy);
		mammothTankData.muzzleFlash2 = Loader.LoadGameObject("Effects/"+mammothTankData.muzzleFlash+"_Prefab");
		mammothTankData.muzzleFlash2.transform.ResetToParent (muzzleFlash2Dummy);
		mammothTankData.muzzleFlash3 = Loader.LoadGameObject("Effects/"+mammothTankData.muzzleFlashImage2+"_Prefab");
		mammothTankData.muzzleFlash3.transform.ResetToParent (muzzleFlash3Dummy);
		mammothTankData.muzzleFlash4 = Loader.LoadGameObject("Effects/"+mammothTankData.muzzleFlashImage2+"_Prefab");
		mammothTankData.muzzleFlash4.transform.ResetToParent (muzzleFlash4Dummy);
		
		// sounds...
		mammothTankData.drivingSound = Scripts.audioManager.PlaySFX3D ("Vehicles/TankLoop", vehicle, "Aiming");
		mammothTankData.drivingSound.enabled = false;
		
		mammothTankData.aimingSound = Scripts.audioManager.PlaySFX3D("Weapons/Aim", mammothTankData.turret, "Aiming");
		mammothTankData.aimingSound.enabled = false;

		// tank car behavior stuff
		
		// Set the meshCollider to convex
		MeshCollider[] meshColliders = vehicle.GetComponentsInChildren<MeshCollider>();
		foreach(MeshCollider meshCollider in meshColliders)
		{
			//meshCollider.sharedMesh = meshCollider.gameObject.GetComponent<MeshFilter>().mesh; // THIS IS VERY IMPORTANT! WE MUST CHANGE THIS IN THE PREFAB?
			meshCollider.convex = true;
		}
		
		// car
		mammothTank = gameObject; // UGH! NOT DONE HERE
		
		tankTrackLeft  = GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"TrackLeft");
		tankTrackRight = GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"TrackRight");
		
		trackLeft  = Instantiate(Loader.LoadMaterial("Vehicles/MammothTankTrack_Material")) as Material;
		trackRight = Instantiate(Loader.LoadMaterial("Vehicles/MammothTankTrack_Material")) as Material;
		
		Material[] materials = tankTrackLeft.GetComponent<Renderer>().materials;
		materials[0] = trackLeft;
		tankTrackLeft.GetComponent<Renderer>().materials = materials;
		
		materials = tankTrackRight.GetComponent<Renderer>().materials;
		materials[0] = trackRight;
		tankTrackRight.GetComponent<Renderer>().materials = materials;
		
		
		// Get the Wheels, in a very specific order.
		// This will help us out later on! E.g. calculate the COM
		// get LeftFront & RightFront wheel; needed!
		wheelList.Add(GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"WheelLF"));
		wheelList.Add(GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"WheelRF"));
		
		// backwheels, which we count!
		GameObject child;
		for (int i = 1; i < 5; i++) { // Max 5 (=10 back) wheels??)
			child = GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"WheelLB"+i);
			if (child == null) break;
			else wheelList.Add(child);
			// if we had an LeftBack+i wheel we MUST have and RightBack as well
			wheelList.Add(GenericFunctionsScript.FindChildSimple(vehicle, vehicle.name+"WheelRB"+i));
		}
		
		// populate radius list, wheelLocalLists as well
		float radius;
		Vector3 localPosition;
		foreach(GameObject wheel in wheelList)
		{
			radius = wheel.GetComponent<Renderer>().bounds.extents.y;
			wheelRadiusList.Add(radius);
			// add local position
			localPosition = wheel.transform.localPosition;
			localPosition.y = radius; // THIS IS THE HACK FOR SLIDING. ONLY BECAUSE ARTIST CAN'T LIGN UP MODELS PROPERLY
			wheelLocalList.Add(localPosition);
			centerCom += localPosition; // add all localPositions
			
		}
		centerCom /= wheelList.Count; // divide with wheelCount to get 'fake' com
		
		// populate wheelOnGround array
		wheelOnGroundArray = new bool[wheelList.Count];
		for (int i = 0; i < wheelList.Count; ++i)
			wheelOnGroundArray[i] = false;
		
		// populate pWheelOffset list
		for (int i = 0; i < wheelList.Count; i++)
			wheelOffsetList.Add(0.0f);
		
		// adjust the Center Of Mass to prevent 'sliding'
		// This is not realistic, but it is perfect for our more arcady solution
		
		// store x & y, where x should be center (most of the time) and y is the height of the first (steering) wheel. The lower y the less toppling occurs
		// z needs to be calculate from the average position from the wheels most furthest apart.
		float x = 0.0f;
		float y = wheelRadiusList[0];
		float z = centerCom.z;
		com = new Vector3(x, y, z); // final com
		
		// set it
		mammothTankData.rigidBody.centerOfMass = com;
		
		// collider/material settings
		vehicle.GetComponent<Collider>().material.dynamicFriction = 0.5f;
		vehicle.GetComponent<Collider>().material.staticFriction = 0.1f;
		vehicle.GetComponent<Collider>().material.bounciness = 0.05f;
		
		// update carData, where applicable OR when nog through sharedData.txt
		mammothTankCarData.hoverOffset = wheelRadiusList[0] + mammothTankCarData.hoverOffset;
		mammothTankCarData.maxSpeedInverse = -mammothTankCarData.maxSpeed*0.5f; // set it how you like it
	}
	
	protected override void UpdateSpecific()
	{
		if (Data.pause || frozen) return;  // don't update when paused

		// store data
		mammothTankCarData.currentSpeed = Vector3.Dot(mammothTankData.rigidBody.velocity, mammothTank.transform.forward);
		mammothTankCarData.currentSpeedPerc = mammothTankCarData.currentSpeed / mammothTankCarData.maxSpeed;
		
		Vector3 angVel = mammothTankData.rigidBody.angularVelocity;
		
		trackLeft.mainTextureOffset  += new Vector2(0, -mammothTankCarData.currentSpeed * trackSpeedMultiplier);
		trackRight.mainTextureOffset += new Vector2(0, -mammothTankCarData.currentSpeed * trackSpeedMultiplier);
		
		if (controller.horAxis != 0f)
		{
			if (controller.horAxis <= 0f){
				trackLeft.mainTextureOffset  += new Vector2(0, -angVel.y * trackSpeedMultiplier2);
				trackRight.mainTextureOffset += new Vector2(0,  angVel.y * trackSpeedMultiplier2);
			} else {
				trackLeft.mainTextureOffset  += new Vector2(0, -angVel.y * trackSpeedMultiplier2);
				trackRight.mainTextureOffset += new Vector2(0,  angVel.y * trackSpeedMultiplier2);
			}
		}
	}
	
	protected override void FixedUpdateSpecific()
	{
		float tVehicleSpeed = Vector3.Dot(mammothTankData.rigidBody.velocity, mammothTank.transform.forward); // we get this again since this is fixedUpdate
		Vector3 tAngVelocity = mammothTankData.rigidBody.angularVelocity;
		
		Vector3 tWheelPos;
		Vector3 tPointVelocity;
		float tForceFloat;
		Vector3 tForceVec;
		
		float tPowerCoeff = 0.0f;  // increased at each wheel that's on the ground
		float tPowerCoeffOffset = 1f/wheelList.Count;
		
		bodyUpVector = mammothTank.transform.up;
		bodyDownVector = bodyUpVector * -1;
		hovering = false;
		
		// cast rays for each wheel
		// this is the code that actually 'floats' the car!
		for(int i = 0; i < wheelList.Count; ++i)
		{
			int currentWheel = i;
			tWheelPos = gameObject.transform.TransformPoint(wheelLocalList[currentWheel]); // VERY important! This is NOT the actual wheel position, but where it was on startup (in the actual model)!
			ray = new Ray(tWheelPos, bodyDownVector);
			//Debug.DrawLine(ray.origin, ray.origin + (ray.direction*tankCarData.hoverOffset));
			
			if (Physics.Raycast(ray, out rayCastHit, mammothTankCarData.hoverOffset, layerMask))
			{  
				wheelOffsetList[currentWheel] = rayCastHit.distance; 
				tForceFloat = mammothTankCarData.strength * (mammothTankCarData.hoverOffset - wheelOffsetList[currentWheel]);
				tPointVelocity = GetComponent<Rigidbody>().GetPointVelocity(tWheelPos); // calculate velocity of wheel (for damping)
				tForceFloat = tForceFloat + (mammothTankCarData.damping * Vector3.Dot(tPointVelocity, bodyDownVector)); // compute damping
				tForceVec = bodyUpVector * (tForceFloat * mammothTankCarData.mass);
				mammothTankData.rigidBody.AddForceAtPosition(tForceVec, tWheelPos);
				
				hovering = true;
				tPowerCoeff += tPowerCoeffOffset;
				
				if ((rayCastHit.rigidbody == null) && (rayCastHit.collider.gameObject.layer != 1)) // needs attention
				{  
					wheelOnGroundArray[currentWheel] = true;
					wheelList[i].transform.localPosition = wheelLocalList[i] + (wheelOffsetList[i] - wheelRadiusList[i]) * bodyDownVector; // update wheel location
				}
				
			} else
			{ 
				wheelOffsetList[currentWheel] = 0.0f;
				wheelOnGroundArray[currentWheel] = false;
				wheelList[i].transform.localPosition = wheelLocalList[i] + (mammothTankCarData.hoverOffset - wheelRadiusList[i]) * bodyDownVector; // update wheel location
			}
		}
		
		if (hovering)
		{
			// we store these vars seperately
			float tAdjMaxSpeed = mammothTankCarData.maxSpeed;
			float tAdjMaxSpeedInverse = mammothTankCarData.maxSpeedInverse;
			
			float tDiff;
			Vector3 tImpulse;
			
			// go forward
			if (controller.verAxis > 0.01f)
			{  
				tDiff = Mathf.Min(tAdjMaxSpeed - tVehicleSpeed, 30.0f);  // maxed at 30 so that we don't apply a huge force when we're just driving away
				tImpulse = new Vector3(0,0,1) * (controller.verAxis * tPowerCoeff * mammothTankCarData.mass * tDiff * mammothTankCarData.accelerationGain);  // note the parentheses: compute all floats and only then multiply with vector
				mammothTankData.rigidBody.AddRelativeForce(tImpulse);
			}
			
			// go backward/brake
			if (controller.verAxis < -0.01f)
			{
				tDiff = (tAdjMaxSpeedInverse) - tVehicleSpeed;  // in general a negative number			
				tImpulse = new Vector3(0,0,-1) * (controller.verAxis * tPowerCoeff * mammothTankCarData.mass * tDiff * mammothTankCarData.decelerationGain);  // note the parentheses: compute all floats and only then multiply with vector
				mammothTankData.rigidBody.AddRelativeForce(tImpulse);
			}
			
			// Traditional turning
			if (controller.horAxis < -0.01f)
			{
				tImpulse = new Vector3(0,-1,0) * (controller.horAxis * -mammothTankCarData.turnGain * mammothTankCarData.mass * tPowerCoeff);
				if (tVehicleSpeed < 0.0f) tImpulse = -tImpulse;  // invert steering when driving backward
				mammothTankData.rigidBody.AddRelativeTorque(tImpulse);
			}
			
			if (controller.horAxis > 0.01f)
			{
				tImpulse = new Vector3(0,1,0) * (controller.horAxis * mammothTankCarData.turnGain * mammothTankCarData.mass * tPowerCoeff);
				if (tVehicleSpeed < 0.0f) tImpulse = -tImpulse;  // invert steering when driving backward
				mammothTankData.rigidBody.AddRelativeTorque(tImpulse);
			}	
			
			// this is a small & weird, but important 'hack'. This'll make sure the car won't turn left/right uncontrolable.
			// The car will 'straighten' itself...
			float tAngVelLength = tAngVelocity.magnitude;
			if (tAngVelLength > 0.0f)
			{
				float tPerc = 1.0f;
				float tVehicleSpeedAbs = Mathf.Abs(tVehicleSpeed);
				float tCompensateGain = 0.25f + (mammothTankCarData.turnGain * tPerc);
				if (tCompensateGain > mammothTankCarData.turnGain) tCompensateGain = mammothTankCarData.turnGain;
				tImpulse = Vector3.up * (Vector3.Dot(tAngVelocity, bodyUpVector) * (-tCompensateGain * mammothTankCarData.mass * tPowerCoeff));
				mammothTankData.rigidBody.AddRelativeTorque(tImpulse);
				
				// WHAT THE FUCK IS THIS!
				if (tVehicleSpeedAbs < 20.0f) mammothTankData.rigidBody.angularVelocity *= 1.0f - (0.1f - tVehicleSpeedAbs*0.005f);
				if (tVehicleSpeedAbs <  5.0f) mammothTankData.rigidBody.angularVelocity *= 1.0f - (0.2f - tVehicleSpeedAbs*0.04f);
			}			
			
			// Work out how fast we're sliding left/right and compensate according to the grip property
			// We just allow the car to slide if it's slipping (with slip key)
			float tSlideSpeed = Vector3.Dot(mammothTank.transform.right, mammothTankData.rigidBody.velocity);
			tImpulse = new Vector3(1,0,0) * (-tSlideSpeed * mammothTankCarData.mass * mammothTankCarData.grip * tPowerCoeff);
			// Apply an impulse to compensate for sliding
			mammothTankData.rigidBody.AddRelativeForce(tImpulse);
			
			// Apply Drag proportional to speed, but not when we press forward key so we can actually get to our max speed (drag prevents us from getting the max speed otherwise)
			if (controller.verAxis == 0f)
			{
				tImpulse = new Vector3(0,0,-1) * (tVehicleSpeed * mammothTankCarData.drag * mammothTankCarData.mass);
				mammothTankData.rigidBody.AddRelativeForce(tImpulse);
			}
			upsideDownTimer = 0.0f;
		} else
		{  // we're not hovering, check for upside down
			if (tVehicleSpeed < 2.0f)
			{  // we're not moving fast
				if (Vector3.Dot(bodyUpVector, Vector3.up) < 0.3f)
				{  // we're sort of upside-down
					upsideDownTimer += Time.fixedDeltaTime;
					if (upsideDownTimer > 2.0f)
					{
						ResetTank();
						upsideDownTimer = 0.0f;
					}
				} else
				{
					upsideDownTimer = 0.0f;
				}
			}
		}
	}
	
	
	public void ResetTank()
	{		
		Debug.Log ("Not working yet!");
		mammothTankData.rigidBody.velocity = new Vector3(0,0,0);
		mammothTankData.rigidBody.angularVelocity = new Vector3(0,0,0);
	}

}