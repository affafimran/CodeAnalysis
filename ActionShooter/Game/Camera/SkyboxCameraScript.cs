using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Skybox camera script.
/// <para>Ideally this script or the functionality should be embedded in CameraScript.</para>
/// <para>Currently it is a separate MonoBehavior attached to the skybox camera in the Game scene.</para>
/// </summary>
public class SkyboxCameraScript : MonoBehaviour {

	public float verticalOffset = 0f; // offset up down

	private Camera skyboxCamera; // skybox camera reference
	private Camera parentCamera; // parent camera (mostly activeCamera) reference

	void Start() {
		GetCamera(); // :(
	}

	void LateUpdate(){
		if (parentCamera == null) GetCamera(); // :(
		skyboxCamera.rect = parentCamera.rect; // rect
		skyboxCamera.transform.rotation = parentCamera.transform.rotation; // rotation
		skyboxCamera.transform.position = Vector3.zero + new Vector3(0, verticalOffset, 0); // position with vertical offset if you want this... (move skybox up and down)
		skyboxCamera.fieldOfView = parentCamera.fieldOfView; // fieldOfView
	}

	void GetCamera(){
		skyboxCamera = gameObject.GetComponent<Camera>(); // get the skybox camera from this gameObject
		parentCamera = CameraManager.activeCamera.GetComponent<Camera>(); // get activeCamera camera component
		skyboxCamera.renderingPath = parentCamera.renderingPath; // set so same renderingpath
	}

}
