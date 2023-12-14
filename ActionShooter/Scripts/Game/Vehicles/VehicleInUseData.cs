using UnityEngine;
using System.Collections;

[System.Serializable]
public class VehicleInUseData
{
	public bool inUse = false;
	public bool AI    = false;
	public GameObject driver;
	[HideInInspector] public string previousCameraSetting;
}


