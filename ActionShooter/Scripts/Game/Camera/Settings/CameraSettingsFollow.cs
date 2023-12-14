using UnityEngine;
using System.Collections;

/// <summary>
/// Camera settings for the Follow state.
/// </summary>
[System.Serializable]
public class CameraSettingsFollow
{
	public bool parentToTarget = false; // parent to target. If false this will position the camera and NOT orientate it.

	public bool useTargetUp = false; // use the up vector of the target or not (e.g. when doing barrel rolls should you look at if from a world view or from the object itself)
	internal Vector3 previousUp = Vector3.up; // storage for previous Up vector so we can lerp. 
	public float targetUpLerp = 0.01f; // lerp speed

	public string targetName = "None"; // name from target object
	public Transform target = null; // target transform
	public Vector3 targetOffset = new Vector3(); // offset from target
	
	public string lookAtName = "None"; // name from look at object
	public Transform lookAt = null; // look at transform
	public Vector3 lookAtOffset = new Vector3(0, 1, 0); // look at offset from look at transform

	public string positionName = "None"; // name from position object
	public Transform position = null; // position object transform
	public Vector3 positionOffset = new Vector3(0, 1, 1); // position offset from position transform

	public Vector3 lerpMultiplier = new Vector3(15, 15, 15); // lerp x,y,z
	public bool lerp = false; // should we lerp

	public float fov = 60.0f; // camera field of view
	
	public string mouseSettings = "Default"; // mouse setting name

	public bool rayCasting = true;
	internal LayerMask rayCastLayers = (1 << 0) | (1 << 13) | (1 << 14); //| (1 << 15);
	internal Ray ray;
	internal RaycastHit rayCastHit;

	internal GameObject cameraTarget   = null; // reference
	internal GameObject cameraLookAt   = null; // reference
	internal GameObject cameraPosition = null; // reference
}

