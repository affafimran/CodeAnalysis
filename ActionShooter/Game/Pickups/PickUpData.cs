using UnityEngine;
using System.Collections;

/// <summary>
/// PickUp data.
/// </summary>
[System.Serializable]
public class PickUpData
{
	public string  prefab 	= ""; // prefab to load
	public int value		= 0;  // value/score
	public float lifeTime   = -1.0f; // time it is alive
	public string sound     = ""; // sound to play
	public string effect    = ""; // effect to spawn when pickup up. e.g. sparks or whatever
}