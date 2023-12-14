using UnityEngine;
using System.Collections;

/// <summary>
/// Spawner data.
/// <para>Not to be confused with SpawnData. This class contains data for actual Spawner scripts</para>
/// </summary>
[System.Serializable]
public class SpawnerData
{
	[HideInInspector] public string entry = "None";
	public SpawnerManager.TYPES type = SpawnerManager.TYPES.Enemy; // Type of spawner
	public string subType = "EnemySuit"; // Subtype (see SharedData.txt for spawner subtypes)

	public float interval = 1f; // Time to next spawn ( <=0 spawns almost instantly, per frame delay)
	[HideInInspector] public float intervalTimer = 1f; // The interval timer / hidden
	public int amount = 1; // Amount of spawns (left) 
	public bool unlimitedAmount = false; // toggle this for unlimited spawns

	public bool inRange = false; // Spawner only runs when in range.
	public float range = 30f; // Range when spawner will activate, <= 0 is always in range
	public Vector2 minMaxOffset = new Vector2(0, 0); // 2d offset (min/max range) spawned objects will have a random offset based upon this variable
	public SpawnerManager.ROTATION rotation = SpawnerManager.ROTATION.GameObject; // Orientation of the spawned object
}
