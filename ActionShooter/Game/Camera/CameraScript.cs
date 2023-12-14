using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Camera Script.
/// <para>Main camera script. Controls camera position, look at, etc. etc. </para>
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
	private Camera cameraObject; // new reference to camera component for easy access
	public CameraData cameraData; // camera data (see CameraData class)

	public CameraMouseSettings mouseSettings; // mouse settings (see MouseSettingsClass)

	public CameraSettingsFollow follow; // follow settings (See CameraSettingsFollow)
	public CameraSettingsMimic mimic; // mimic settings (TBD)
	public CameraSettingsFree free; // free settings (TBD)

	// new gameobject reference for audio. This object has the listener and should be positioned at the cameras location.
	// We position it since we want to have more control over this. So we don't parent it OR attach the listener to the camera.
	private GameObject audioObject; 

	/// <summary>
	/// Initialize the camera script. 
	/// </summary>
	/// <param name="aCameraData">A camera data.</param>
	public void Initialize (CameraData aCameraData)
	{
		cameraObject = gameObject.GetComponent<Camera>(); // store camera
		cameraData = aCameraData; // store data
		InitializeSettings();  // initialize the current setting
		audioObject = AudioManager.audioGameObject; // store the main audio object
	}

	/// <summary>
	/// Initializes a specific setting.
	/// <para>This is always based upon the current 'cameraData.state'.</para>
	/// </summary>
	public void InitializeSettings(){
		switch(cameraData.state){
			case CameraManager.STATES.Follow: follow = new CameraSettingsFollow(); InitializeFollow(); break;
			case CameraManager.STATES.Mimic: InitializeMimic(); break;
			case CameraManager.STATES.Free: InitializeFree(); break;
		}
	}

	/// <summary>
	/// Initialize the Follow state/setting.
	/// </summary>
	public void InitializeFollow(){InitializeFollow(true);}
	public void InitializeFollow(bool reParse){
	
		// Update the class (only when reParse is true, sometimes you don't want this. E.g. when resetting this script)
		if(reParse) follow = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Cameras"].d[cameraData.settings].d, follow) as CameraSettingsFollow;
  
		// create dummies OR use the active ones (No need to destroy/create them on each setting change)
		GameObject findObject = GameObject.Find("Camera"+cameraData.id+"Target");
		follow.cameraTarget   = (findObject == null) ? new GameObject("Camera"+cameraData.id+"Target") : findObject;
		findObject            = GameObject.Find("Camera"+cameraData.id+"LookAt");
		follow.cameraLookAt   = (findObject == null) ? new GameObject("Camera"+cameraData.id+"LookAt") : findObject;
		findObject            = GameObject.Find("Camera"+cameraData.id+"Position");
		follow.cameraPosition = (findObject == null) ? new GameObject("Camera"+cameraData.id+"Position") : findObject;

		// parent lookAt object & position object to pivot (=cameraTarget)
		follow.cameraLookAt.transform.parent = follow.cameraTarget.transform;
		follow.cameraLookAt.transform.localPosition = follow.lookAtOffset;
		follow.cameraLookAt.transform.localRotation = Quaternion.identity;
		follow.lookAt = follow.cameraTarget.transform;
		
		follow.cameraPosition.transform.parent = follow.cameraTarget.transform;
		follow.cameraPosition.transform.localPosition = follow.positionOffset;
		follow.cameraPosition.transform.localRotation = Quaternion.identity;
		follow.position = follow.cameraTarget.transform;

		// parent the cameraTarget (=pivot) to our target to follow
		follow.cameraTarget.transform.parent = follow.target;
		follow.cameraTarget.transform.localPosition = follow.targetOffset;
		follow.cameraTarget.transform.localRotation = Quaternion.identity;

		//  create mouseSettings
		mouseSettings = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["CameraMouseSettings"].d[follow.mouseSettings].d, new CameraMouseSettings()) as CameraMouseSettings;
		mouseSettings.invertMouse = GameData.invertMouse; // override invertMouse from (global) GameData

		// set fov instantly
		cameraObject.fieldOfView = follow.fov;
	}

	/// <summary>
	/// Initializes the Mimic setting/state. TBD
	/// </summary>
	public void InitializeMimic(){}

	/// <summary>
	/// Initializes the Free setting/state. TBD
	/// </summary>
	public void InitializeFree(){}

	public void LateUpdate ()
	{
		// store delta time
		cameraData.deltaTime = (cameraData.useUnscaledDeltaTime) ? Time.unscaledDeltaTime : Time.deltaTime;

		// store myDelta time
		cameraData.currentframe = Time.realtimeSinceStartup;
		cameraData.myDelta = cameraData.currentframe - cameraData.lastframe;
		cameraData.lastframe = cameraData.currentframe; 

		// update specific state
		switch(cameraData.state){
			case CameraManager.STATES.Follow: UpdateFollow(); break;
			case CameraManager.STATES.Mimic: UpdateMimic();  break;
			case CameraManager.STATES.Free: UpdateFree(); break;
		}

		// set the audio object to the location of the camera
		audioObject.transform.position = gameObject.transform.position;
	}

	/// <summary>
	/// Update the Follow setting/state.
	/// </summary>
	public void UpdateFollow()
	{
		// mouse input
		mouseSettings.invertMouse = GameData.invertMouse; // check/set if we should inverse (this could be changed anytimge)
		if (cameraData.allowInput && (mouseSettings.mouseLookX || mouseSettings.mouseLookY)) UpdateMouseInput(); // only get mouseData if input is allowed or any of the mouseLook axis should be used

		// parent or position?
		if (follow.target != null){
			// parent and reset
			if (follow.parentToTarget && transform.parent != follow.target){	

				follow.cameraTarget.transform.parent = follow.target;
				follow.cameraTarget.transform.localPosition = follow.targetOffset;
				follow.cameraTarget.transform.localRotation = Quaternion.identity;

			} else{ // set position 'manually'
				if (follow.cameraTarget.transform.parent != null) follow.cameraTarget.transform.parent = null;
				Vector3 tOffset = (follow.cameraTarget.transform.right*follow.targetOffset.x) + (follow.cameraTarget.transform.up*follow.targetOffset.y) + (follow.cameraTarget.transform.forward*follow.targetOffset.z);
				follow.cameraTarget.transform.position = follow.target.transform.position + tOffset;
			}
		}

		follow.cameraLookAt.transform.parent = follow.lookAt; // parent lookAt to a look at object
		follow.cameraLookAt.transform.localPosition = follow.lookAtOffset; // offset for lookat
		follow.cameraPosition.transform.parent = follow.position; // parent position to a position object
		follow.cameraPosition.transform.localPosition = follow.positionOffset; // a position offset

		// mouse look
		if (mouseSettings.mouseLookX) follow.cameraTarget.transform.Rotate(0, mouseSettings.x, 0);
		if (mouseSettings.mouseLookY) follow.cameraTarget.transform.localEulerAngles = new Vector3(-mouseSettings.y, follow.cameraTarget.transform.localEulerAngles.y, 0);
			
		// store targetCameraposition (we're going to do all kinds of stuff with this)
		Vector3 targetCameraPosition = follow.cameraPosition.transform.position;

		// setup RayCast
		Vector3 rayPosition = follow.cameraLookAt.transform.position; //follow.cameraTarget.transform.position;
		//rayPosition.y += follow.lookAtOffset.y;

		Vector3 rayDirection = follow.cameraPosition.transform.position - rayPosition;
		float rayLength = rayDirection.magnitude;
		rayDirection.Normalize();


		Debug.DrawRay(rayPosition, rayDirection * rayLength, Color.magenta);

		// Cast ray (if hit, update targetCameraPosition to interesectPoint with small offset)
		if (follow.rayCasting)
		{
			follow.ray = new Ray(rayPosition, rayDirection);
			if(Physics.Raycast(follow.ray, out follow.rayCastHit, rayLength, follow.rayCastLayers)){
				// THIS IS BETTER BUT STILL NOG CIGAR???
				targetCameraPosition = follow.rayCastHit.point - (rayDirection * 0.25f); // set the camera at the maximum allowed position! There should be a nicer solution here.
				follow.cameraLookAt.transform.position += -rayDirection * (rayLength - follow.rayCastHit.distance); 
				cameraObject.nearClipPlane = Mathf.Min(0.25f, follow.rayCastHit.distance/rayLength); // Adjust the clipping when we're going 'closer'
				//cameraObject.nearClipPlane =  Mathf.Max(0.1f, 0.5f-follow.rayCastHit.distance/rayLength); 
			} else cameraObject.nearClipPlane = 0.5f;
		} 

		// [HACK] this is a lerping hack and def. not finished or usable under all circumstances
		if(follow.lerp)	{
			// set camera and look at target
			float avg = (cameraData.deltaTime + Time.smoothDeltaTime + cameraData.myDelta)*0.33333f; // odd avg. code
			// calculate all axis seperately so you can have different lerp speeds
			float posX = Mathf.Lerp (transform.position.x, targetCameraPosition.x, avg*follow.lerpMultiplier.x);
			float posY = Mathf.Lerp (transform.position.y, targetCameraPosition.y, avg*follow.lerpMultiplier.y);
			float posZ = Mathf.Lerp (transform.position.z, targetCameraPosition.z, avg*follow.lerpMultiplier.z);
			targetCameraPosition = new Vector3(posX, posY, posZ); 
		}

		// update fov (you could also lerp this for a smoother effect!)
		cameraObject.fieldOfView = follow.fov;

		// cameraShake
		Vector3 shakeOffset = Vector3.zero;
		if (cameraData.shake){
			// random location/offset
			shakeOffset = Random.onUnitSphere;
			shakeOffset = shakeOffset * cameraData.shakeIntensity;
			cameraData.shakeIntensity =  Mathf.Max(0.0f, cameraData.shakeIntensity-((cameraData.deltaTime)*13.33f));// lower the intensity per frame
			// shake camera
			follow.cameraTarget.transform.localRotation = follow.cameraTarget.transform.localRotation*Quaternion.Euler(shakeOffset);

			// PIETER. I commented this out since it is causing a bit harm.
			// The code allows for having more or less proper shaking in slowmotion but it forces rotation in such away that we don't look properly along the lookat
			// In otherwords when the camera is obstructed (e.g. wall behind) this cause the camera to rotate. We don't want this since the user should always have
			// the best 'view' available.
			// For the game this isn't an issue, as we don't shake in Hammer Time.

//			// (re)set the location if angle is small enough
//			if (Quaternion.Angle(cameraData.shakeSourceRotation, cameraData.shakeTargetRotation) <= 1f) {
//				cameraData.shakeSourceRotation = cameraData.shakeTargetRotation;
//				cameraData.shakeTargetRotation = Quaternion.Euler(shakeOffset);
//			} else cameraData.shakeSourceRotation = Quaternion.Slerp(cameraData.shakeSourceRotation, cameraData.shakeTargetRotation, Time.timeScale); // else slerp to targetRotation (shake)
//			follow.cameraTarget.transform.localRotation = cameraData.shakeSourceRotation; // set that rotation on the cameraTarget
		}

		// finally update the camera with the targetCameraPosition
		gameObject.transform.position = targetCameraPosition;

		// let the camera look at a specific position. 
		// You can use different different up vectors for this as e.g. you want more control how you rotate while flying/doing a barrelroll
		Vector3 up = Vector3.up;
		if (follow.useTargetUp){
			if (follow.lerp){
				up = Vector3.Lerp(follow.previousUp, follow.cameraTarget.transform.up, follow.targetUpLerp);
				follow.previousUp = up;
			} else up = follow.cameraTarget.transform.up;
		} 
		// set the actual lookAt
		gameObject.transform.LookAt(follow.cameraLookAt.transform.position, up);
	}

	/// <summary>
	/// Update the Mimic setting/state
	/// </summary>
	public void UpdateMimic(){}

	/// <summary>
	/// Updates the Free setting/state.
	/// </summary>
	public void UpdateFree(){}

	/// <summary>
	/// Updates the mouse input.
	/// </summary>
	public void UpdateMouseInput()
	{
		// X - no limits
		mouseSettings.x = CrossPlatformInputManager.GetAxis("Mouse X") * (mouseSettings.mouseSensitivity.x * GameData.mouseSensitivity);

		// [MOBILE] we process Y differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float y = CrossPlatformInputManager.GetAxis("Mouse Y");
		// Y - has limits
		if (y == 0) mouseSettings.storedY = mouseSettings.y; //follow.cameraTarget.transform.eulerAngles.x;
		if (GameData.mobile && y != 0){
			float newY = mouseSettings.storedY + (y * Mathf.Abs(mouseSettings.mouseLookYLimit.x));
			newY = Mathf.Clamp (newY, mouseSettings.mouseLookYLimit.x, mouseSettings.mouseLookYLimit.y);
			mouseSettings.y = Mathf.Lerp(newY, mouseSettings.y, 0.7f);
		} else {
			if(mouseSettings.invertMouse) mouseSettings.y -= y * (mouseSettings.mouseSensitivity.y * GameData.mouseSensitivity);
			else mouseSettings.y += y * (mouseSettings.mouseSensitivity.y * GameData.mouseSensitivity);
			mouseSettings.y = Mathf.Clamp (mouseSettings.y, mouseSettings.mouseLookYLimit.x, mouseSettings.mouseLookYLimit.y);
		}
	}

	/// <summary>
	/// Reset the camera script.
	/// </summary>
	public void Reset(){
		switch(cameraData.state){
			case CameraManager.STATES.Follow: ResetFollow(); InitializeFollow(false); break;
			case CameraManager.STATES.Mimic: ResetMimic(); InitializeMimic(); break;
			case CameraManager.STATES.Free: ResetFree(); InitializeFree(); break;
		}
	}

	/// <summary>
	/// Specific reset function for Follow setting/state.
	/// </summary>
	void ResetFollow(){
		// reset mouseSettings
		mouseSettings.x = 0f;
		mouseSettings.y = 0f;
	}

	/// <summary>
	/// Specific reset function for Mimic setting/state. TBD
	/// </summary>
	void ResetMimic(){}

	/// <summary>
	/// Specific reset function for Free setting/state. TBD
	/// </summary>
	void ResetFree(){}
}