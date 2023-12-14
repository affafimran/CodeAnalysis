using UnityEngine;
using System.Collections;

/// <summary>
/// Camera mouse settings.
/// </summary>
[System.Serializable]
public class CameraMouseSettings
{
	public bool invertMouse = false; // invert mouse over y?
	public bool mouseLookX = false; // should we use/update X 
	public bool mouseLookY = false; // should we use/update Y
	public Vector2 mouseLookXLimit = new Vector2(); // x,y limits for X axis (we don't currently use this, but you can if you want)
	public Vector2 mouseLookYLimit = new Vector2(); // x,y limits for Y axis
	public Vector2 mouseSensitivity = new Vector2(1.0f, 1.0f); // default mouse sensitivity
	public float x = 0f; // x offset
	public float y = 0f; // y offset
	//public float storedX = 0f; // We have not implemented this for the camera (yet) since we rotate the objects and not the camera (99/100 times)
	public float storedY = 0f;
}

