using UnityEngine;
using System.Collections;

/// <summary>
/// Target manager.
/// <para>Has one function now. Get 'proper' target data from Hammer to object requesting the data.</para>
/// </summary>
public static class TargetManager
{
	/// <summary>
	/// Initialize the TargetManager.
	/// <para>There is nothing to initialize here yet.</para>
	/// </summary>
	public static void Initialize()
	{}

	/// <summary>
	/// Get the target data.
	/// NOTE: THIS IS HARDCODED TO HAMMER NOW!
	/// In the future you can/might sort this on multiple characters and return the closest/best one.
	/// </summary>
	/// <returns>The target data.</returns>
	/// <param name="aGameObject">A game object.</param>
	public static TargetData GetTargetData(GameObject aGameObject)
	{
		TargetData targetData = new TargetData(); // new class
		Hammer hammer = Scripts.hammer; // [HARDCODED] Hammer as target
		if (hammer == null) return targetData;

		targetData.target = true;// we have a target
		targetData.gameObject = Scripts.hammer.gameObject; // gameObject reference
		if (hammer.vehicleData.isInVehicle) targetData.gameObject = hammer.vehicleData.vehicle; // reference when in vehicle

		targetData.position = targetData.gameObject.transform.position; // actual position
		targetData.distance = Vector3.Distance(targetData.position, aGameObject.transform.position); // distance

		Vector3 positionLevel = new Vector3(targetData.position.x, aGameObject.transform.position.y, targetData.position.z);
		targetData.distanceLeveled = Vector3.Distance(positionLevel, aGameObject.transform.position); // distance leveled

		targetData.verticalOffset = aGameObject.transform.position.y - targetData.position.y; // vertical offset

		return targetData;
	}
}