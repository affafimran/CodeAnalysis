//Description : DifficultyManager.cs : list of difficulty parameters for each car 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour {

	public bool 			SeeInspector = false;

	public int 				selectedDifficulties = 0;

	[System.Serializable]
	public class _difficulties{
		public List<float> 	addGlobalSpeedOffset	= new List<float>();				// Add global speed offset
		public List<int> 	waypointSuccesRate 		= new List<int>();					// if 100% the car follow the best path
		public List<float> 	waypointMinTarget 		= new List<float>();				// offset position minimum on the path
		public List<float> 	waypointMaxTarget 		= new List<float>();				// offset position maximum on the path 	

		public List<int> 	speedSuccesRate	 		= new List<int>();					// if 100% the car speed is optimal
		public List<float> 	speedOffset 			= new List<float>();				// offset

		public List<int> 	rotationSuccesRate 		= new List<int>();					// if 100% the car rotation is optimal 
		public List<float> 	rotationOffset 			= new List<float>();				// offset  

		public _difficulties(){}
	}

	public List<_difficulties> 		difficulties 	= new List<_difficulties>();		// list of difficulty parameters for each car
}
