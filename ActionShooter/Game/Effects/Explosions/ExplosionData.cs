using UnityEngine;
using System.Collections;

/// <summary>
/// Explosion data.
/// </summary>
[System.Serializable]
public class ExplosionData
{
	public string prefab = "None"; // prefab to load/instantiate
	public string sound = "None"; //sound to play
	public float range = 0f; // range (for shake intensity
	public float shake = 0f; // shake amount
}
