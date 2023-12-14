using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CarData
{
	public float maxSpeed =  0.0f;
	public float maxSpeedInverse =  0.0f;
	
	public  float accelerationGain = 0.0f;
	public  float decelerationGain = 0.0f;
	public float turnGain = 0.0f;
	
	public  float drag = 0.0f;
	public  float angularDrag = 0.0f;
	public  float grip = 0.0f;
	public  float driftGrip = 0.0f;
	
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

public class Car : Vehicle {

	public CarData carData;

	public bool frozen = false;
	private bool hovering = false;    // if car has one wheel on ground its considered as hovering

	private List<GameObject> wheelList   = new List<GameObject>();          // Contains the wheels
	private List<float> wheelRadiusList  = new List<float>();
	private List<Vector3> wheelLocalList = new List<Vector3>();
	private List<float> wheelOffsetList  = new List<float>();            // Contains wheel offsets, set in FixedUpdate(), used in Update()
	private bool[] wheelOnGroundArray;  // each wheel is either on ground or not
	
	// skidmarks
	private GameObject[] pSkidMarkObject;
	private Skidmarks[] pSkidMarkScript;
	private int[] pSkidMarkLastIndex;
	private bool skidMarksActive = false;
	
	// drift smoke
	private GameObject[] pDriftSmokeObject;
	private bool driftSmokeActive = false;
	
	private GameObject car;
	
	private Vector3 bodyDownVector;
	private Vector3 bodyUpVector;
	
	private Vector3 centerCom, com;
	
	private bool vehicleSlip = false;           // Set to true when slip key is been pressed
	private float vehicleAdjGrip = 3.0f;
	private float vehicleWheelTurn = 0.0f;
	private float vehicleWheelSpin = 0.0f;
	
	private float upsideDownTimer = 0.0f;  // if not 0 or below, no new spark particle system is spawned
	
	private Ray ray;
	private RaycastHit rayCastHit;
	private LayerMask layerMask = (1 << 0) | (1 << 13) | (1 << 14);
	
	private float deltaTime;
	
	//private Vector3 currentSpeed;
	private Vector3 lastGroundedPosition;
	
	public override void InitializeSpecific()
	{
		// new TurretData
		carData = new CarData ();
		carData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["CarSettings"].d[vehicleData.typeSetting].d, carData) as CarData;
		carData.mass = vehicleData.mass; // store this. Easier to use below - pull everything from carData

		// Add Rigidbody (to gameobject!!!)
		Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
		rigidBody.mass = carData.mass;

		//rigidBody.constraints = RigidbodyConstraints.FreezeAll; // TEMP!
		gameObject.GetComponent<Rigidbody>().isKinematic = true;
		frozen = true;

		// Set the meshCollider to convex
		MeshCollider[] meshColliders = vehicle.GetComponentsInChildren<MeshCollider>();
		foreach(MeshCollider meshCollider in meshColliders)
		{
		
			// temp
			//meshCollider.sharedMesh = vehicle.GetComponent<MeshFilter>().sharedMesh;
			meshCollider.convex = true;
		}
		
		// car
		car = gameObject; // UGH! NOT DONE HERE
		
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
		
		// car rigidbody settings
		car.GetComponent<Rigidbody>().mass = vehicleData.mass;
		car.GetComponent<Rigidbody>().useGravity = true;
		car.GetComponent<Rigidbody>().angularDrag = carData.angularDrag;
		car.GetComponent<Rigidbody>().drag = 0.0f;
		car.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		
		// adjust the Center Of Mass to prevent 'sliding'
		// This is not realistic, but it is perfect for our more arcady solution
		
		// store x & y, where x should be center (most of the time) and y is the height of the first (steering) wheel. The lower y the less toppling occurs
		// z needs to be calculate from the average position from the wheels most furthest apart.
		float x = 0.0f;
		float y = wheelRadiusList[0];
		float z = centerCom.z;
		com = new Vector3(x, y, z); // final com
		
		// set it
		car.GetComponent<Rigidbody>().centerOfMass = com;
		
		// collider/material settings
		vehicle.GetComponent<Collider>().material.dynamicFriction = 0.5f;
		vehicle.GetComponent<Collider>().material.staticFriction = 0.1f;
		vehicle.GetComponent<Collider>().material.bounciness = 0.05f;
		
		// update carData, where applicable OR when nog through sharedData.txt
		carData.hoverOffset = wheelRadiusList[0] + carData.hoverOffset;
		carData.maxSpeedInverse = -carData.maxSpeed*0.5f; // set it how you like it
		
		// update vars
		vehicleAdjGrip = carData.grip;
	}
	
	protected override void UpdateSpecific()
	{
		if (Data.pause || frozen) return;  // don't update when paused
		deltaTime = Time.deltaTime; // store delta time
		
		// update the skidmarks
		if (skidMarksActive) UpdateSkidMarks ();
		
		// update the drift smoke objects
		if (driftSmokeActive) UpdateDriftSmoke ();
		
		// store data
		carData.currentSpeed = Vector3.Dot(car.GetComponent<Rigidbody>().velocity, car.transform.forward);
		carData.currentSpeedPerc = carData.currentSpeed / carData.maxSpeed;
		
		// calc wheel turn
		if (controller.horAxis >  0.1f) vehicleWheelTurn = Mathf.Min(1f,  vehicleWheelTurn + (deltaTime * 2f));
		if (controller.horAxis < -0.1f) vehicleWheelTurn = Mathf.Max(-1f, vehicleWheelTurn - (deltaTime * 2f)); 
		if (!controller.horAxisAsBool) vehicleWheelTurn *= 0.9f;
		
		// calc wheel spin.
		// PLEASE NOTE: This is 'visual guess'. If you want to have this correct (for different size wheels as well) think about implementing rotation/angles 
		// based upon distance travelled. I opted out for this as I don't want to calc it all.
		vehicleWheelSpin += 60*carData.currentSpeed * deltaTime;
		if (vehicleWheelSpin > 360.0f) vehicleWheelSpin -= 360.0f;
		else if (vehicleWheelSpin < -360.0f) vehicleWheelSpin += 360.0f;
		
		//rotate wheels
		for (int i = 0; i < wheelList.Count; ++i)
		{
			if (i <= 1) wheelList[i].transform.localRotation = Quaternion.Euler(new Vector3(vehicleWheelSpin, (vehicleWheelTurn * carData.maxWheelTurn), 0));
			else wheelList[i].transform.localRotation = Quaternion.Euler(new Vector3(vehicleWheelSpin, 0, 0));
		}
	}
	
	protected override void FixedUpdateSpecific()
	{
		// Test
		//car.GetComponent<Rigidbody>().centerOfMass = com;

		if (frozen) return;


		float tVehicleSpeed = Vector3.Dot(car.GetComponent<Rigidbody>().velocity, car.transform.forward); // we get this again since this is fixedUpdate
		Vector3 tAngVelocity = car.GetComponent<Rigidbody>().angularVelocity;


		if (!frozen && tVehicleSpeed < 0.1f && !vehicleInUseData.inUse)
		{
			//gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
			frozen = true;
		}

		float tAdjTurnGain;
		Vector3 tWheelPos;
		Vector3 tPointVelocity;
		float tForceFloat;
		Vector3 tForceVec;
		
		float tPowerCoeff = 0.0f;  // increased at each wheel that's on the ground
		float tPowerCoeffOffset = 1f/wheelList.Count;

		bodyUpVector = car.transform.up;
		bodyDownVector = bodyUpVector * -1;
		hovering = false;

		// cast rays for each wheel
		// this is the code that actually 'floats' the car!
		for(int i = 0; i < wheelList.Count; ++i)
		{
			int currentWheel = i;
			tWheelPos = gameObject.transform.TransformPoint(wheelLocalList[currentWheel]); // VERY important! This is NOT the actual wheel position, but where it was on startup (in the actual model)!
			ray = new Ray(tWheelPos, bodyDownVector);
			Debug.DrawLine(ray.origin, ray.origin + (ray.direction*carData.hoverOffset));
			
			if (Physics.Raycast(ray, out rayCastHit, carData.hoverOffset, layerMask))
			{  

//				if (vehicleInUseData.inUse) Debug.Log (rayCastHit.collider);

				wheelOffsetList[currentWheel] = rayCastHit.distance; 
				tForceFloat = carData.strength * (carData.hoverOffset - wheelOffsetList[currentWheel]);
				tPointVelocity = GetComponent<Rigidbody>().GetPointVelocity(tWheelPos); // calculate velocity of wheel (for damping)
				tForceFloat = tForceFloat + (carData.damping * Vector3.Dot(tPointVelocity, bodyDownVector)); // compute damping
				tForceVec = bodyUpVector * (tForceFloat * carData.mass);
				car.GetComponent<Rigidbody>().AddForceAtPosition(tForceVec, tWheelPos);
				
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
				wheelList[i].transform.localPosition = wheelLocalList[i] + (carData.hoverOffset - wheelRadiusList[i]) * bodyDownVector; // update wheel location
			}
		}
		
		bool tSteering = true;
		if (hovering)
		{
			// steering
			// disabled this steering code... I can't brake and steer and the same time which annoys me beyond the edge of the universe
			// had to be enabled... yes
			// only useful for PC, don't use for mobile devices
			if (Data.platform == "PC")
			{  
				if ((tVehicleSpeed < 0.0f) && (controller.verAxis > 0.01f)) tSteering = false;
				else if ((tVehicleSpeed > 0.0f) && (controller.verAxis < -0.01f)) tSteering = false;
			}
			
			// we store these vars seperately
			float tAdjMaxSpeed = carData.maxSpeed;
			float tAdjMaxSpeedInverse = carData.maxSpeedInverse;
			
			float tDiff;
			Vector3 tImpulse;
			
			// go forward
			if (controller.verAxis > 0.01f)
			{  
				tDiff = Mathf.Min(tAdjMaxSpeed - tVehicleSpeed, 30.0f);  // maxed at 30 so that we don't apply a huge force when we're just driving away
				tImpulse = new Vector3(0,0,1) * (controller.verAxis * tPowerCoeff * carData.mass * tDiff * carData.accelerationGain);  // note the parentheses: compute all floats and only then multiply with vector
				car.GetComponent<Rigidbody>().AddRelativeForce(tImpulse);
			}
			
			// go backward/brake
			if (controller.verAxis < -0.01f)
			{
				tDiff = (tAdjMaxSpeedInverse) - tVehicleSpeed;  // in general a negative number			
				tImpulse = new Vector3(0,0,-1) * (controller.verAxis * tPowerCoeff * carData.mass * tDiff * carData.decelerationGain);  // note the parentheses: compute all floats and only then multiply with vector
				car.GetComponent<Rigidbody>().AddRelativeForce(tImpulse);
			}
			
			// compute turn gain
			// adjust turn gain when below 22.5% of max speed
			tAdjTurnGain = carData.turnGain;  // turn gain
			if (Mathf.Abs(tVehicleSpeed) <= (tAdjMaxSpeed * .225f))
			{  
				float tPerc = Mathf.Abs(tVehicleSpeed) / (tAdjMaxSpeed * .225f);
				tAdjTurnGain = carData.turnGain * tPerc;
				if (tAdjTurnGain < 0.05f) tAdjTurnGain = 0.0f;
			}
			
			// apply extra turn gain when drifting
			if (controller.drift)
			{  
				float tMultFix = carData.currentSpeedPerc;  // makes it so you won't turn insanely fast when almost still
				if (tMultFix > 0.0)
				{
					if (tMultFix > 0.6) tMultFix = 1.0f;
					else                tMultFix *= 1.6667f;  // at 0.6 should be 1 so (1/0.6)
					//tAdjTurnGain *= (5.0f * tMultFix);
				}
			}
			
			// Left/Right steering
			if (tSteering)
			{
				// Traditional turning
				if (controller.horAxis < -0.01f)
				{
					tImpulse = new Vector3(0,-1,0) * (controller.horAxis * -tAdjTurnGain * carData.mass * tPowerCoeff);
					if (tVehicleSpeed < 0.0f) tImpulse = -tImpulse;  // invert steering when driving backward
					car.GetComponent<Rigidbody>().AddRelativeTorque(tImpulse);
				}
				
				if (controller.horAxis > 0.01f)
				{
					tImpulse = new Vector3(0,1,0) * (controller.horAxis * tAdjTurnGain * carData.mass * tPowerCoeff);
					if (tVehicleSpeed < 0.0f) tImpulse = -tImpulse;  // invert steering when driving backward
					car.GetComponent<Rigidbody>().AddRelativeTorque(tImpulse);
				}
			}
			
			// this is a small & weird, but important 'hack'. This'll make sure the car won't turn left/right uncontrolable.
			// The car will 'straighten' itself...
			float tAngVelLength = tAngVelocity.magnitude;
			if (tAngVelLength > 0.0f)
			{
				float tPerc = 1.0f;
				float tVehicleSpeedAbs = Mathf.Abs(tVehicleSpeed);
				float tCompensateGain = 0.25f + (tAdjTurnGain * tPerc);
				if (tCompensateGain > tAdjTurnGain) tCompensateGain = tAdjTurnGain;
				tImpulse = Vector3.up * (Vector3.Dot(tAngVelocity, bodyUpVector) * (-tCompensateGain * carData.mass * tPowerCoeff));
				car.GetComponent<Rigidbody>().AddRelativeTorque(tImpulse);

				// WHAT THE FUCK IS THIS!
				if (tVehicleSpeedAbs < 20.0f) car.GetComponent<Rigidbody>().angularVelocity *= 1.0f - (0.1f - tVehicleSpeedAbs*0.005f);
				if (tVehicleSpeedAbs <  5.0f) car.GetComponent<Rigidbody>().angularVelocity *= 1.0f - (0.2f - tVehicleSpeedAbs*0.04f);
			}			
			
			// handle drifting
			if (controller.drift)
			{
				vehicleSlip = true;
				vehicleAdjGrip = carData.driftGrip;
			}
			
			if (vehicleSlip)
			{
				if (vehicleAdjGrip < carData.grip) vehicleAdjGrip += 0.1f;
				if (vehicleAdjGrip > carData.grip)
				{
					vehicleAdjGrip = carData.grip;
					vehicleSlip = false;
				}
			}
			
			// Work out how fast we're sliding left/right and compensate according to the grip property
			// We just allow the car to slide if it's slipping (with slip key)
			float tSlideSpeed = Vector3.Dot(car.transform.right, car.GetComponent<Rigidbody>().velocity);
			tImpulse = new Vector3(1,0,0) * (-tSlideSpeed * carData.mass * vehicleAdjGrip * tPowerCoeff);
			// Apply an impulse to compensate for sliding
			car.GetComponent<Rigidbody>().AddRelativeForce(tImpulse);
			
			// Apply Drag proportional to speed, but not when we press forward key so we can actually get to our max speed (drag prevents us from getting the max speed otherwise)
			if (!(controller.verAxis>0.01f))
			{
				tImpulse = new Vector3(0,0,-1) * (tVehicleSpeed * carData.drag * carData.mass);
				car.GetComponent<Rigidbody>().AddRelativeForce(tImpulse);
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
						ResetCar();
						upsideDownTimer = 0.0f;
					}
				} else
				{
					upsideDownTimer = 0.0f;
				}
			}
		}
		
		// makes sure we haven't fallen beneath the street
		// It is sort of a hack, but if you comment this and get into the sportscar and start driving against the stadium pillars there is a chance that you will get thrown under the street.
		// Should be a harmless hack otherwise.
		//if (vehicleInUseData.inUse) Grounded ();
		
		//currentSpeed = car.GetComponent<Rigidbody>().velocity;
	}

	// TEMP: RESET CAR
	public void ResetCar()
	{		
		Debug.Log ("Not working yet!");
		car.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
		car.GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
	}
		
	void CreateSkidMarks () 
	{
		int amountOfWheels = wheelList.Count;
		pSkidMarkObject = new GameObject[amountOfWheels];
		pSkidMarkScript = new Skidmarks[amountOfWheels];
		pSkidMarkLastIndex = new int[amountOfWheels];
		
		for(int i=0;i<amountOfWheels;i++)
		{
			pSkidMarkObject[i] = new GameObject();
			pSkidMarkObject[i].name = "Skidmark";
			pSkidMarkObject[i].AddComponent<MeshFilter>();
			pSkidMarkObject[i].AddComponent<MeshRenderer>();
			pSkidMarkObject[i].GetComponent<Renderer>().sharedMaterial = Loader.LoadMaterial("Effects/Skidmarks_Material" );
			pSkidMarkObject[i].AddComponent<Skidmarks>();
			pSkidMarkScript[i] = pSkidMarkObject[i].GetComponent<Skidmarks>();
			pSkidMarkLastIndex[i] = -1;
		}
		
		skidMarksActive = true;
	}
	
	void DestroySkidMarks()
	{
		int amountOfWheels = wheelList.Count;
		for (int i = 0; i < amountOfWheels; i++)
		{
			Destroy(pSkidMarkObject[i]);
			pSkidMarkObject[i] = null;
		}
		
		skidMarksActive = false;
	}
	
	void UpdateSkidMarks()
	{
		int amountOfWheels = wheelList.Count;
		float tBlend = Mathf.Min(0.7f, Mathf.Abs(carData.currentSpeed) * 0.01f);
		Vector3 yOffset = new Vector3 (0, 0, 0);
		if(controller.drift && hovering)
		{
			for (int i = 0; i < amountOfWheels; i++)
			{
				yOffset.y = wheelRadiusList[i];
				pSkidMarkLastIndex[i] = pSkidMarkScript[i].AddSkidMark(wheelList[i].transform.position -yOffset, Vector3.up, tBlend, pSkidMarkLastIndex[i]);
			}
		}
		else
		{
			for (int i = 0; i < amountOfWheels; i++) pSkidMarkLastIndex[i] = -1;
		}
		
	}
	
	void CreateDriftSmoke()
	{
		int amountOfWheels = wheelList.Count;
		pDriftSmokeObject = new GameObject[amountOfWheels];
		
		for (int i = 0; i < amountOfWheels; i++)
		{
			pDriftSmokeObject[i] = Loader.LoadGameObject("Effects/CarDriftSmoke_Prefab");
			pDriftSmokeObject[i].transform.position = wheelList[i].transform.position - new Vector3(0, wheelRadiusList[i], 0);
			pDriftSmokeObject[i].transform.parent = transform;
			pDriftSmokeObject[i].SetActive(false);
		}
		
		driftSmokeActive = true;
	}
	
	void DestroyDriftSmoke()
	{
		int amountOfWheels = wheelList.Count;
		
		for (int i = 0; i < amountOfWheels; i++)
		{
			Destroy (pDriftSmokeObject[i]);
			pDriftSmokeObject[i] = null;
		}
		
		driftSmokeActive = false;
	}
	
	void UpdateDriftSmoke()
	{
		foreach(GameObject obj in pDriftSmokeObject)
		{
			if (hovering) obj.SetActive(controller.drift);
			else obj.SetActive(false);
		}
	}
	

	void Grounded()
	{
		Ray tRay = new Ray (transform.position + new Vector3(0, 1, 0), Vector3.down);
		RaycastHit tRayCastHit;
		LayerMask tLayerMask = (1 << 0) | (1 << 13);
		if (Physics.Raycast(tRay, out tRayCastHit, 1000, tLayerMask))
		{
			if (!tRayCastHit.collider.gameObject.name.Contains("InvisibleWall")) 
			{
				lastGroundedPosition = tRayCastHit.point;
				return;
			}
		}
		Bounds bounds = vehicle.GetComponent<Renderer>().bounds;
		tRay = new Ray (lastGroundedPosition + new Vector3(0, 1, 0), Vector3.down);
		if (Physics.Raycast(tRay, out tRayCastHit, 1000, tLayerMask))
		{
			transform.position = tRayCastHit.point + new Vector3(0, bounds.extents.y, 0);
			transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
		}
	}

	protected override void EnterVehicleSpecific()
	{
		CreateSkidMarks ();
		CreateDriftSmoke ();
		gameObject.GetComponent<Rigidbody>().isKinematic = false;
		frozen = false;
	}

	protected override void ExitVehicleSpecific()
	{
		DestroySkidMarks();
		DestroyDriftSmoke();
	}


}
