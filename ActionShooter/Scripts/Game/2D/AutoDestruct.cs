using UnityEngine;
using System.Collections;

public class AutoDestruct : MonoBehaviour {

	// Epic script that deletes this gameobject after some time.

	public float time;

	void Start () {
			if (time == 0.0f) time = 2.0f;
			Object.Destroy(gameObject, time);
	}

}
