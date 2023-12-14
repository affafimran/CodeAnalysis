using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectScript : MonoBehaviour {
	ScrollRect scrollRect;
	bool mouseDown, buttonUp, buttonDown;
	// Use this for initialization
	void Start () {
		scrollRect = GetComponent<ScrollRect>();
	}
	
	// Update is called once per frame
	void Update () {
		if(mouseDown)
			if(buttonDown)
				ScrollForward();
			else
				ScrollBack();
	}

	public void ButtonDownIsPressed() {
		mouseDown = true;
		buttonDown = true;
	}

	public void ButtonUpIsPressed() {
		mouseDown = true;
		buttonUp = true;
	}

	void ScrollForward() {
		if(Input.GetMouseButtonUp(0)) {
			mouseDown = false;
			buttonDown = false;
		} else {
			scrollRect.horizontalNormalizedPosition -= 0.01f;
		}
	}

	void ScrollBack() {
		if(Input.GetMouseButtonUp(0)) {
			mouseDown = false;
			buttonUp = false;
		} else {
			scrollRect.horizontalNormalizedPosition += 0.01f;
		}
	}
}
