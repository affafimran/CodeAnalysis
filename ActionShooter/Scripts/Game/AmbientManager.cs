using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AmbientManager{
	
	public static Color32 sunColor = new Color32(255,249,218,255);
	public static float sunIntensity = 0.55f;
	public static float shadowStrength = 0.5f;

	public static Color32 ambientColor = new Color32(40,55,55,255); 
	public static Color32 backgroundColor = new Color(60,200,240,255); 
	
	public static Color32 fogColor = new Color32(177,170,156,255); 
	public static float fogDensity = 0.0025f;

	public static Material skyboxMaterial;
	private static Texture skyboxTexture;

	public static string ambientStyleString;
	public static Dictionary<string, DicEntry> ambientStyle = new Dictionary<string, DicEntry>();

	public static void SetAmbientStyle(string style)
	{
//		Debug.Log ("AmbientManager SetAmbientStyle called: " + style);
		ambientStyleString = style;

		ambientStyle = Data.Shared["AmbientStyle"].d[ambientStyleString].d;

		// Ambient
		ambientColor = GenericFunctionsScript.ColorFromList(ambientStyle["ALC"].l);
//		Debug.Log ("AmbientManager found light color: " + ambientColor.ToString());
		RenderSettings.ambientLight = ambientColor;

		// Skybox
		skyboxTexture = Resources.Load("Textures/World/Skybox" + ambientStyle["SBT"].s + "_Texture") as Texture;
		skyboxMaterial = GameObject.Find("Skybox_Prefab").GetComponent<MeshRenderer>().material;
		skyboxMaterial.mainTexture = skyboxTexture;

		// Sunlight
		sunColor = GenericFunctionsScript.ColorFromList(ambientStyle["SLC"].l);
		GameObject.Find("SunLight").GetComponent<Light>().color = sunColor;
		sunIntensity = ambientStyle["SLI"].f;
		GameObject.Find("SunLight").GetComponent<Light>().intensity = sunIntensity;
		shadowStrength = ambientStyle["HSS"].f;
		GameObject.Find("SunLight").GetComponent<Light>().shadowStrength = shadowStrength;
		GameObject.Find("SunLight").GetComponent<Light>().shadowBias = 0.15f; // hardcoded for now.

		// Fog
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		fogColor = GenericFunctionsScript.ColorFromList(ambientStyle["FC"].l);
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogStartDistance = ambientStyle["FSD"].f;
		RenderSettings.fogEndDistance = ambientStyle["FED"].f;
		fogDensity = ambientStyle["FD"].f;
		RenderSettings.fogDensity = fogDensity; // Not used when Linear.

		// Now using global fog, we should set this to false otherwise it will fog the minimap.
		RenderSettings.fog = false;
	}
}


