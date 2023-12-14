using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SpawnerManager
{
	public static int spawnerCounter; // global counter of the amount of spawners (dynamically) created
	public static List<GameObject> spawners = new List<GameObject>(); // not likey a lot
	public static GameObject spawnerHolder; // this gameobject holds the spawner with no parent!

	public enum TYPES{Enemy, Vehicle, PickUp}; // The types of objects that can be spawned

	// Rotation of the spawned object 
	// None, Random rotation, use GameObjects rotation, Offset (look outwards from spawner), InverseOffset (look towards spawner)
	public enum ROTATION{None, Random, GameObject, Offset, InverseOffset};

	/// <summary>
	/// Initialize the SpawnerManager
	/// </summary>
	public static void Initialize()
	{
		spawnerCounter = spawners.Count; // amount of spawners here
		spawnerHolder = new GameObject("Spawners"); // new object THIS IS TEMP
		SetSpawnersActive(false); // this is because some may already been 'start' or 'awake'. Only because you can add Spawners to exisiting objects in your scene
	}

	/// <summary>
	/// <para>Adds a  new spawner.</para>
	/// <para>Needs a type (see SharedData.txt) and a position.</para>
	/// <para>Additionaly you can tell it to be (in)active at creation. (Default is true).</para>
	/// </summary>
	/// <returns>SpawnerData.</returns>
	/// <param name="aType">A type.</param>
	/// <param name="aPosition">A position.</param>
	public static SpawnerData AddSpawner(string aType, Vector3 aPosition){return AddSpawner(aType, aPosition, true);}
	public static SpawnerData AddSpawner(string aType, Vector3 aPosition, bool activeOnStart)
	{
		spawnerCounter++; // you never know what you need it for?
		if (aType == "None") return null; // Not too happy with this
		GameObject tSpawner = new GameObject("Spawner");
		tSpawner.transform.position = aPosition;
		SpawnerData spawnerData = new SpawnerData();
		spawnerData.entry = aType; // Manually update hidden name.
		spawnerData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Spawners"].d[aType].d, spawnerData) as SpawnerData;
		spawnerData.intervalTimer = spawnerData.interval;
		tSpawner.AddComponent<Spawner>().Initialize(spawnerData, true);
		tSpawner.transform.parent = spawnerHolder.transform;
		spawners.Add(tSpawner);
		return spawnerData;
	}

	/// <summary>
	/// Add multiple spawners at a location.
	/// </summary>
	/// <param name="aPosition">A position.</param>
	/// <param name="aSpawners">A spawners.</param>
	public static void AddSpawners(Vector3 aPosition, List<string> aSpawners)
	{
		if (aSpawners.Count == 0) return; // don't do anything whne the list is empty
		if (aSpawners.Contains("Random")) AddSpawner(GetRandomSpawnerFromList(aSpawners), aPosition); // This is one spawn if the list contains "Random"
		else{// This is 'normal' OR all pickups in the list
				foreach(string spawner in aSpawners){
				AddSpawner(spawner, aPosition);
			}
		}
	}

	/// <summary>
	/// Gets a random spawner (string) from a list of spawners.
	/// </summary>
	/// <returns>The random spawner from list.</returns>
	/// <param name="aSpawners">A spawners.</param>
	public static string GetRandomSpawnerFromList(List<string> aSpawners)
	{
		aSpawners.Remove("Random"); // Remove "Randomg" to prevent adding a "Random" (=non-existing spawner)
		int random = Random.Range(0, aSpawners.Count); // get a random int
		return aSpawners[random]; // return random spawner strings
	}

	/// <summary>
	/// Get a random spawner from all available spawners in SharedData.txt - Spawners.
	/// </summary>
	/// <returns>The random spawner.</returns>
	public static string GetRandomSpawner()
	{
		int random = Random.Range(0, Data.Shared["Spawners"].d.Count);
		List<string> keys = new List<string>(Data.Shared["Spawners"].d.Keys);
		return keys[random];
	}

	/// <summary>
	/// Get the score of a specific object in a Spawner.
	/// </summary>
	/// <returns>The score.</returns>
	/// <param name="aSpawnerData">A spawner data.</param>
	public static int GetScore(SpawnerData aSpawnerData)
	{
		int score = 0; // default score
		string entry = aSpawnerData.subType; // store which spawner it is
		switch (aSpawnerData.type)
		{
			case TYPES.Enemy: // get from enemy data
				score = Data.Shared["Characters"].d[entry].d["score"].i;
				break;
			case TYPES.Vehicle: // get from vehicle data
				score = Data.Shared["Vehicles"].d[entry].d["score"].i;
				break;
			case TYPES.PickUp: // get nothing, this is always 0
				score = 0;
				break;
		} 
		return score; // return the score
	}

	/// <summary>
	/// Spawn the actual object.
	/// <para>Returns true when spawning was succesful.</para>
	/// </summary>
	/// <param name="aGameObject">A game object.</param>
	/// <param name="spawnerData">Spawner data.</param>
	public static bool Spawn(GameObject aGameObject, SpawnerData spawnerData)
	{
		bool result = true; // default true since a character spawn is the only one that can fail, we just assume the other two never fail ;) 

		SpawnData spawnData   = new SpawnData(); // new spawndata
		spawnData.gameObject  = aGameObject; // store the object 
		spawnData.offset      = SpawnerManager.GetOffset(spawnerData.minMaxOffset); // create an offset from the spawnposition
		spawnData.position    = SpawnerManager.GetPosition(aGameObject, spawnData.offset, spawnerData); // get the final position, taking the offset into account
		spawnData.rotation    = SpawnerManager.GetRotation(spawnerData.rotation, aGameObject, spawnData.offset); // calculate the rotation of the spawned object

		// what to spawn
		switch(spawnerData.type){
			case SpawnerManager.TYPES.Enemy: // Enemy
				result = CharacterManager.AddCharacter(spawnerData.subType, CharacterManager.CONTROLLER.AI, spawnData);
				break;
			case SpawnerManager.TYPES.Vehicle: // Vehicle
				Vehicle vehicle = VehicleManager.AddVehicle(spawnerData.subType, VehicleManager.CONTROLLER.AI, spawnData);
				vehicle.SetVehicleActive(true);
				break;
			case SpawnerManager.TYPES.PickUp: // Pickup
				PickUpManager.AddPickUp(spawnerData.subType, spawnData.position);
				break;
		}
		return result; // return our succes or failure
	}

	/// <summary>
	/// Calculate a random offset based upon a vector 2d with max. values.
	/// </summary>
	/// <returns>The offset as a Vector3 where y is always 0.</returns>
	/// <param name="minMaxOffset">Minimum max offset.</param>
	public static Vector3 GetOffset(Vector2 minMaxOffset)
	{
		Vector2 offset2D = Random.insideUnitCircle * Random.Range(minMaxOffset.x, minMaxOffset.y);
		Vector3 offset = new Vector3(offset2D.x, 0, offset2D.y);
		return offset;
	}	

	/// <summary>
	/// Calculate the correct orientation for an object to be spawned.
	/// </summary>
	/// <returns>The rotation.</returns>
	/// <param name="aRotation">A rotation.</param>
	/// <param name="aSpawnObject">A spawn object.</param>
	/// <param name="anOffsetPosition">An offset position.</param>
	public static Quaternion GetRotation (SpawnerManager.ROTATION aRotation, GameObject aSpawnObject, Vector3 anOffsetPosition)
	{
		// references
		Quaternion rotation = Quaternion.identity;
		Vector3 sourcePosition = aSpawnObject.transform.position;
		Vector3 targetPosition = sourcePosition + anOffsetPosition;

		switch(aRotation){
		case SpawnerManager.ROTATION.GameObject: // Get from spawnObject
			rotation = aSpawnObject.transform.rotation;
			break;
		case SpawnerManager.ROTATION.Offset: // Look outwards (away from spawnobject)
			rotation = Quaternion.LookRotation((targetPosition-sourcePosition).normalized);
			break;
		case SpawnerManager.ROTATION.InverseOffset: // Look inwards (to spawnObject)
			rotation = Quaternion.LookRotation((sourcePosition-targetPosition).normalized);
			break;
		case SpawnerManager.ROTATION.Random: // Do whatever you like...
			rotation = Random.rotation;
			break;
		}
		return rotation; // return the result
	}

	/// <summary>
	/// Calculate the final position of the object to be spawned. This takes a possible offset into account.
	/// </summary>
	/// <returns>The position as a Vector3.</returns>
	/// <param name="aSpawnObject">A spawn object.</param>
	/// <param name="anOffset">An offset.</param>
	/// <param name="aSpawnerData">A spawner data.</param>
	public static Vector3 GetPosition(GameObject aSpawnObject, Vector3 anOffset, SpawnerData aSpawnerData)
	{
		Vector3 position = aSpawnObject.transform.position; // spawnObject position
		if (aSpawnerData.rotation != ROTATION.GameObject) position += anOffset; // basic offset 
		else{ // correct ones
			Vector2 minMaxOffset = aSpawnerData.minMaxOffset;
			position = position + (aSpawnObject.transform.right * Random.Range(-minMaxOffset.x, minMaxOffset.x));
			int tForwardOffset = (int)Random.Range(0f, minMaxOffset.y);
			position = position + (aSpawnObject.transform.forward * tForwardOffset);
		}
		return position; // return final position
	}	

	/// <summary>
	/// Creates 'existing' data from an object that is already available but was never spawned (or lost the data).
	/// <para>E.g. adding a character without a spawner but through code, like the Hammer.</para>
	/// </summary>
	/// <returns>The existing spawn data.</returns>
	/// <param name="aParent">A parent.</param>
	/// <param name="aGameObject">A game object.</param>
	public static SpawnData GetExistingSpawnData(GameObject aParent, GameObject aGameObject)
	{
		SpawnData spawnData = new SpawnData(); // new data
		spawnData.gameObject = aParent; // spawner object (can be null)
		spawnData.offset = Vector3.zero; // no offset
		spawnData.position = aGameObject.transform.position; // current position
		spawnData.rotation = aGameObject.transform.rotation; // current rotaion
		return spawnData; // return the data
	}

	/// <summary>
	/// Enable or disable all available spawners.
	/// </summary>
	/// <param name="aBool">If set to <c>true</c> a bool.</param>
	public static void SetSpawnersActive(bool aBool)
	{
		Spawner tComponent;
		foreach (GameObject spawner in spawners)
		{
			if (spawner == null) continue;
			tComponent = spawner.GetComponent<Spawner>();
			tComponent.spawnerActive = aBool;
		}	
	}

	
	/// <summary>
	/// Destroy spawners (could be multiple ones).
	/// <para>If a spawner is destroyed we award the user with score that was left in the spawner.</para>
	/// <para>So you can still get points for destroying vehicles with spawner which are not done spawning yet.</para>
	/// <para>This is false by default!</para>
	/// </summary>
	/// <param name="aGameObject">A game object.</param>
	public static void DestroySpawners(GameObject aGameObject) {DestroySpawners(aGameObject, false);}
	public static void DestroySpawners(GameObject aGameObject, bool aState)
	{
		if (aGameObject == null) return; // no spawners anyway
		Spawner[] tSpawners = aGameObject.GetComponentsInChildren<Spawner>();
		foreach(Spawner spawner in tSpawners){
			if (aState){ // if true we process/award the remaining spawns
				SpawnerData spawnerData = spawner.spawnerData; 
				for (int i = 1; i <= spawnerData.amount; i++){
					MissionManager.ProcessTarget(spawnerData.subType, GetScore(spawnerData));
				}
			}
			spawner.Destroy();	
		}
	}

	/// <summary>
	/// Reset SpawnerManager
	/// </summary>
	public static void Reset()
	{
		spawnerCounter = 0;
		spawners = new List<GameObject>();
	}

	/// <summary>
	/// Destroy the SpawnerManger
	/// <para>Reset and destroy spawnerHolder.</para>
	/// </summary>
	public static void Destroy()
	{
		SpawnerManager.Reset();
		UnityEngine.Object.Destroy(spawnerHolder);
	}
}
