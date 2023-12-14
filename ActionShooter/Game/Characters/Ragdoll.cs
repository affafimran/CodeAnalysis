using UnityEngine;
using System.Collections;

//A very simple script to show how to turn a Mecanim character into a ragdoll
//when the user presses space, assuming that the Unity Ragdoll Wizard has
//been used to add the ragdoll RigidBody components

public class EnemyRagdoll : MonoBehaviour {

	void Awake()
	{
		SetRagdoll(false);
	}

	public void SetRagdoll(bool aState)
	{
		//Get an array of components that are of type Rigidbody
		Rigidbody[] bodies = gameObject.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rb in bodies)
			rb.isKinematic = !aState;
		
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders)
			if (collider.GetType() != typeof(CharacterController)) collider.isTrigger = !aState;

		gameObject.GetComponent<Animator>().enabled = !aState;

	}
}
