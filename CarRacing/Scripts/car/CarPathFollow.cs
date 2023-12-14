//Description : CarPathFollow.cs : Allow car AI to follow a path. ||  Use to know the position of each car on race. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarPathFollow : MonoBehaviour {

	public Transform 												target;
	public float 													progressDistance = 0; 					// The progress round the route, used in smooth mode.

	public 	UnityStandardAssets.Utility.WaypointCircuit 			Track; 									// A reference to the waypoint-based route we should follow
	public 	UnityStandardAssets.Utility.WaypointCircuit.RoutePoint 	progressPoint { get; private set; }		// access variable from WaypointCircuit standard asset

	public	int 													iLapCounter = 1;						// know the number of lap done by a car
	private bool													pathExist = false;						// check if there are checkpoints on track path

	private LapCounter												sLapCounter;							// accessLapCounterscript component
	private CarPathFollow											carPathFollow;


	void Start()
	{
		GameObject tmpObj = GameObject.Find ("P" + transform.GetComponent<CarController> ().playerNumber + "_Target");	// you need to have a gameObject named "P1_Target" "P2_Target" "P3_Target" "P4_Target" on your scene
		if(tmpObj)target = tmpObj.transform;																			// access the target that follow the car																

		tmpObj = GameObject.Find ("Track_Path");
		if(tmpObj)Track = tmpObj.GetComponent<UnityStandardAssets.Utility.WaypointCircuit>();							// access the track (car path)

		if(Track != null && Track.waypointList.items.Length >0)
			pathExist = true;

		tmpObj = GameObject.Find ("StartLine_lapCounter");
		if(tmpObj)sLapCounter = tmpObj.GetComponent<LapCounter> ();														// access the track (car path)

		carPathFollow = GetComponent<CarPathFollow> ();

	}
		

	void Update()
	{
		if (Track != null && target != null && pathExist) {

			target.position = Track.GetRoutePoint (progressDistance + 1.45f).position;									// find the next position for the target	
			target.rotation = Quaternion.LookRotation (Track.GetRoutePoint (progressDistance).direction);				// find the new rotation for the target


			progressPoint = Track.GetRoutePoint (progressDistance);														// --> Get the progressPoint position
			Vector3 progressDelta = progressPoint.position - transform.position;
			if (Vector3.Dot (progressDelta, progressPoint.direction) < 0) {												// if progress point position is behind the car
				progressDistance += progressDelta.magnitude * 0.5f;														// change the progress point position
			}
		}

		if(Track != null && progressDistance/Track.Length > 1){
			//Debug.Log ("Lap");
			iLapCounter++;
			if(sLapCounter)sLapCounter.displayLap (carPathFollow);
			progressDistance = progressDistance % Track.Length;
		}
	}


	void OnDrawGizmos()
	{
		if (Application.isPlaying && Track != null && target != null)
		{
			Gizmos.color = Color.yellow;																				// Create a line between the car position and the target position
			Gizmos.DrawLine(transform.position, target.position);
		}
	}
}
