// Description : LapCounter.cs : Use to calculate and display on screen laps and position for each car. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 																								//This allows the IComparable Interface
using UnityEngine.UI;


public class LapCounter : MonoBehaviour {

	public Game_Manager 										gameManager;								// Access the Game_Mananger component
	public bool 												SeeInspector = false;						// use on custom editor to see all the variables
	public bool													b_Pause = false;							// use for pause Mode
	public Color GizmoColor = new Color(1,.92f,.016f,.5f);													// Gizmo color for the la counter gameObject

	public bool													b_ActivateLapCounter = true;				// if true : the Lap Counter is activated
	public int													lapNumber = 3;								// know the number of lap in a race

	public List<CarPathFollow> 									car = new List<CarPathFollow> ();			// access CarPathFollow componentn for each car
	public List<CarController> 									carController = new List<CarController> ();	// access CarController component for each car
	public List<float> 											carProgressDistance = new List<float> ();	// know the progression of each car during the race 
	public List<int> 											carLap = new List<int> ();					// know the number of lap done for each car
	public List<float> 											carTime = new List<float> ();				// know the time in race for each car
	public List<bool> 											raceFinished = new List<bool> ();			// Know if the race is finisg-hed for each car
	//public List<int> 											carPositionAtEnd = new List<int> ();		// Know if a car is on 1 2 3 4 position in a race when race is finished
	public List<int> 											carPosition = new List<int> ();				// Know if a car is on 1 2 3 4 position in a race 
	public float 												Timer = 0;									// global race timer
	public bool 												startTimer = false;							// know if the timer starts


	private UnityStandardAssets.Utility.WaypointCircuit 		Track; 										// A reference to the waypoint-based route we should follow

	private float												trackLengthReference = 0;					// Save the track length distance.
	public Text 												Txt_P1;										// Display Player 1 position on race
	public Text 												Txt_P2;										// Display Player 2 position on race
	public Text 												Txt_Timer;									// Display global timer
	public Text 												Txt_P1_Lap;									// Display the number of lap for player 1
	public Text 												Txt_P2_Lap;									// Display the number of lap for Player 2
	public float												RefreshPosTime_ = .2f;
	private float												RefreshPositionTimer = 0;

	private bool 												player2IsManageByCPU = true;				// Know if the player 2 is Manage by the CPU.
	private bool 												initDone = false;							// Know if the Initialialization is done
	public InventoryCar 										inventoryItemCar;							// Use to know if LapCOunter is activate for the Race.
											

// --> Initilization
	void Start(){
		if (inventoryItemCar)
			b_ActivateLapCounter = inventoryItemCar.b_LapCounter;


		GameObject tmpObj = GameObject.Find ("Track_Path");
		if (tmpObj) {
			Track = tmpObj.GetComponent<UnityStandardAssets.Utility.WaypointCircuit> ();					// access the track (car path)

		}

		tmpObj = GameObject.Find ("Game_Manager");
		if (tmpObj) {
			gameManager = tmpObj.GetComponent<Game_Manager> ();												// access the Game_Manager

		}

		//if (PlayerPrefs.GetInt ("TestMode") == 1) {															// Init if Test Mode Activated
		if (gameManager.inventoryItemList.inventoryItem[0].b_TestMode == true) {								// Init if Test Mode Activated

			InitCar ();
		}

		if (car [0] != null && Txt_P1_Lap) {
			Txt_P1_Lap.text = "Lap " + (car [0].iLapCounter).ToString () + "/" + lapNumber.ToString () ;
		}
		if (car [1] != null && Txt_P2_Lap) {
			Txt_P2_Lap.text = "Lap " + (car [1].iLapCounter).ToString () + "/" + lapNumber.ToString ();
		} 
	}

// --> Init list car and list carController. Call by the Game_Manager or at the beginning of this script if ''Test Mode'' is activated
	public void InitCar(){
		// --> Find car on scene and init Car list carController. 
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag ("Car");							

		foreach (GameObject carFind in arrCars) {
			if (carFind.GetComponent<CarController> ()) {
				car [carFind.GetComponent<CarController> ().playerNumber - 1] = carFind.GetComponent<CarPathFollow> ();					// access component for each car
				carController [carFind.GetComponent<CarController> ().playerNumber - 1] = carFind.GetComponent<CarController> ();		// access component for each car


				if (carFind.GetComponent<CarController> ().playerNumber == 2 && carFind.GetComponent<CarAI> ().enabled == false)		// Know if player 2 is manage by a human Player or CPU
					player2IsManageByCPU = false;
			}
		}
		initDone = true;																												// Init done
	}

// --> Update function
	void Update(){
		if (!b_Pause && initDone) {
			
			for (var i = 0; i < car.Count; i++) {
				if (car [i] != null) {
					carLap [i] = car [i].iLapCounter;																		// number of lap for each car
					carProgressDistance [i] = car [i].progressDistance;														// car progression for each car
				}
			}
			if (Track != null && trackLengthReference == 0)																	// if track
				trackLengthReference = Track.Length;																		// save the track length
			
			positionOnRace ();																								// know the position for each car on race

			if(startTimer)																									// if the race is started
				F_Timer ();																									// Display Timer on screen 
		}
	}

// --> display timer on screen
	void F_Timer (){
		Timer += Time.deltaTime;
		string minutes = "";
		if(Mathf.Floor(Timer / 60) > 0 && Mathf.Floor(Timer / 60) < 10)
			minutes = Mathf.Floor(Timer / 60).ToString("0");
		
		if(Mathf.Floor(Timer / 60) >  10)
			minutes = Mathf.Floor(Timer / 60).ToString("00");
		
		string seconds = Mathf.Floor(Timer % 60).ToString("00");
		string milliseconds = Mathf.Floor((Timer*1000) % 1000).ToString("000");


		if (Txt_Timer){
			if(Mathf.Floor(Timer / 60) == 0)
				Txt_Timer.text = seconds + ":" + milliseconds;
			else
				Txt_Timer.text = minutes + ":" + seconds + ":" + milliseconds;
		}
	}




// --> Display car position on screen
	void positionOnRace (){
		RefreshPositionTimer = Mathf.MoveTowards (RefreshPositionTimer, RefreshPosTime_, Time.deltaTime);



		List<progressionCompare> playersPositions = new List<progressionCompare>();								// Create a list 


		for(int i = 0; i < car.Count; i++){																		// Create the list with name and scores

			float tmpProg = ((carLap [i] * trackLengthReference) + carProgressDistance [i])*1000;

			playersPositions.Add (new progressionCompare(car[i], Mathf.RoundToInt(tmpProg)));
		}

		playersPositions.Sort();																				// sort the list
		playersPositions.Reverse();																				// reverse list	


		for(int i = 0; i < playersPositions.Count; i++){														// Create the list with name and scores

			if (raceFinished [0] == false) {
				if (playersPositions [i].car == car [0]															// Display on screen Player 1 position
				   && Txt_P1 && Timer == 0) {
						Txt_P1.text = 4 + "/" + car.Count;
				} else if (playersPositions [i].car == car [0] && Txt_P1) {
					if (RefreshPositionTimer == RefreshPosTime_) {
						Txt_P1.text = (i + 1) + "/" + car.Count;
					}
					carPosition [0] = i + 1;
				}
			}

			if (raceFinished [1] == false) {
				if (playersPositions [i].car == car [1]															// Display on Screen player 2 position
				   && Txt_P2
				   && Timer == 0) {
						Txt_P2.text = 3 + "/" + car.Count;
				} else if (playersPositions [i].car == car [1] && Txt_P2) {
					if (RefreshPositionTimer == RefreshPosTime_) {
						Txt_P2.text = (i + 1) + "/" + car.Count;
					}

					carPosition [1] = i + 1;
				}
			}

			if (raceFinished [2] == false) {
				if (playersPositions [i].car == car [2])
					carPosition [2] = i + 1;
			}
			if (raceFinished [3] == false) {
				if (playersPositions [i].car == car [3])
					carPosition [3] = i + 1;
			}


			/*if (playersPositions [i].car == car [0])carPosition [i] = 1;
			if (playersPositions [i].car == car [1])carPosition [i] = 2;
			if (playersPositions [i].car == car [2])carPosition [i] = 3;
			if (playersPositions [i].car == car [3])carPosition [i] = 4;*/
		}
		if(RefreshPositionTimer == RefreshPosTime_)
			RefreshPositionTimer = 0;


	}

// --> Pause
	public void Pause(){
		if (b_Pause) {									// -> Stop Pause
			b_Pause = false;
		} 
		else {											// -> Start Pause
			b_Pause = true;
		}
	}


	void OnTriggerEnter(Collider other) {
		CarPathFollow carPathFollow = other.GetComponent<CarPathFollow> ();
		if (other.tag == "Car" && carPathFollow.iLapCounter < lapNumber+1) {													// A car finish a lap
			if(b_ActivateLapCounter){
				if (carPathFollow.iLapCounter <= lapNumber+1) {
					if (car [0] != null && car [0].name == other.name && Txt_P1_Lap) {
						Txt_P1_Lap.text = "Lap " + (car [0].iLapCounter).ToString () + "/" + lapNumber.ToString ();
				}
					if (car [1] != null && car [1].name == other.name && Txt_P2_Lap) {
						Txt_P2_Lap.text = "Lap " + (car [1].iLapCounter).ToString () + "/" + lapNumber.ToString ();
				}
			}
		}
		}

		if (b_ActivateLapCounter) {
			if (other.tag == "Car" && carPathFollow.iLapCounter >= lapNumber+1) {												// A car finish the race
				//Debug.Log (other.name + " finished the race");
				for (var i = 0; i < car.Count; i++) {
					if (car [i] != null && car [i].name == other.name && !raceFinished [i]) {
						raceFinished [i] = true;
						carTime [i] = Timer;
						carController [i].raceIsFinished = true;							
	

						if (i == 0 && player2IsManageByCPU) {
							PlayerPrefs.SetInt ("CurrentScore", Mathf.RoundToInt(carTime [i]*1000));
							gameManager.RaceIsFinished ();
						} else if ((i == 0 || i == 1)  && !player2IsManageByCPU && carController [0] != null && carController [0].raceIsFinished && carController [1] != null  && carController [1].raceIsFinished) {
							gameManager.RaceIsFinished ();
						}
					}
				}
			}
		}
	}

	public void displayLap(CarPathFollow carAddLap){

	}

	void OnDrawGizmos()
	{

		Gizmos.color = GizmoColor;																					// Create a line between the car position and the target position

		Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
		Gizmos.matrix = cubeTransform;

		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}


// --> Compare car position 1st, 2nd, 3, or 4
	public class progressionCompare : IComparable<progressionCompare>
	{
		public CarPathFollow car;
		public int total;

		public progressionCompare (CarPathFollow newCar ,int newtotal)
		{
			car = newCar;
			total = newtotal;
		}

		//This method is required by the IComparable
		//interface. 
		public int CompareTo(progressionCompare other)
		{
			if(other == null)
			{
				return 1;
			}

			//Return the difference in power.
			return total - other.total;
		}
	}



}

