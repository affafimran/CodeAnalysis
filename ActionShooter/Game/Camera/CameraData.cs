using UnityEngine;
using System.Collections;

/// <summary>
/// Camera Data.
/// <para>This class contains generic camera data.</para>
/// </summary>
[System.Serializable]
public class CameraData
{
	public bool allowInput = false; // allow input (mouse only for now)
	public bool debug = false; // debug
	public int id = 0; // id of the camera
	public CameraManager.STATES state = CameraManager.STATES.Follow; // Follow is default state since we pretty much use that all the time
	public string settings = "Default"; // settings name as defined in SharedData.txt

	public bool useUnscaledDeltaTime = false; // tell the camera to use (unscaled)deltaTime. Sometimes you don't want the camera to move in slowmotion when playing with the TimeScale.
	internal float deltaTime = 0.01f; // deltaTime
	internal float lastframe = 0f; // time last frame
	internal float currentframe = 0f; // time current frame
	internal float myDelta = 0f; // delta last & current

	public bool shake = true; // should the camera be able to shake. Shaking is done by rotating the camera target!
	public float shakeIntensity = 0.0f; // intensity of the shake

	internal Quaternion shakeSourceRotation = Quaternion.identity; // rotation reference for lerping
	internal Quaternion shakeTargetRotation = Quaternion.identity; // rotation reference for lerping 	
}