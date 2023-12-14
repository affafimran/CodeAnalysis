// Description : CheckOtherCarCollision.cs use on gameobject "CheckOtherCarCollision" : Use in CarAI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOtherCarCollision : MonoBehaviour {

	public CarAI				car;
	public GameObject			CarTarget;

	public void ChangeCarTargetPosition(float NewPosX){										// Change the position of the target that follow the car AI. Call on script CarAI.cs
		if(CarTarget)CarTarget.transform.localPosition = new Vector3 (NewPosX, 0, 0);
	}
}
