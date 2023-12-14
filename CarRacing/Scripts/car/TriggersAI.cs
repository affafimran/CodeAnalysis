// Description : TriggerAI.cs . Allow to manipulate car manage by the AI when tese cars enter specific trigger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggersAI : MonoBehaviour {
	public bool 							SeeInspector 				= false;

	public List<CarAI>						l_Cars = new List<CarAI>();							// access carAI component
	public List<bool>						l_allowRandomValue = new List<bool>();				// enabled the random value  for AI car
	public List<float>						l_targetsValues = new List<float>();				// the new value for each car
	public List<int>						successRate_BestTargetPos = new List<int>();		// success rate 

	public void InitTriggersAI(List<CarController> carAI){
		for(var i = 0;i< carAI.Count;i++){
			if(carAI[i]!=null)
				l_Cars.Add(carAI[i].gameObject.GetComponent<CarAI>());
		}
	}
	

	void OnTriggerEnter(Collider other){
		if(other.tag == "Car"){
			float randomValue = Random.Range (0, 101);
			for (var i = 0; i < l_Cars.Count; i++) {

				if (l_Cars [i].gameObject == other.gameObject) {
					//Debug.Log ("Trigger AI : " + other.name);
					if (l_Cars [i].enabled) {
						//Debug.Log (randomValue);
						if (successRate_BestTargetPos[i] >= randomValue) {	
							l_Cars [i].allowRandomValues = l_allowRandomValue [i];
							l_Cars [i].Target.transform.localPosition = new Vector3 (l_targetsValues [i], 0, 0);
						}

					} else{
						//Debug.Log ("Car manage by player");
					}
				}
			}

		}
	}
}
