using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class BossActivationTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider aCollider)
	{
		// [HARDCODED] to hammer
		Hammer hammer = Scripts.hammer;
		bool activate = (aCollider.gameObject == hammer.gameObject);
		if (hammer.vehicleData.isInVehicle){
			List<Collider> colliders = hammer.vehicleData.vehicle.GetComponentsInChildren<Collider>().ToList();
			activate = colliders.Contains(aCollider);
		}
		
		if (activate){
			Debug.Log("[BossActivationTrigger] BossTrigger activated: " + gameObject.name);
			string bossAsString = MissionManager.missionData.target + "_Prefab";
			GameObject bossAsObject = GameObject.Find(bossAsString);
			Boss boss = bossAsObject.GetComponent<Boss>();
			boss.bossData.active = true;
			gameObject.SetActive(false);
		}
	}
}
