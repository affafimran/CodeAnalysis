using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// TriggerInterface
/// Very basic script to detect if the player walks into a dummy box.
/// Now used to call cinematic shot on bosses.
/// </summary> 

public class TriggerInterface : MonoBehaviour {

	public string triggerName;

	void Start()
	{
		if (triggerName == "") triggerName = gameObject.name;
	}

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
			Debug.Log("[TriggerInterface] CinematicTrigger activated: " + gameObject.name);
			if (Scripts.interfaceScript != null) Scripts.interfaceScript.Trigger(triggerName);
			gameObject.SetActive(false);
		}
	}
}
