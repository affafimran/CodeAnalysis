//Description : SimpleScale.cs : Change the gameObject scale
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleScale : MonoBehaviour {

	public int 				speed 	= 2;
	public Vector3 			dir 	= new Vector3(0,1,0);
	public AnimationCurve	curveX 	= new AnimationCurve ();
	public AnimationCurve	curveY 	= new AnimationCurve ();
	public float 			tmpTime = 0;

	public RectTransform 	rectTransform;
	public bool 			startAnimation;

	// Use this for initialization
	void Start () {
		//startAnimation = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (startAnimation) {
			tmpTime = Mathf.MoveTowards (tmpTime, 1, Time.deltaTime*speed);

			rectTransform.localScale = new Vector3 (curveX.Evaluate (tmpTime), curveY.Evaluate (tmpTime), 1);
			if (tmpTime == 1)
				startAnimation = false;
		}
	}
}
