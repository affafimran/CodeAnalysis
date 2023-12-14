using UnityEngine;
using System.Collections;

/// <summary>
/// Spawn data.
/// <para>Holds spawn data for objects to be spawned.</para>
/// </summary>
[System.Serializable]
public class SpawnData
{
	public GameObject gameObject = SpawnerManager.spawnerHolder; // the object where the spawner script was attached to
	public Vector3 position      = SpawnerManager.spawnerHolder.transform.position; // default position for the spawned object
	public Vector3 offset        = Vector3.zero; // default position offset for the object to be spawned
	public Quaternion rotation   = Quaternion.identity; // default rotation of the object to be spawned
}