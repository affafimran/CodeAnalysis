// Description : Activate Inputt Buttons if needed when player come back from Pause Mode
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInitAfterPause : MonoBehaviour {

	public GameObject 	MobileInputCanvas;
	private bool		b_MobileInputActivated = false;

	// Use this for initialization
	void Start () {
		if (MobileInputCanvas && MobileInputCanvas.activeSelf) {
			b_MobileInputActivated = true;
		}
	}
	

	public void ActivateMobileInput () {
		if (MobileInputCanvas && b_MobileInputActivated) {
			MobileInputCanvas.SetActive (true);
		}
	}
}
