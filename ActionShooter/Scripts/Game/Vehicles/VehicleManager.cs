using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VehicleManager {
	
	public static int vehicleCounter = 0;
	public static List<GameObject> vehicles;
	public static GameObject vehicleHolder;

	public enum TYPES{Unkown, Helicopter, Plane, Turret, Tank, Car}
	public enum CONTROLLER {Empty, User, AI} // Different types of controllers, 0where none is a non-controlled unit

	public static int vehicleLayer = LayerMask.NameToLayer("Vehicles");

	public static List<string> availableVehicles;
	public static List<string> generatorVehicles;

	//--------------------------------------------------------------------------
	// Initialize this manager
	//--------------------------------------------------------------------------
	public static void Initialize()
	{
		vehicleCounter = 0;
		vehicles = new List<GameObject>();

		GameObject tVehicleHolder = GameObject.Find("Vehicles"); // Main unit object
		if (tVehicleHolder == null) vehicleHolder = new GameObject("Vehicles"); // Create or assign
		else vehicleHolder = tVehicleHolder;

		availableVehicles = new List<string>();

		foreach(string key in Data.Shared["Vehicles"].d.Keys) {
			availableVehicles.Add(key);
		}

		generatorVehicles = new List<string>
		{
			"F22",
			"TrafficSportsCar",
			"TrafficSchoolBus",
			"TrafficTruckGasTrailer",
			"Turret",
			"MissileTurret",
			"Tank",
			"MammothTank",
			"Helicopter",
			"Apache",
		};
	}
	
	//--------------------------------------------------------------------------
	// Spawn a new unit
	//--------------------------------------------------------------------------
	public static Vehicle AddVehicle(string aType, CONTROLLER aController, SpawnData aSpawnData){ return AddVehicle(aType, aController, aSpawnData, true);}
	public static Vehicle AddVehicle(string aType, CONTROLLER aController, SpawnData aSpawnData, bool aCreateMapIcon)
	{
		GameObject vehicle = null;

		VehicleData vehicleData = new VehicleData();
		vehicleData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Vehicles"].d[aType].d, vehicleData) as VehicleData;

		vehicle = Loader.LoadGameObject("Vehicles/"+vehicleData.prefab+"_Prefab");

		// Set the parent 
		vehicle.transform.parent = vehicleHolder.transform; // TEMP

		string componentAsString = aType;
		if (vehicleData.type == TYPES.Car) componentAsString = "Car"; //we need only the car script if this is a car, no need for all kinds of car scripts which would do the same
		Vehicle component = vehicle.AddComponent(GenericFunctionsScript.GetType(componentAsString)) as Vehicle;
		
		component.Initialize(vehicleData, aController, aSpawnData);

		vehicles.Add(vehicle); // Add the vehicle to the list
		vehicleCounter++; // count up 

		if (aCreateMapIcon) Scripts.map.CreateMapIcon(vehicle, aType); // create icon

		return component; // return it
	}

	public static void AddVehiclesFromChildren(GameObject aParent, CONTROLLER aController) {AddVehiclesFromChildren(aParent, aController, true);}
	public static void AddVehiclesFromChildren(GameObject aParent, CONTROLLER aController, bool aCreateMapIcon)
	{
		if (!GameData.createVehicles) return;

		int count = aParent.transform.childCount-1;
		GameObject child;
		string name;

		// hack
		if (!GameData.AIController) aController = CONTROLLER.Empty;

		for (int i = count; i >= 0; i--) { // We count down as some vehicles (non-dynamic ones) can be reparented to another object. e.g. a building
			child = aParent.transform.GetChild(i).gameObject;
			name = child.name.Substring(0, child.name.Length-7);
			if (Data.Shared["Vehicles"].d.ContainsKey(name))
			{
				VehicleData vehicleData = new VehicleData();
				vehicleData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Vehicles"].d[name].d, vehicleData) as VehicleData;
		
				string componentAsString = name;
				if (vehicleData.type == TYPES.Car) componentAsString = "Car"; //we need only the car script if this is a car, no need for all kinds of car scripts which would do the same
				Vehicle component = child.AddComponent(GenericFunctionsScript.GetType(componentAsString)) as Vehicle;
		
				component.Initialize(vehicleData, aController, SpawnerManager.GetExistingSpawnData(aParent, child));
				vehicles.Add(child);

				vehicleCounter++;

				if (aCreateMapIcon) Scripts.map.CreateMapIcon(child, name);
			}
		}
	}

	public static GameObject GetClosestVehicle(Vector3 aPosition)
	{
		float targetDistance = Mathf.Infinity;
		float distance;
		GameObject target = null;

		//List<GameObject> vehicles = new List<GameObject>(UnitManager.units);
		//vehicles.AddRange(TrafficManager.traffic);

		// bounds
		foreach (GameObject vehicle in vehicles)
		{
			if (vehicle == null) continue; // FOR THE CHEAT

			distance = (vehicle.transform.position - aPosition).sqrMagnitude;
			if (distance <= targetDistance)
			{
				targetDistance = distance;
				target = vehicle;
			}
		}

		// TROEP! NIET DOORDACHT, TEST MAAR MET DE MAMMOTH TANK
		if (target != null) 
			if (IsInBounds (target.transform.GetChild (0).gameObject, aPosition))
				return target;

		// view
		foreach (GameObject vehicle in vehicles)
		{
			if (vehicle == null) continue; // FOR THE CHEAT

			if (!vehicle.IsInView()) continue;
			distance = (vehicle.transform.position - aPosition).sqrMagnitude;
			if (distance <= targetDistance)
			{
				targetDistance = distance;
				target = vehicle;
			}
		}
		return target;
	}

	public static bool IsInBounds(GameObject aGameObject, Vector3 aPosition)
	{
		Vector3 boundPosition = aGameObject.transform.position;
		Bounds bounds = aGameObject.GetComponent<Renderer>().bounds;

		if (Mathf.Abs(aPosition.x - boundPosition.x) < bounds.extents.x)
		{
			if (Mathf.Abs(aPosition.z - boundPosition.z) < bounds.extents.z)
			{
				return true;
			}
		}
		return false;
	}


	public static void RemoveVehicle(GameObject aGameObject)
	{
		vehicles.Remove(aGameObject);
		UnityEngine.Object.Destroy(aGameObject);
	}
	
		
	public static void SetVehiclesActive(bool aState)
	{
		Vehicle component;
		foreach (GameObject vehicle in vehicles)
		{
			if (vehicle == null) continue; // Don't like this, lets test in the future
			component = vehicle.GetComponent<Vehicle>();
			if (component.vehicleInUseData.inUse){
				if (aState) component.AddController(CONTROLLER.User);
				else component.AddController(CONTROLLER.Empty);
			} else vehicle.GetComponent<Vehicle>().SetVehicleActive(aState);
		}		
	}
	
	public static void Reset()
	{
		vehicles.Clear();
	}
	
	public static void Destroy()
	{}
	
}
