// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE

using UnityEngine;
using System;
using System.Collections;

public class Options
{
	public static void SetVisuals()
	{
		string tQualityLevel = (Application.platform == RuntimePlatform.WebGLPlayer || GameData.testWebGL) ? "WebGL"+Data.quality : Data.quality; // Now gotten straight from the globals / data.
		Debug.Log("[Options] SetVisuals quality: " + Data.quality);

		// find index of given name in quality settings
		string[] names = QualitySettings.names;
		int i = Array.IndexOf(names, tQualityLevel);
		if (i == -1) throw new UnityException("Unknown quality setting!");
		// Set Unity quality setting
		QualitySettings.SetQualityLevel(i, true);

		// Alert the camera manager of the change.
		CameraManager.SetPostProcessingEffects(Data.quality == "Fantastic"); // If it's not Fantastic, remove bloom and such.
		// Hardcoded for the Hammer
		CameraManager.SetCullingDistances(Data.quality);

	}

	public static void ToggleFullScreen()
	{
		// Often, the fullscreen resolution is not what you had expected.
		// This might be caused by the setting of the resolution before going fullscreen.
		// So you might want to get rid of that.
		if (!Screen.fullScreen)
		{
			Screen.SetResolution(Data.fullScreenWidth, Data.fullScreenHeight, true);
			Screen.fullScreen = true;
		} else
		{
			Screen.fullScreen = false;
		}

	}
}
