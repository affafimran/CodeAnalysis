// Description : CarController.cs : car engine
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
																	

public class CarController : MonoBehaviour {


	public bool 			SeeInspector = false;


	public  LayerMask 		myLayerMask;							// Ignore specific Layer when physic raycasting is used	

	public bool 			b_Pause = false;						// Use to pause this script

	public AnimationCurve 	curveRotation;
	public AnimationCurve 	curveAcceleration;

	public int				playerNumber		= 0;				// Know if it is player 1 2 3 or 4

// --> Inputs Gamepad
	public string			Input_Left			= "Left";
	public string			Input_Right			= "Right";
	public string			Input_Accelerator	= "Vertical";
	public string			Input_Break			= "";
	public string			Respawn				= "";

// --> Inputs Keyboard
	public KeyCode 			key_Left 	= KeyCode.LeftArrow;
	public KeyCode 			key_Right 	= KeyCode.RightArrow;
	public KeyCode 			key_Up 		= KeyCode.UpArrow;
	public KeyCode 			key_Down 	= KeyCode.DownArrow;
	public KeyCode 			key_Respawn = KeyCode.H;


	public float 			MaxSpeed 			= 3F;						// car maximumu speed
	public float 			offsetSpeedDifficultyManager 	= 0;			// car offset speed. Set in Difficulty Manager
	public float 			offsetSpeedForMobile 	= 0;					// car offset speed. Set in Game_Manager Manager
	public float 			CarRotationSpeed 	= 1.5F;						// car rotation speed
	public float 			offsetRotationForMobile 	= 0;				// car offset rotation. Set in Game_Manager Manager
	private float 			Speed 				= 0;						// current speed
	private Rigidbody 		rb;												// access car rigidbody component

	public float 			springConstant  	= 0.0F;
	public float 			damperConstant  	= 0.0F;

// --> Spring Height
	public float			SpringHeight		= .11F;						// use to set the spring height on custom editor						
	public float 			restLength  		= 0.0F;						// spring length			
	public float 			dis 				= .2F;						// wheel raycast length

	public float 			offsetWheelFront 	= 1;						// use to set the wheel model position
	public float 			offsetWheelRear 	= 1;						// use to set the wheel model position

	public Vector3 			eulerAngleVelocity;
	public float 			Force 				= 120;						// force apply to car when acceleration is activated
	public float 			BrakeForce 			= 35;						// natural car break force

	public Transform 		t_ApplyForce;

	public float 			Coeff 				= .15F;						// forward sliding coefficient
	public float 			refCoeffZ_Min 		= .5F;	
	public float 			refCoeffZ_Max 		= 1.5F;
	public float 			CoeffZWhenCarIsSlow	= 10f;								
	public float 			CoeffZ 				= 1.5F;						// Side sliding coefficient

	public Vector3 			com;											// Center Of Mass

	private float 			DirBackward_ 		= 1;						// car direction

	public int 				NumberOfWheelThatTouchGround 	= 0;


// --> Wheels
	public float			LenghtWheelsDistance	= .127F;				// use in custom editor	
	public float			RearWheelsDistance		= .07F;					// use in custom editor	
	public float			FrontWheelsDistance		= .07F;					// use in custom editor	
	public GameObject[] 	RayCastWheels;

	private float[] 		springforce2 		= new float[4];				// use to calcuate car spring
	private float[] 		damperForce2 		= new float[4];				// use to calcuate car spring
	private float[] 		previous_Length2 	= new float[4];				// use to calcuate car spring
	private float[] 		current_Length2 	= new float[4];				// use to calcuate car spring
	private float[] 		springVelocity2 	= new float[4];				// use to calcuate car spring

	public GameObject		rearCapsuleCollider;							// use in custom editor	
	public GameObject		frontCapsuleCollider;							// use in custom editor	

	public float			PicWheelSize		= .19F;						// use in custom editor	
	public float			WheelSizeRear		= 0F;						// use in custom editor	
	public float			WheelSizeFront		= 0F;						// use in custom editor	

	public GameObject[] 	Wheel_X_Rotate 		= new GameObject[4];
	public List<float> 		Wheel_X_RefLocalPosition	= new List<float>();// Save when the script Awake the local position of the 4 Wheel_X_Rotate. 
	public GameObject[] 	Wheel_Z_Rotate 		= new GameObject[4];
	public float[] 			tmpAngle 			= new float[4];

// --> Audio
	public AudioSource 		audio_;											// Audiosource accelerator
	public float 			MaxAudioPitch		= .9F;
	public AudioSource 		objSkid_Sound;									// Audiosource skid sound
	public AudioSource		obj_CarImpact_Sound;							// Audiosource Impact sound
	public float			impactVolumeMax 	= .3F;
	private bool 			Once 				= false;					// use to play only one time a sound
		
	public bool 			b_Grounded_Audio	= false;


	public float 			tmpmulti 			= 0;						// coefficient used when car use acceleration
	public float 			tmprotate 			= 0;						// coefficient used when car use break

	public float 			Input_Acceleration 	= 0;						// acceleration between -1 and 1
	public bool 			b_btn_Acce 			= false;					// use with car AI and mobile inputs						
	public bool 			b_btn_Break 		= false;					// use with car AI and mobile inputs		
	public float 			btn_Rotation 		= 0;						// use with car AI and mobile inputs		
	public bool 			b_btn_Left 			= false;					// use with car AI and mobile inputs		
	public bool 			b_btn_Right 		= false;					// use with car AI and mobile inputs	
	public bool 			b_RespawnMobile 		= false;					// use with car AI and mobile inputs	

	public GameObject 		CarBodyCollider;								// use in custom editor	

	public CarPathFollow 	carPathFollow;									// Car Path

// --> car respawn
	private float 			timerBeforeRespawn = 0;							// if timerBeforeRespawn = 3 the car respawn
	public GameObject 		RespawnCloud;									// Instantiate this gameobject when car respawn
	public bool				b_CarIsRespawning	= false;
	public float 			DurationBeforeCarRespawn = 1;
	public Transform		objRespawn;

	public GameObject		Capsule_Rear;									// use when car is deactivate or activated
	public GameObject		Capsule_Front;									// use when car is deactivate or activated
	public GameObject		BodyCollider;									// use when car is deactivate or activated
	public Collider			ColliderCarOnBack;								// use when car is deactivate or activated

	public GameObject		Grp_BodyPlusBlobShadow;							// use when car is deactivate or activated


// --> Car Selection
	public Transform		pivotCarSelection;								// Transform use when the car is spawn on the Car Selection Menu
	public float 			pivotOffsetZ = 0;								// Offset use on the Custom Editor

// --> Car AI
	public bool 			b_AutoAcceleration = false;						// if true car is control with the AI
	public bool				b_random = false;
	public bool				b_allowRandomCarValue = true;
	public CarAI 			carAI;											// access the carAI component
	public float			randomSpeedOffset = 0;							// Random value for car speed when AI is used
	public bool				b_CarAccelerate			= false;

// --> Car Fake body rotation
	public float 			tmpFakeBodyRotation = 0;						// use to move the car body. Fake Left and right movement
	public float			BodyRotationValue	= 5;						// use to move the car body. Fake Left and right movement. Custom Editor						

// --> Limit car slinding
	public bool				StopAcceleration = false;						// Use to Prevent the car moving on vertical elements
	private float 			Slide_01 = 1;									// use to limit car sliding
	private float 			Slide_02 = 0;									// use to limit car sliding
	private float 			Slide_03 = 0;									// use to limit car sliding
	public AnimationCurve 	curve_01;										// use to limit car sliding


	public float 			ReachMaxRotation = 0;
	public float 			ReachMaxRotationAcc = 0;

	public int 				turnDirection = 0;								// Prevent bug when player press left and right inputs
	public bool 			b_CarMoveForward = true;						// Know if the car move forward. Use to apply rotation when player is moving forward or backward

	public float 			t = 0;

	public bool 			raceIsFinished = false;							// true when the race is finished. call by LapCOunter.cs


	private Game_Manager	gameManager;

	public bool 			b_CountdownActivate	= false;					// if countdown is use in the scene. the car doesn't move
	public bool   			b_MaxAccelerationAfterCountdown = true;			// Acceleration is maximum for car AI when the countdown ended


	private Vector3 		tmpVelocity;									// --> Car Pause State

	public GameObject 		camTarget;										// Use to initialized the Camera when GameStart Call by script Cam_Follow.cs on P1_Cam or P2_Cam
	public bool				b_InitInputWhenGameStart = true;
// --> Awake 
	void Awake(){
		for(var i = 0;i< Wheel_X_Rotate.Length;i++){
			Wheel_X_RefLocalPosition.Add (Wheel_X_Rotate [i].transform.localPosition.y);		// Save the wheel position reference
		}

		GameObject tmpObj = GameObject.Find ("Game_Manager");
		if (tmpObj) {
			gameManager = tmpObj.GetComponent<Game_Manager> ();
			b_CountdownActivate = gameManager.b_UseCountdown;
		}
		
	}

// --> Use this for initialization
	void Start () {
		if(b_InitInputWhenGameStart)
		UpdateCarInputs ();

		rb = GetComponent<Rigidbody>();
		audio_ = audio_.gameObject.GetComponent<AudioSource>();
		objSkid_Sound = objSkid_Sound.gameObject.GetComponent<AudioSource>();
		obj_CarImpact_Sound = obj_CarImpact_Sound.gameObject.GetComponent<AudioSource>();

		if (this.GetComponent<CarAI> ())
			carAI = this.GetComponent<CarAI> ();

		if (carAI.enabled)
			b_AutoAcceleration = true;

		InitCarAudioSource ();

	}

// --> Update the car inputs when the game starts
	public void UpdateCarInputs(){
		Input_Left			= PlayerPrefs.GetString ("PP_" + playerNumber + "_Pad_Left");
		Input_Right			= PlayerPrefs.GetString ("PP_" + playerNumber + "_Pad_Right");
		Input_Accelerator	= PlayerPrefs.GetString ("PP_" + playerNumber + "_Pad_Acceleration");
		Input_Break			= PlayerPrefs.GetString ("PP_" + playerNumber + "_Pad_Break");
		Respawn				= PlayerPrefs.GetString ("PP_" + playerNumber + "_Pad_Respawn");

		key_Left 	= FindTheKeyCodeUpdate(PlayerPrefs.GetString ("PP_" + playerNumber + "_Desktop_Left"));
		key_Right 	= FindTheKeyCodeUpdate(PlayerPrefs.GetString ("PP_" + playerNumber + "_Desktop_Right"));
		key_Up 		= FindTheKeyCodeUpdate(PlayerPrefs.GetString ("PP_" + playerNumber + "_Desktop_Acceleration"));
		key_Down 	= FindTheKeyCodeUpdate(PlayerPrefs.GetString ("PP_" + playerNumber + "_Desktop_Break"));
		key_Respawn = FindTheKeyCodeUpdate(PlayerPrefs.GetString ("PP_" + playerNumber + "_Desktop_Respawn"));


		if (playerNumber == 1) {																					// Activate the mobile inputs if needed
			GameObject tmpObj = GameObject.Find ("Canvas_MobileButtons");
			if(tmpObj)tmpObj.GetComponent<InitMobileInputs> ().F_InitMobileButtons (gameObject);
		}
	}

// --> find the input
	public KeyCode FindTheKeyCodeUpdate(string playerLoad){																
		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{
			if(key.ToString () == playerLoad)
			{
				return key;
			}
		}
		return KeyCode.None;
	}

// --> Initialized car sounds
	public void InitCarAudioSource(){
		GameObject cameraMainMenu	= GameObject.FindGameObjectWithTag ("CameraMainMenu");				// Use to prevent bug when car is used on Main Menu. We don't the car sound in this case

		if (cameraMainMenu == null)
			audio_.mute = false;


		if (!carAI.enabled) {								// car controlled by PLayer
			audio_.spatialBlend = .25f;						// Audiosource accelerator spatialBlend
			objSkid_Sound.spatialBlend = 0;					// Audiosource skid sound spatialBlend
			obj_CarImpact_Sound.spatialBlend = .5f;			// Audiosource Impact sound spatialBlend
		} else {											// car controlled by CPU
			audio_.spatialBlend = 1;						// Audiosource accelerator spatialBlend
			objSkid_Sound.spatialBlend = 1;					// Audiosource skid sound spatialBlend
			obj_CarImpact_Sound.spatialBlend = 1;			// Audiosource Impact sound spatialBlend
		}
	}


// --> Next functions are move and turn on mobile and for the car AI
	public void Btn_AccelerationActivate(){
		b_btn_Acce = true;
		b_btn_Break = false;
	}
	public void Btn_AccelerationDeactivate(){
		b_btn_Acce = false;
		b_btn_Break = false;
	}

	public void Btn_BreakActivate(){
		b_btn_Break = true;
	}
	public void Btn_BreakDeactivate(){
		b_btn_Acce = false;
		b_btn_Break = false;
	}

	public void Btn_LeftActivate(){
		b_btn_Left = true;
		b_btn_Right = false;
	}
	public void Btn_LeftDeactivate(){
		b_btn_Left = false;
	}

	public void Btn_RightActivate(){
		b_btn_Right = true;
		b_btn_Left = false;
	}
	public void Btn_RightDeactivate(){
		b_btn_Right = false;
	}

	public void Btn_Respawn(){
		if (!raceIsFinished
		    && !b_CountdownActivate
		    && !b_CarIsRespawning) {
			b_RespawnMobile = true;
		}
	}

// --> Fixed Update fonction
	void FixedUpdate () {
		if(!b_Pause || !b_CarIsRespawning && !b_Pause){
			if ((Input.GetKey (key_Respawn) 
				|| b_RespawnMobile
				|| (playerNumber < 3 && Input.GetButton (Respawn)) )
				 
				&& !raceIsFinished 
				&& !b_CountdownActivate 
				&& !b_CarIsRespawning
				&& !b_AutoAcceleration) {
				b_RespawnMobile = false;
				Debug.Log ("Respawn");
				RespawnTheCar ();
			}
		




// --> Section input to accelerate or break	
				if (Input_Accelerator != Input_Break) {															// -> We don't use the same axis for Accreleration and break
					if (b_btn_Acce// mobile
					    || !b_AutoAcceleration && Input.GetAxisRaw (Input_Accelerator) == 1// gamepad
						|| !b_AutoAcceleration && Input.GetKey (key_Up)) {										// desktop
						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, 1, Time.deltaTime * 8F);

						b_CarMoveForward = true;
						b_CarAccelerate = true;
					} else if (b_btn_Break// mobile
					           || !b_AutoAcceleration && Input.GetAxisRaw (Input_Break) > 0// gamepad
					           || !b_AutoAcceleration && Input.GetKey (key_Down)) {									// desktop
			
						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, -1, Time.deltaTime * 4F);
						b_CarMoveForward = false;
						b_CarAccelerate = true;
					} else {
						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, 0, Time.deltaTime * 4F);
						b_CarAccelerate = false;
					}
				} else {																						// -> We use the same axis for Acceleration and break
					if (b_btn_Acce// mobile
					    || !b_AutoAcceleration && Input.GetAxisRaw (Input_Accelerator) < -.5F// gamepad
					    || !b_AutoAcceleration && Input.GetKey (key_Up)) {										// desktop
						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, 1, Time.deltaTime * 8F);
						b_CarAccelerate = true;

					} else if (b_btn_Break// mobile
					           || !b_AutoAcceleration && Input.GetAxisRaw (Input_Accelerator) > .5F// gamepad
					           || !b_AutoAcceleration && Input.GetKey (key_Down)) {									// desktop

						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, -1, Time.deltaTime * 4F);
						b_CarAccelerate = true;

					} else {

						Input_Acceleration = Mathf.MoveTowards (Input_Acceleration, 0, Time.deltaTime * 4F);
						b_CarAccelerate = false;
					}
				}

// --> Inputs to turn left and right
				if (Input_Left != Input_Right) {																// -> We don't use the same axis for turning left and right
					if (!b_AutoAcceleration && b_btn_Left && turnDirection != 1										// mobile
							|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Left) >= .4F && turnDirection != 1	// gamepad
							|| !b_AutoAcceleration && Input.GetKey (key_Left) && turnDirection != 1					// desktop
							|| b_AutoAcceleration && b_btn_Left														// AI
					) {
					ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 1, Time.deltaTime * (CarRotationSpeed+offsetRotationForMobile));
						btn_Rotation = Mathf.MoveTowards (btn_Rotation, -1, Time.deltaTime * 6 * curveRotation.Evaluate (ReachMaxRotation));
						turnDirection = -1;

					} else if (!b_AutoAcceleration && b_btn_Right && turnDirection != -1							// mobile
							|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Right) >= .4F && turnDirection != -1	// gamepad
							|| !b_AutoAcceleration && Input.GetKey (key_Right) && turnDirection != -1				// desktop
							|| b_AutoAcceleration && b_btn_Right													// AI
					) {
					ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 1, Time.deltaTime * (CarRotationSpeed+offsetRotationForMobile));
						btn_Rotation = Mathf.MoveTowards (btn_Rotation, 1, Time.deltaTime * 6 * curveRotation.Evaluate (ReachMaxRotation));
						turnDirection = 1;

					} else if (!b_AutoAcceleration && !b_btn_Left && !b_btn_Right									// mobile
							|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Left) == 0 && Input.GetAxisRaw (Input_Right) == 0// gamepad
							|| !b_AutoAcceleration && !Input.GetKey (key_Left) && !Input.GetKey (key_Right)			// desktop
							|| b_AutoAcceleration && !b_btn_Left && !b_btn_Right									// AI
					) {
						btn_Rotation = Mathf.MoveTowards (btn_Rotation, 0, Time.deltaTime * 5F);
						ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 0, Time.deltaTime * 5F);
						turnDirection = 0;
					}
				} 
				else {																							// -> We use the same axis for turning left and right
					if (!b_AutoAcceleration && b_btn_Left && turnDirection != 1												// mobile
									|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Left) <= -.4F && turnDirection != 1	// gamepad
									|| !b_AutoAcceleration && Input.GetKey (key_Left) && turnDirection != 1					// desktop
									|| b_AutoAcceleration && b_btn_Left														// AI
							){
					ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 1, Time.deltaTime * (CarRotationSpeed+offsetRotationForMobile));
								btn_Rotation = Mathf.MoveTowards (btn_Rotation, -1, Time.deltaTime * 6 * curveRotation.Evaluate (ReachMaxRotation));
								turnDirection = -1;
					} else if (!b_AutoAcceleration && b_btn_Right && turnDirection != -1									// mobile
									|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Right) >= .4F && turnDirection != -1	// gamepad
									|| !b_AutoAcceleration && Input.GetKey (key_Right) && turnDirection != -1				// desktop
									|| b_AutoAcceleration && b_btn_Right													// AI
							){
					ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 1, Time.deltaTime * (CarRotationSpeed+offsetRotationForMobile));
								btn_Rotation = Mathf.MoveTowards (btn_Rotation, 1, Time.deltaTime * 6 * curveRotation.Evaluate (ReachMaxRotation));
								turnDirection = 1;
					} else if (!b_AutoAcceleration && !b_btn_Left && !b_btn_Right											// mobile
									|| !b_AutoAcceleration && Input.GetAxisRaw (Input_Left) == 0							// gamepad
									|| !b_AutoAcceleration && !Input.GetKey (key_Left) && !Input.GetKey (key_Right)			// desktop
									|| b_AutoAcceleration && !b_btn_Left && !b_btn_Right									// AI
							){
								btn_Rotation = Mathf.MoveTowards (btn_Rotation, 0, Time.deltaTime * 5F);
								ReachMaxRotation = Mathf.MoveTowards (ReachMaxRotation, 0, Time.deltaTime * 5F);
								turnDirection = 0;
							}
				}
			


// --> create a fake rotation on car body when car turn left or right
			if (rb.velocity.magnitude > .2f && (turnDirection == 1 && !b_AutoAcceleration || turnDirection == 1 && b_AutoAcceleration && Mathf.Abs(carAI.angle) > 10)) {					// --> fake z body rotation
				if(b_AutoAcceleration)	tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, -1*(BodyRotationValue+2), 10 * Time.deltaTime);							// car AI
				else  					tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, -1*BodyRotationValue, 100 * Time.deltaTime);						// player

			} else if (rb.velocity.magnitude > .2f && (turnDirection == -1 && !b_AutoAcceleration || turnDirection == -1 && b_AutoAcceleration && Mathf.Abs(carAI.angle) > 10)) {
				if(b_AutoAcceleration)	tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, BodyRotationValue+2, 10 * Time.deltaTime);							// car Ai
				else  					tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, BodyRotationValue, 100 * Time.deltaTime);							// Player
			}
			else {
				if(b_AutoAcceleration)	tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, 0, 10 * Time.deltaTime);							// car AI
				else  					tmpFakeBodyRotation = Mathf.MoveTowards (tmpFakeBodyRotation, 0, 100 * Time.deltaTime);							// player
			}


			Grp_BodyPlusBlobShadow.transform.localEulerAngles = new Vector3 (	
				Grp_BodyPlusBlobShadow.transform.localEulerAngles.x,
				Grp_BodyPlusBlobShadow.transform.localEulerAngles.y,
				tmpFakeBodyRotation);



		NumberOfWheelThatTouchGround = 0;
		Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);

// --> Calculate the car spring for each wheel 
		RaycastHit hit;
		StopAcceleration = false;
		for(int i = 0;i< RayCastWheels.Length;i++){
			Vector3 dir = RayCastWheels[i].transform.TransformDirection(-Vector3.up);

			if (Physics.Raycast(RayCastWheels[i].transform.position, dir, out hit,dis,myLayerMask)){
				previous_Length2[i] = current_Length2[i];
				current_Length2[i] = restLength -  hit.distance;
				springVelocity2[i] = (current_Length2[i] - previous_Length2[i])/Time.fixedDeltaTime;
				springforce2[i] = springConstant * current_Length2[i];
				damperForce2[i] = damperConstant * springVelocity2[i];


// --> Find the car angle
					float carAngle = 0;
					Vector3 tangente = new Vector3 ();
					RaycastHit hit2;
					if (Physics.Raycast(rb.transform.position, -rb.transform.up, out hit2,10,myLayerMask)){						
						hit2.normal.Normalize();
						var distance = -Vector3.Dot(hit2.normal, Vector3.up);    
						//Debug.DrawRay(hit2.point, (Vector3.up + hit2.normal * distance).normalized , Color.white);
						tangente = (Vector3.up + hit2.normal * distance).normalized;

						carAngle = Vector3.Angle(tangente, -Vector3.up);														// the current car angle
						//Debug.DrawRay(hit2.point,  -Vector3.up, Color.cyan);
					}


// --> limit sliding on hill			
					if(rb.velocity.magnitude < 2f && Input_Acceleration == 0) {
						if (rb.velocity.magnitude < .5f) {																		// the car will stop
							Slide_03 = Mathf.MoveTowards (Slide_03, 1, Time.deltaTime);
							Slide_01 = Mathf.MoveTowards (Slide_01, 0, Time.deltaTime * .1f * curve_01.Evaluate(Slide_03));
							Slide_02 = Mathf.MoveTowards (Slide_02, 1, Time.deltaTime * .1f * curve_01.Evaluate(Slide_03));
						} 
						else {																									// reduce the speed of the car using his angle
							Slide_01 = Mathf.MoveTowards (Slide_01, 1-(carAngle-90)/90, Time.deltaTime * .1f);
							Slide_02 = Mathf.MoveTowards (Slide_02, 0+(carAngle-90)/90, Time.deltaTime * .1f);
							Slide_03 = 0;
						}
					} else {
						Slide_03 = Mathf.MoveTowards (Slide_03, 1, Time.deltaTime);
						Slide_01 = Mathf.MoveTowards (Slide_01, 1, Time.deltaTime*.5f*curve_01.Evaluate(Slide_03));
						Slide_02 = Mathf.MoveTowards (Slide_02, 0, Time.deltaTime*.5f*curve_01.Evaluate(Slide_03));
					}

// --> Spring force added for each wheel
					rb.AddForceAtPosition((RayCastWheels[i].transform.up*Slide_01+Vector3.up*Slide_02) * (springforce2[i] + damperForce2[i]),RayCastWheels[i].transform.position,ForceMode.Force);
					Debug.DrawRay(RayCastWheels[i].transform.position, dir*dis, Color.red);

					if(Slide_02 >0)
						Debug.DrawRay(RayCastWheels[i].transform.position, (RayCastWheels[i].transform.up*Slide_01+Vector3.up*Slide_02)*.25f, Color.cyan);
					else
						Debug.DrawRay(RayCastWheels[i].transform.position, (RayCastWheels[i].transform.up*Slide_01+Vector3.up*Slide_02)*.25f, Color.yellow);

					if (hit.transform.tag == "Wall") {				// If a gameObject is tagged "Wall" the car could not accelerate on this object
						StopAcceleration = true;
						break;
					}

// --> Move the wheel model in association with the spring
				if(hit.distance > .1){
					if(i == 0 || i == 1){
						Wheel_X_Rotate[i].transform.position 		= new Vector3(Wheel_X_Rotate[i].transform.position.x,hit.point.y -.03f + offsetWheelFront,Wheel_X_Rotate[i].transform.position.z);
						Wheel_X_Rotate[i].transform.localPosition 	= new Vector3(0,Wheel_X_Rotate[i].transform.localPosition.y,0);
					}
					else{
							Wheel_X_Rotate[i].transform.position 		= new Vector3(Wheel_X_Rotate[i].transform.position.x,hit.point.y -.03f + offsetWheelRear,Wheel_X_Rotate[i].transform.position.z);
						Wheel_X_Rotate[i].transform.localPosition 	= new Vector3(0,Wheel_X_Rotate[i].transform.localPosition.y,0);
					}


						if(Wheel_X_Rotate[i].transform.localPosition.y > 0)
							Wheel_X_Rotate[i].transform.localPosition = new Vector3(0,0f,0);

						if (Wheel_X_Rotate [i].transform.localPosition.y <= Wheel_X_RefLocalPosition [i]) {
							float tmpWheelX_LocalPosition = Wheel_X_Rotate[i].transform.localPosition.y;
							tmpWheelX_LocalPosition = Mathf.MoveTowards (tmpWheelX_LocalPosition, Wheel_X_RefLocalPosition[i], Time.fixedDeltaTime);
							Wheel_X_Rotate [i].transform.localPosition = new Vector3 (0, tmpWheelX_LocalPosition, 0);
						}
				}
				else{
					Wheel_X_Rotate[i].transform.localPosition = new Vector3(0,0f,0);
				}

				NumberOfWheelThatTouchGround++;
			}
			else{																														// -> wheel is not touching the ground
				float tmpWheelX_LocalPosition = Wheel_X_Rotate[i].transform.localPosition.y;
				tmpWheelX_LocalPosition = Mathf.MoveTowards(tmpWheelX_LocalPosition, Wheel_X_RefLocalPosition[i],Time.fixedDeltaTime);	// init wheel poisition
				Wheel_X_Rotate[i].transform.localPosition = new Vector3(0,tmpWheelX_LocalPosition ,0);

				Debug.DrawRay(RayCastWheels[i].transform.position, dir*dis, Color.green);
			}
		}


// -->  Use with car to create random reactions. 
			if (Physics.Raycast(rb.transform.position, new Vector3(0,-1,0), out hit,.3F,myLayerMask)){
				/*if(localVelocity.y  < -.2F && !obj_CarImpact_Sound.isPlaying && NumberOfWheelThatTouchGround >2){						// PLay a sound When there his a car impact on ground
					float tmpVolume = 0;
					if(-localVelocity.y * impactVolumeMax > impactVolumeMax-.1F)tmpVolume =  impactVolumeMax-.1F;
					if(-localVelocity.y * impactVolumeMax < impactVolumeMax-.1F)tmpVolume = .1F;
					obj_CarImpact_Sound.volume = tmpVolume;

					obj_CarImpact_Sound.Play();
				}*/

				if(localVelocity.y  < -.2F && NumberOfWheelThatTouchGround >2 && b_allowRandomCarValue){								// Use with car to create random reactions. 
					StartCoroutine ("I_AllowRandomCarValue");
				}
		}

// --> Car Is Grounded
		if(NumberOfWheelThatTouchGround >=2){																						

			Vector3 opposite = rb.transform.InverseTransformDirection(-rb.velocity);
			opposite.z = opposite.z*(Coeff);
			opposite.x = opposite.x*CoeffZ;


			rb.AddRelativeForce(opposite*BrakeForce,ForceMode.Force);
			//	Debug.DrawRay(rb.transform.position, opposite, Color.cyan);
			Vector3 tmpVect = new Vector3(0,0,0);
			tmpVect = t_ApplyForce.position;

			if(Input_Acceleration < 0 ){
				tmpmulti = Mathf.Lerp(tmpmulti, .25F,Time.fixedDeltaTime*4);
			}
			else if(Input_Acceleration > 0){
				tmpmulti = Mathf.Lerp(tmpmulti, 1,Time.fixedDeltaTime*4);
			}

// -> Car Acceleration
			if(b_btn_Acce 
				|| b_btn_Break 
				|| !b_AutoAcceleration && Mathf.Abs(Input.GetAxisRaw(Input_Accelerator)) > .5F 
				|| !b_AutoAcceleration && Mathf.Abs(Input.GetAxisRaw(Input_Break)) > .5F
				|| !b_AutoAcceleration && Input.GetKey(key_Up)
				|| !b_AutoAcceleration && Input.GetKey(key_Down)){

					if (!StopAcceleration) {																		
						ReachMaxRotationAcc = Mathf.MoveTowards (ReachMaxRotationAcc, 1, Time.deltaTime * 2);
						if (!raceIsFinished && !b_CountdownActivate) {
							if (b_AutoAcceleration && b_MaxAccelerationAfterCountdown) {
								b_MaxAccelerationAfterCountdown = false;
								Input_Acceleration = 1;
								tmpmulti = 1;
								CoeffZ = refCoeffZ_Max;
								ReachMaxRotationAcc = 1;
							} 
								
								
							rb.AddForceAtPosition (rb.transform.forward * tmpmulti * Force * 1 * curveAcceleration.Evaluate (ReachMaxRotationAcc) * Input_Acceleration, tmpVect, ForceMode.Force);
						}
					}
			}
			else{
				ReachMaxRotationAcc = 0;
			}

// --> add natural break force
			Vector3 opposite2 = -rb.angularVelocity;
			rb.AddTorque(opposite2*BrakeForce*0.01F);

// --> play skid sound
			if(btn_Rotation == 1){
				tmpAngle[0] = Mathf.MoveTowards(tmpAngle[0],20F,Time.fixedDeltaTime*100);
				if(tmpAngle[0] == 20){
					if(0  == 0)CoeffZ = Mathf.MoveTowards(CoeffZ,refCoeffZ_Min,Time.fixedDeltaTime*40F);

					if (!b_AutoAcceleration) {
						if (!Once && objSkid_Sound && Mathf.Abs (localVelocity.z) > .4F) {
							objSkid_Sound.mute = false;
							Once = true;
						} else if (Once && objSkid_Sound && Mathf.Abs (localVelocity.z) <= .4F) {
							objSkid_Sound.mute = true;
							Once = false;
						}
					}
				}
			}
			else if(btn_Rotation == -1){
				tmpAngle[0] = Mathf.MoveTowards(tmpAngle[0],-20F,Time.fixedDeltaTime*70);
				if(tmpAngle[0] == -20){
					if(0  == 0)CoeffZ = Mathf.MoveTowards(CoeffZ,refCoeffZ_Min,Time.fixedDeltaTime*40F);
					if (!b_AutoAcceleration) {
						if (!Once && objSkid_Sound && Mathf.Abs (localVelocity.z) > .4F) {
							objSkid_Sound.mute = false;
							Once = true;
						} else if (Once && objSkid_Sound && Mathf.Abs (localVelocity.z) <= .4F) {
							objSkid_Sound.mute = true;
							Once = false;
						}
					}
				}
			}
			else{
				tmpAngle[0] = Mathf.MoveTowards(tmpAngle[0],0,Time.fixedDeltaTime*100);
					if(0  == 0)CoeffZ = Mathf.MoveTowards(CoeffZ,refCoeffZ_Max,Time.fixedDeltaTime*10F);
				if(Once && objSkid_Sound && Mathf.Abs(localVelocity.z) > .4F){
					objSkid_Sound.mute = true;
					Once = false;
				}
			}


// --> Wheels Y Rotation
				if(btn_Rotation > 0){
					tmpAngle[1] = Mathf.MoveTowards(tmpAngle[1],20F,Time.fixedDeltaTime*100);}
				else if(btn_Rotation < 0){
					tmpAngle[1] = Mathf.MoveTowards(tmpAngle[1],-20F,Time.fixedDeltaTime*100);}
				else{
					tmpAngle[1] = Mathf.MoveTowards(tmpAngle[1],0,Time.fixedDeltaTime*100);}
			
				for(int i = 0;i< 2;i++){										// Front Wheels rotate
					Wheel_X_Rotate[i].transform.localEulerAngles = new Vector3(0,tmpAngle[1],0);
				}


			Speed = rb.velocity.magnitude;
				float tmpPitch = 0;
				if (!b_CountdownActivate) {
					tmpPitch = .5F + MaxAudioPitch * Speed / (MaxSpeed + randomSpeedOffset + offsetSpeedDifficultyManager+offsetSpeedForMobile);
				}			// Case : race is starting
				else if(b_CountdownActivate && Input_Acceleration>=0){// Case : countdown
					tmpPitch= .5F + (MaxAudioPitch*MaxSpeed*Input_Acceleration)/(MaxSpeed + randomSpeedOffset + offsetSpeedDifficultyManager+offsetSpeedForMobile);
				}	
				else if(b_CountdownActivate && Input_Acceleration<0){// Case : countdown
					tmpPitch= .5F;
				}

			audio_.pitch = Mathf.MoveTowards(audio_.pitch,tmpPitch,Time.fixedDeltaTime);
		}
		else{
			audio_.pitch = Mathf.MoveTowards(audio_.pitch,.5F,Time.fixedDeltaTime);
			if(Once && objSkid_Sound){
				objSkid_Sound.mute = true;
				Once = false;
			}
		}

// --> infos for car rotation
		float tmpAngleRotation = 1;

		float VelocityDir = Vector3.Angle(rb.velocity, rb.transform.forward);									// know if the car go backward or forward

			//Debug.Log(VelocityDir);

			if (Input_Acceleration > 0) {																		// Move forward. Accelerator is Pressed
				DirBackward_ = Mathf.MoveTowards (DirBackward_, 1, 7 * Time.fixedDeltaTime);
				tmpAngleRotation = Mathf.Lerp (tmpAngleRotation, 1, Time.fixedDeltaTime * 4);
			} else if (Input_Acceleration < 0) {																// Move backward. Accelerator is Pressed
				DirBackward_ = Mathf.MoveTowards (DirBackward_, -1, 3 * Time.fixedDeltaTime);
				tmpAngleRotation = Mathf.Lerp (tmpAngleRotation, 7F, Time.fixedDeltaTime * 4);
			} 
			else if (VelocityDir >= 90) {																		// Move backward. Accelerator is not Pressed
				//Debug.Log("Backward");
				DirBackward_ = Mathf.MoveTowards (DirBackward_, -1, 3 * Time.fixedDeltaTime);
				tmpAngleRotation = Mathf.Lerp (tmpAngleRotation, 7F, Time.fixedDeltaTime * 4);

			} else if (VelocityDir >= 0) {																		// Move forward. Accelerator is not Pressed
				//Debug.Log("forward");
				DirBackward_ = Mathf.MoveTowards (DirBackward_, 1, 7 * Time.fixedDeltaTime);
				tmpAngleRotation = Mathf.Lerp (tmpAngleRotation, 1, Time.fixedDeltaTime * 4);
		} 
		

// --> move the center of mass depending the car speed
		if(Mathf.Abs(localVelocity.z) < .4F){
			rb.centerOfMass = new Vector3(com.x,com.y,0);

			tmprotate = Mathf.MoveTowards(tmprotate, 0,Time.fixedDeltaTime*4);
		}
		else {
			rb.centerOfMass = com;
			tmprotate = Mathf.MoveTowards(tmprotate, 1,Time.fixedDeltaTime*4);

			Wheel_Z_Rotate[0].transform.Rotate(Vector3.right*Time.deltaTime*1000*localVelocity.z);
			Wheel_Z_Rotate[1].transform.Rotate(Vector3.right*Time.deltaTime*-1000*localVelocity.z);
			Wheel_Z_Rotate[2].transform.Rotate(Vector3.right*Time.deltaTime*1000*localVelocity.z);
			Wheel_Z_Rotate[3].transform.Rotate(Vector3.right*Time.deltaTime*-1000*localVelocity.z);
		}
		

// -> Car Rotation
			float CoeffRotionIfJump = 1;
			if (NumberOfWheelThatTouchGround <1 && b_AutoAcceleration)
				CoeffRotionIfJump = .7f;
			

		Quaternion deltaRotation = 	
				Quaternion.Euler(eulerAngleVelocity*tmpAngleRotation*tmprotate *Time.fixedDeltaTime * btn_Rotation*DirBackward_*CoeffRotionIfJump);
		
			if (Mathf.Abs (localVelocity.z) > .1F && !b_CountdownActivate) {
				
				rb.MoveRotation (rb.rotation * deltaRotation);
			}


// -> normalized car speed
			if(rb.velocity.magnitude > (MaxSpeed + randomSpeedOffset + offsetSpeedDifficultyManager+offsetSpeedForMobile)){
				rb.velocity = rb.velocity.normalized * (MaxSpeed + randomSpeedOffset + offsetSpeedDifficultyManager+offsetSpeedForMobile);
		}
	}


// --> Incease Z friction when car speed is slow and player do not press accelerator
		if (!b_CarAccelerate && rb.velocity.magnitude < .5f) {										
			CoeffZ = Mathf.MoveTowards (CoeffZ, CoeffZWhenCarIsSlow, Time.fixedDeltaTime * 40F);
		}

// --> Respawn the car after 3 seconds if he doesn't move any more for AI or if player is on his side
		if (!b_Pause && !raceIsFinished && !b_CountdownActivate) {
			if (rb.velocity.magnitude < 0.1f && NumberOfWheelThatTouchGround == 0 && !b_AutoAcceleration && timerBeforeRespawn < 3) {		// Case Car control by player : Respawn if the car does not move during 3 seconds and the car is on the side
				timerBeforeRespawn = Mathf.MoveTowards (timerBeforeRespawn, 3, Time.deltaTime);
				if (timerBeforeRespawn == 3)
					RespawnTheCar ();
			} else if (rb.velocity.magnitude < 0.1f && b_AutoAcceleration && timerBeforeRespawn < 3) {										// Case Car control by CPU : Respawn if the car does not move during 3 seconds and the car is on the side
				timerBeforeRespawn = Mathf.MoveTowards (timerBeforeRespawn, 3, Time.deltaTime);
				if (timerBeforeRespawn == 3)
					RespawnTheCar ();
			} else {
				timerBeforeRespawn = 0;
			}
		}
	}

// --> Find angle
	float AngleDir(Vector3 _forward, Vector3 _targetDir, Vector3 _up){		
		Vector3 perp = Vector3.Cross(_forward, _targetDir);
		float dir = Vector3.Dot(perp, _up);

		if (dir > 0) {
			return 1;} 
		else if (dir < 0) {
			return -1;} 
		else {
			return 0F;}
	}

// --> Use to Init the Player Input
	public void InitPlayerButton(				
		string 	_Input_Left,
		string 	_Input_Right,
		string 	_Input_Accelerator,
		string 	_Input_Break,
		string 	_Respawn){

		Input_Left			= _Input_Left;
		Input_Right			= _Input_Right;
		Input_Accelerator	= _Input_Accelerator;
		Input_Break			= _Input_Break;
		Respawn				= "";
	}

// --> Use to respawn the car
	public void RespawnTheCar(){					
		if (!b_CarIsRespawning) {
			b_CarIsRespawning = true;
			StopCoroutine ("RepawnInitialization");
			StartCoroutine ("RepawnInitialization");
		}
	}

// --> use to init car after respawn
	IEnumerator RepawnInitialization(){
		if (b_AutoAcceleration) {
			carAI.StopCo ();
			carAI.b_endBackward = false;
			carAI.CarMoveForward = true;
		}


		Transform cloud = Instantiate(RespawnCloud.transform);
		cloud.transform.position = gameObject.transform.position;

		t = 0;
		float timeToMove = 1;
		DeactivateCar();
		Vector3 TargetPos = transform.position;
		Vector3 TargetEuler = transform.eulerAngles;

		Vector3 targetPos = FindClosestCheckpoint().transform.position;
		Quaternion targetQuaternion = FindClosestCheckpoint().transform.rotation;

		while(t < timeToMove){																// Move the car to the closest Checkpoint
			if(!b_Pause){
				t = Mathf.MoveTowards(t,timeToMove,Time.deltaTime/timeToMove);
			}
			yield return null;
		}

		timeToMove = 1;
		t = 0;

		while(t < timeToMove){																// Move the car to the closest Checkpoint
			if(!b_Pause){
				t = Mathf.MoveTowards(t,timeToMove,Time.deltaTime/timeToMove);

				TargetPos = Vector3.Lerp(TargetPos, targetPos + new Vector3(0,.5F,0),1f - Mathf.Cos(t * Mathf.PI * 0.5f));
				transform.position = TargetPos;

				transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, 1f - Mathf.Cos(t * Mathf.PI * 0.5f));

			}
			yield return null;
		}


		Destroy(cloud.gameObject);

		ActivateCar();
		timeToMove = 1;
		t = 0;

		if (b_AutoAcceleration) {																	// Case : CPU
			b_CarIsRespawning = false;
		} else {

			while (t < timeToMove) {																// Case : Player : Wait 1 second to prevent bug if player press respawn to quickly								
				if (!b_Pause) {		
					t = Mathf.MoveTowards (t, timeToMove, Time.deltaTime / timeToMove);
				}
				yield return null;
			}

			b_CarIsRespawning = false;
		}

	}

	public void DeactivateCar(){
		rb.isKinematic = true;

		for(int i = 0;i<RayCastWheels.Length;i++){
			RayCastWheels[i].gameObject.SetActive(false);
		}
			
		Capsule_Rear.SetActive(false);										// use when car is deactivate or activated
		Capsule_Front.SetActive(false);										// use when car is deactivate or activated
		BodyCollider.SetActive(false);										// use when car is deactivate or activated
		ColliderCarOnBack.GetComponent<BoxCollider>().enabled = false;	

		Grp_BodyPlusBlobShadow.SetActive(false);

	}

	public void ActivateCar(){
		StartCoroutine("I_ActivateCar");
	}
		
	IEnumerator I_ActivateCar(){
		bool collisionCheck = true;											

		while (collisionCheck) {											// Check if the car is close to an other. Wait to respawn the car if the car is too close from an another car
															
			if (!b_Pause) {
				int cmpt = 0;
				Collider[] col = Physics.OverlapBox (transform.position, new Vector3 (.1F, 1, 1.5F), transform.rotation);

				for (int i = 0; i < col.Length; i++) {
					if (col [i].gameObject.tag == "Car" && col [i].gameObject != gameObject)
						cmpt++;			// A car is near.  
				}
				if (cmpt == 0)
					collisionCheck = false;
			}
			yield return null;
		}

		rb.isKinematic = false;												// Activate the car
		for(int i = 0;i<RayCastWheels.Length;i++){
			RayCastWheels[i].gameObject.SetActive(true);
		}

		Capsule_Rear.SetActive(true);										// use when car is deactivate or activated
		Capsule_Front.SetActive(true);										// use when car is deactivate or activated
		BodyCollider.SetActive(true);										// use when car is deactivate or activated
		ColliderCarOnBack.GetComponent<BoxCollider>().enabled = true;										// use when car is deactivate or activated

		Grp_BodyPlusBlobShadow.SetActive(true);
	}

	GameObject FindClosestCheckpoint()
	{
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag("Checkpoint");

		if (gos.Length > 0) {

			GameObject closest = null;
			float distance = Mathf.Infinity;
			Vector3 position = carPathFollow.target.transform.position;

			foreach (GameObject go in gos) {
				Vector3 diff = go.transform.position - position;
				float curDistance = diff.sqrMagnitude;
				if (curDistance < distance) {
					closest = go;
					distance = curDistance;
				}
			}
			return closest;
		} else {
			return null;
		}
	}



// --> Car Pause State
	public void Pause(){
		if (b_Pause) {									// -> Stop Pause
			b_Pause = false;

			audio_.UnPause();								// Pause audio
			objSkid_Sound.UnPause();
			obj_CarImpact_Sound.UnPause();

			if (!b_CarIsRespawning) {						// Pause Physics
				rb.isKinematic = false;
				rb.velocity = tmpVelocity;
			}
		} 
		else {											// -> Start Pause
			tmpVelocity = rb.velocity;						// Unpause Physics
			rb.isKinematic = true;

			audio_.Pause();									// Pause audio
			objSkid_Sound.Pause();
			obj_CarImpact_Sound.Pause();

			b_Pause = true;
		}
	}

	public float _localVelovity(){
		Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
		return localVelocity.z;
	}
		


	IEnumerator I_AllowRandomCarValue(){
		//float tmpTimer = 0;
		b_allowRandomCarValue = false;											
		if (carAI.enabled)
			carAI.F_RandomCarValues ();
		t = 0;
		float timeToMove = 1;
		while(t < 1){																
			if(!b_Pause){
				t = Mathf.MoveTowards(t,1,Time.deltaTime/timeToMove);
			}
			yield return null;
		}
		b_allowRandomCarValue = true;
	}


	void OnTriggerEnter(Collider other){
		if(other.tag == "RespawnZone"){
			RespawnTheCar();
		}
			
	}
		
	void OnCollisionEnter(Collision collision)
	{

		// Play a sound if the colliding objects had a big impact.
		float tmpVolume = 0;
		if (collision.gameObject.tag == "Car") {
			
			if (collision.relativeVelocity.magnitude < 5)
				tmpVolume = impactVolumeMax * collision.relativeVelocity.magnitude * .2F;
			else
				tmpVolume = impactVolumeMax;
			obj_CarImpact_Sound.volume = tmpVolume;


			if (!obj_CarImpact_Sound.isPlaying)
				obj_CarImpact_Sound.Play ();

		} else {
			if (collision.relativeVelocity.magnitude < 5) {
				tmpVolume = impactVolumeMax * collision.relativeVelocity.magnitude * .1F;

				obj_CarImpact_Sound.volume = tmpVolume;

				if (!obj_CarImpact_Sound.isPlaying)
					obj_CarImpact_Sound.Play ();
			}
		}
	}
}

