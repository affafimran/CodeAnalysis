// Description : CarAI.cs : Manage car AI driving
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour {

	public bool 					SeeInspector = false;
	public  LayerMask 				myLayerMask;											// Ignore specific Layer when physic raycasting is used	
	public	bool					b_Pause 							= false;			// use game is paused
	public 	GameObject 				Target;													// Target followed by the car AI
	public 	CarController			carController;											// access component
	public 	int		 				CarAIEulerRotation 					= 70;				// car rotation speed
	public 	string					Obstacles 							= "Null";			// know if there is an obstacle behind the car

	public 	float 					smoothAvoid 						= 0;				// value between 0 and 1
	public 	bool					b_DisableRotation  					= false;			// bool use allow car rotation
	private	bool 					CheckCarVelocity 					= false;			// use when the car need to go backward to avoid an obstacle

	public 	bool					CarMoveForward 						= true;				// know if the car move forward
	public 	bool 					b_endBackward 						= false;			// know if the car is moving backward
	public 	float 					carWaitBeforeBackwardDuration 		= 2;				// use when the car need to go backward to avoid an obstacle
	public 	float 					carBackwardDuration 				= 1;				// use when the car need to go backward to avoid an obstacle
	public	int						HowManyTimeCarCantMove 				= 0;				// If HowManyTimeCarCantMove = 3 car respawn. Prevents that car is block by something.

	public 	GameObject 				obj_pivotCheckCarCollisionRearRight;					// raycast pivot to check if there is a car collision on his right side
	public 	GameObject 				obj_pivotCheckCarCollisionFrontRight;					// raycast pivot to check if there is a car collision on his right side

	public bool						IsTouchingCar_OnHisRight 			= false;			// Know if this car is on collision with car on his right
	public float					WaitBeforeTakingDecision 			= 0;				// use in the decision process

	public GameObject 				SecondCarOnRight;										// Know the car on the right

	public bool						allowRandomValues 					= true;				// allow random value 
	public int						successRate_BestTargetPos			= 100;				// if value = 100 the car take all the taime the good decision
	public float 					TargetPosMin 						= -.15f;			// offset on the left
	public float 					TargetPosMax 						= .15f;				// offset on the right
	public int						successRate_BestOffsetSpeed			= 100;				// if value = 100 the car take all the taime the good decision			
	public float 					offsetSpeedMin 						= -.15f;			// offset min
	private float 					offsetSpeedMax 						= 0;				// offset max
	public int						successRate_BestOffsetRandomEulerAngle	= 100;			// if value = 100 the car take all the taime the good decision	
	public float 					offsetRandomEulerAngleMin 			= -10;				// offset min
	private float 					offsetRandomEulerAngleMax 			= 0;				// offset max
	public float 					offsetRandomEulerAngle 				= 0;				// offset result

	public float 					angle 								= 0; 				// Find angle between car the target path
	public bool 					b_contactWithOtherCar 				= false;			
	private float 					impact 								= 0;				// collision.relativeVelocity.magnitude


	private DifficultyManager 		difficultyManager;

	public float 					ObstacleDistance 	= 0;	
	private float 					raycastLength = 6F;
	public AnimationCurve 			curveOvaidObstacle;
	//public Vector3 RaycastImpact;
	public float angleObstacle = 0;


// --> Use this for initialization
	void Start () {
		


		carController = GetComponent<CarController>();										// Access CarController component

		GameObject tmpObj = GameObject.Find ("P" + transform.GetComponent<CarController> ().playerNumber + "_Target_Part2");
		Target = tmpObj;

		initRandomValue ();
	}

// --> Fixed Update function
	void FixedUpdate () {
		if(!carController.b_CarIsRespawning && !carController.b_CountdownActivate){	// --> car follow the path
			CheckObstacles();													// Check obstacles
			TurnLeftAndRight();													// Turn car to the left or to the right if needed
			Acceleration();														// check acceleration
			CheckCarLocalVelocity();											// check car velocity
		}
		else{																// --> car is respawing
			if(carController.b_btn_Acce || carController.b_btn_Break)			// 
				carController.Btn_AccelerationDeactivate();						// stop accelerartion or break
		}
	}



// -- > Check Obstacle
	public void CheckObstacles(){	
		Obstacles = "Null";	
		RaycastHit hit;


		Vector3 PosCenter = carController.frontCapsuleCollider.gameObject.transform.position;
		Vector3 DirCenter = carController.frontCapsuleCollider.gameObject.transform.forward;

		Vector3 DirAngleLeft = 	-carController.frontCapsuleCollider.gameObject.transform.forward
			+ carController.frontCapsuleCollider.gameObject.transform.right*.40F;

		Vector3 DirAngleRight = -carController.frontCapsuleCollider.gameObject.transform.forward
			- carController.frontCapsuleCollider.gameObject.transform.right*.40F;

		Vector3 PosRight = carController.RayCastWheels[0].gameObject.transform.position;
		Vector3 PosLeft = carController.RayCastWheels[1].gameObject.transform.position;


// --> Next Four raycasts are use to check obstacle collision
		if(Physics.Raycast(PosCenter,DirCenter, out hit, raycastLength,myLayerMask)
			){					// Center
			//Debug.DrawRay(hit.point,  hit.normal, Color.cyan);

			angleObstacle = Vector2.SignedAngle(new Vector2(hit.normal.x,hit.normal.z), 												// Find angle between car the target path
				new Vector2(DirCenter.x,DirCenter.z));			

			if(angleObstacle < 0){
				Obstacles = "Left";
			}
			else{
				Obstacles = "Right";
			}
			ObstacleDistance = hit.distance;
		}
		else if(Physics.Raycast(PosLeft,DirCenter, out hit, raycastLength*.5f,myLayerMask)){					// Left
			Obstacles = "Left";
			ObstacleDistance = hit.distance;
		}
		else if(Physics.Raycast(PosRight,DirCenter, out hit, raycastLength*.5f,myLayerMask)){				// Right
			Obstacles = "Right";
			ObstacleDistance = hit.distance;
		}


		if(Physics.Raycast(PosRight,-DirAngleRight, out hit, raycastLength*.7f,myLayerMask)){		// Right angle
			Obstacles = "Right";
			ObstacleDistance = hit.distance;
			//Debug.Log("Right Angle");
		}

		if(Physics.Raycast(PosLeft,-DirAngleLeft, out hit, raycastLength*.7f,myLayerMask)){			// Left angle
			//Debug.Log("Left Angle");
			Obstacles = "Left";
			ObstacleDistance = hit.distance;
		}



		Debug.DrawLine(PosCenter,PosCenter + DirCenter* raycastLength,Color.green);	
		Debug.DrawLine(PosLeft,PosLeft + DirCenter * raycastLength*.5f,Color.green);	
		Debug.DrawLine(PosRight,PosRight + DirCenter * raycastLength*.5f,Color.green);	

		Debug.DrawLine(PosLeft,PosLeft - DirAngleLeft * raycastLength*.7f,Color.green);											// Left Angle
		Debug.DrawLine(PosRight,PosRight - DirAngleRight * raycastLength*.7f,Color.green);
		//Debug.Log (ObstacleDistance);

// --> Next raycasts are use to check car collision

		Vector3 CarDir01 = 	-obj_pivotCheckCarCollisionFrontRight.transform.forward
			+ obj_pivotCheckCarCollisionFrontRight.gameObject.transform.right*.2F;

		Vector3 CarDir02 = -obj_pivotCheckCarCollisionRearRight.transform.forward
			- obj_pivotCheckCarCollisionRearRight.transform.right*.2F;

		if (Physics.Raycast (obj_pivotCheckCarCollisionFrontRight.transform.position, CarDir01, out hit, 3 * .15f) && hit.collider.tag == "Car"
		   && Physics.Raycast (obj_pivotCheckCarCollisionRearRight.transform.position, -CarDir02, out hit, 3 * .15f) && hit.collider.tag == "Car") {					// Check raycast with gameObject "CheckOtherCarCollision"
			IsTouchingCar_OnHisRight = true;
			if (WaitBeforeTakingDecision == 0) {
				SecondCarOnRight = hit.collider.gameObject;
				StartCoroutine ("Take_Decision_Because_Car_Touch_Other_Car_On_His_Right");
			}
		} else if(IsTouchingCar_OnHisRight){
			IsTouchingCar_OnHisRight = false;
		}
		Debug.DrawLine(obj_pivotCheckCarCollisionFrontRight.transform.position,obj_pivotCheckCarCollisionFrontRight.transform.position + CarDir01 * raycastLength*.15f,Color.yellow);			// 
		Debug.DrawLine(obj_pivotCheckCarCollisionRearRight.transform.position,obj_pivotCheckCarCollisionRearRight.transform.position - CarDir02 * raycastLength*.15f,Color.yellow);			// 
}

// -- > Turn car to the left or to the right if needed
	public void TurnLeftAndRight(){											

		Vector3 PosCenter = carController.frontCapsuleCollider.gameObject.transform.position;

		Vector3 dir = Target.transform.position - PosCenter;									// Angle between car and target

		angle = Vector2.Angle(new Vector2(dir.x,dir.z), 												// Find angle between car the target path
			new Vector2(transform.forward.x,transform.forward.z));			
		float angle2 = AngleDir(transform.forward,dir,transform.up);									// Find if car is on the left or on the right of the target path


		//Debug.DrawLine(PosCenter,PosCenter + dir,Color.yellow);	
		float percentageAIRotation = (angle - 0) / (15F - 0);
		//Debug.Log ("angle : " + angle);
		if (!b_contactWithOtherCar) {

			if (!b_DisableRotation) {
				if (Obstacles == "Null") {
					if (angle2 == 1 && Mathf.Abs (angle) > 2) {											// Check if the car need to turn left or right
						carController.Btn_RightActivate ();
						carController.Btn_LeftDeactivate ();
						//Debug.Log ("Turn_Right");
					} else if (angle2 == -1 && Mathf.Abs (angle) > 2) {
						carController.Btn_RightDeactivate ();
						carController.Btn_LeftActivate ();
						//Debug.Log ("Turn_Left");
					} else {
						carController.Btn_RightDeactivate ();
						carController.Btn_LeftDeactivate ();
						//Debug.Log ("Go_Forward");
					}
				} else if (Obstacles == "Left") {
					//Debug.Log("Obstacle Middle");
					carController.Btn_RightActivate ();
					carController.Btn_LeftDeactivate ();//}
				} else if (Obstacles == "Right") {
					//Debug.Log("Obstacle Middle");
					carController.Btn_LeftActivate ();
					carController.Btn_RightDeactivate ();//}
				}
			} else {																					// Car go backward because she can't move
				if (Obstacles == "Right") {
					carController.Btn_RightActivate ();
					carController.Btn_LeftDeactivate ();
				} else {
					carController.Btn_RightDeactivate ();
					carController.Btn_LeftActivate ();
				}

				percentageAIRotation = (30 - 0) / (15F - 0);
			}



			if (Obstacles == "Null") {
				smoothAvoid = Mathf.MoveTowards (smoothAvoid, 1, Time.deltaTime * .5f);
				carController.eulerAngleVelocity.y 														// Car turn left and right
				= (CarAIEulerRotation + offsetRandomEulerAngle) * percentageAIRotation *smoothAvoid;	

			} else {
				smoothAvoid = 0;


				carController.eulerAngleVelocity.y 														// Car turn left and right
				= (CarAIEulerRotation + offsetRandomEulerAngle)/* * percentageAIRotation */* 9f * curveOvaidObstacle.Evaluate(1 - (ObstacleDistance*1)/raycastLength);	
			}
		}
	}


// --> Find if car is on the left or on the right of the target path
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

	public void Acceleration(){
		if (!b_Pause && !b_contactWithOtherCar) {
			if (CarMoveForward) {
				b_DisableRotation = false;
				carController.b_btn_Acce = true;
				carController.b_btn_Break = false;
			} else {
				b_DisableRotation = true;
				carController.b_btn_Acce = false;
				carController.b_btn_Break = true;
			}
		}
	}


	public void CheckCarLocalVelocity(){
		if (!b_Pause && !carController.raceIsFinished) {
			if (carController._localVelovity () < .2 && !CheckCarVelocity && !b_endBackward && !carController.b_CarIsRespawning) {
				StartCoroutine ("WaitBeforeGoBackward");
			} else if (carController._localVelovity () >= Mathf.Abs(.2f) && CheckCarVelocity) {
				StopCoroutine ("WaitBeforeGoBackward");
				CarMoveForward = true;
				b_DisableRotation = false;
				b_endBackward = false;
			}
		}
	}

// --> Init Random value using the selected difficulty
	public void initRandomValue (){
		GameObject tmpObj = GameObject.Find ("Game_Manager");
		if (tmpObj)
			difficultyManager = tmpObj.GetComponent<DifficultyManager> ();

		if(difficultyManager !=  null && carController != null && carController.playerNumber != 1){
			carController.offsetSpeedDifficultyManager	= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].addGlobalSpeedOffset[carController.playerNumber - 2];
			successRate_BestTargetPos 					= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].waypointSuccesRate[carController.playerNumber - 2];
			TargetPosMin 								= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].waypointMinTarget[carController.playerNumber - 2];
			TargetPosMax								= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].waypointMaxTarget[carController.playerNumber - 2];
			successRate_BestOffsetSpeed					= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].speedSuccesRate[carController.playerNumber - 2];
			offsetSpeedMin								= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].speedOffset[carController.playerNumber - 2];
			successRate_BestOffsetRandomEulerAngle		= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].rotationSuccesRate[carController.playerNumber - 2];
			offsetRandomEulerAngleMin					= difficultyManager.difficulties[PlayerPrefs.GetInt ("DifficultyChoise")].rotationOffset[carController.playerNumber - 2];

			F_RandomCarValues ();
		}
	}

// --> creation variations in car reactions
	public void F_RandomCarValues(){
		if (allowRandomValues) {
			float randomValue = Random.Range (0, 101);

			//Debug.Log (randomValue);
			if (successRate_BestTargetPos < randomValue) {													// if random superior to siccess rate : Random Target local Position
				randomValue = Random.Range (TargetPosMin, TargetPosMax);									
				Target.transform.localPosition = new Vector3 (randomValue, 0, 0);
				//Debug.Log ("New Target not Succeed");
			} else {
				Target.transform.localPosition = new Vector3 (0, 0, 0);
				//Debug.Log ("Best Target Succeed");
			}

			randomValue = Random.Range (0, 101);

			if (successRate_BestOffsetSpeed < randomValue) {												// if random superior to siccess rate : Random Target local Position
				randomValue = Random.Range (offsetSpeedMin, offsetSpeedMax);									
				carController.randomSpeedOffset = randomValue;
			} else {
				carController.randomSpeedOffset = 0;
			}

			randomValue = Random.Range (0, 101);
		
			if (successRate_BestOffsetRandomEulerAngle < randomValue) {									// if random superior to siccess rate : Random euler angle rotation
				randomValue = Random.Range (offsetRandomEulerAngleMin, offsetRandomEulerAngleMax);			
				offsetRandomEulerAngle = randomValue;
			} else {
				offsetRandomEulerAngle = 0;
			}
		}
	}

	IEnumerator Take_Decision_Because_Car_Touch_Other_Car_On_His_Right(){

		WaitBeforeTakingDecision = 0;
		while(WaitBeforeTakingDecision < 1){					
			if(!b_Pause){
				WaitBeforeTakingDecision = Mathf.MoveTowards(WaitBeforeTakingDecision,1,Time.deltaTime);
			}
			yield return null;
		}

		if (allowRandomValues) {																				// add offset on targets that follow the cars
			Target.transform.localPosition = new Vector3 (-0.15f, 0, 0);
			SecondCarOnRight.GetComponent<CheckOtherCarCollision> ().ChangeCarTargetPosition (.15f);
		}

		float refForce = carController.Force;
		while(WaitBeforeTakingDecision < 4){					
			if(!b_Pause){
				WaitBeforeTakingDecision = Mathf.MoveTowards(WaitBeforeTakingDecision,4,Time.deltaTime);
				carController.Force = Mathf.MoveTowards (carController.Force, refForce - 80, Time.deltaTime*80);
			}
			yield return null;
		}
		carController.Force = refForce;
		if (allowRandomValues) {
			Target.transform.localPosition = new Vector3 (0, 0, 0);
			SecondCarOnRight.GetComponent<CheckOtherCarCollision> ().ChangeCarTargetPosition (0);
		}
		WaitBeforeTakingDecision = 0;
	}

	public void StopCo(){
		
		StopAllCoroutines();
	}

	IEnumerator WaitBeforeGoBackward(){
		float tmpTimer = 0;
		b_endBackward = true;
		while(tmpTimer < carWaitBeforeBackwardDuration){					
			if(!b_Pause){
				tmpTimer = Mathf.MoveTowards(tmpTimer,carWaitBeforeBackwardDuration,Time.deltaTime);
			}
			yield return null;
		}
		if(HowManyTimeCarCantMove == 3){
			Debug.Log("Car need to be respawn");
			carController.RespawnTheCar();
			HowManyTimeCarCantMove = 0;
			CarMoveForward = true;
			b_DisableRotation = false;
			b_endBackward = false;
		}
		else{
			if(carController._localVelovity() >= Mathf.Abs(.2f)){
				HowManyTimeCarCantMove = 0;
			}
			else{
				Debug.Log("Car need to go Backward");
				HowManyTimeCarCantMove++;
				tmpTimer = 0;
				CarMoveForward = false;
				while(tmpTimer < carBackwardDuration){					
					if(!b_Pause)tmpTimer = Mathf.MoveTowards(tmpTimer,carBackwardDuration,Time.deltaTime);
					yield return null;
				}
			}
			CarMoveForward = true;
			b_DisableRotation = false;
			b_endBackward = false;
		}

	}

	public void Pause(){
		if (b_Pause) {									// -> Stop Pause
			b_Pause = false;
		} 
		else {											// -> Start Pause
			b_Pause = true;
		}
	}
		
	void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > 1.5f 
			&& collision.gameObject.tag == "Car" 
			&& !b_contactWithOtherCar 
			&& !collision.gameObject.GetComponent<CarAI>().isActiveAndEnabled					// car is controller by player not a CPU
		) {
			
			b_contactWithOtherCar = true;
			carController.b_btn_Acce = false;
			carController.b_btn_Break = false;

			impact = collision.relativeVelocity.magnitude;

			StartCoroutine ("WaitAfterContact");
		}
	}


	IEnumerator WaitAfterContact(){
		float tmpTimer = 0;
		if (impact > 5) {
			impact = 1f;
		} else {
			impact = .5f;
		}
		while(tmpTimer < impact){					
			if(!b_Pause){
				tmpTimer = Mathf.MoveTowards(tmpTimer,1,Time.deltaTime);
			}
			yield return null;
		}
		b_contactWithOtherCar = false;
	}
}
