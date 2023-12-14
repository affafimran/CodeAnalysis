using UnityEngine;
using System.Collections;

/// <summary>
/// Spawner.
/// Spawns stuff. See SharedData.txt - Spawners section to see what and how it can spawn.
/// PLEASE NOTE: SINCE THIS COMPONENT CAN BE ADDED DURING EDITING WE STILL NEED TO 'REGISTER' THIS GO TO THE SPAWNERMANAGER.
/// THEREFORE THE Start() METHOD AND(!) AM ADDITION TO THE SCRIPT EXECUTION ORDER TO MAKE SURE IT WON'T RUN BEFORE THE INITIALIZATION OF ITS MANAGER
/// </summary>
public class Spawner : MonoBehaviour
{
	public bool spawnerActive = false; // are we active
	public string spawner = "None"; // type/name
	public SpawnerData spawnerData = new SpawnerData(); // my spawnerdata
	public TargetData targetData = new TargetData(); // targetdata
	[HideInInspector] public bool dynamic = false; // are we dynamic

	// Start
	void Start()
	{
		// Get spawner data on startup
		if (!dynamic && spawner != "None"){
			if (!Data.Shared["Spawners"].d.ContainsKey(spawner)){
				Debug.Log("[Spawner] No such spawner ("+spawner+") in SharedData.txt.");
				spawnerActive = false;
				return;
			}
			// new data, update, add this gO to the list
			spawnerData = new SpawnerData();
			spawnerData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Spawners"].d[spawner].d, spawnerData) as SpawnerData;
			spawnerData.intervalTimer = spawnerData.interval;
			SpawnerManager.spawners.Add(gameObject);
			spawnerActive = false; // just in case
			if(spawner == "BigBoss") spawnerActive = true; // epic last minute [HACK]. It is visually much better to see the BigBoss instantly in the final mission...
		}
	}

	/// <summary>
	/// Initialize a Spawner.
	/// <para>This method is only called when you add a spawner dynamically through the SpawnerManager</para>
	/// </summary>
	/// <param name="aSpawnerData">A spawner data.</param>
	/// <param name="active">If set to <c>true</c> active.</param>
	public void Initialize(SpawnerData aSpawnerData, bool active)
	{
		spawnerActive = active; // active
		spawnerData = aSpawnerData; // data
		spawner = spawnerData.entry; // name
		dynamic = true; // tell we're dynamic (since we were created dynamically)
	}

	void Update ()
	{
		if (!spawnerActive) return; // no stuff when inactive

		targetData = TargetManager.GetTargetData(gameObject); // get targetdata
		spawnerData.inRange = targetData.target && targetData.distance <= spawnerData.range;
		if (!spawnerData.inRange) return; // if not inRange get out of here

		// so let's do this
		if (spawnerData.amount > 0){ // do we have 'spawns left'
			spawnerData.intervalTimer -= Time.deltaTime; // countdown to next spawn
			if (spawnerData.intervalTimer <= 0f){ // we can spawn
				spawnerData.intervalTimer = spawnerData.interval; // reset intervalTimer
				bool result = SpawnerManager.Spawn(gameObject, spawnerData); // Spawn it...
				if (!spawnerData.unlimitedAmount && result) --spawnerData.amount; // count it
				if (spawnerData.amount == 0) Destroy(); // Destroy when done
			}
		}
	}

	/// <summary>
	/// Destroy this Spawner.
	/// </summary>
	public void Destroy()
	{
		SpawnerManager.spawners.Remove(gameObject); // Remove from list
		Destroy(gameObject); // Destroy this object
	}
}

