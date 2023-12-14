using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pick up.
/// </summary>
public class PickUp : MonoBehaviour {

	public bool pickUpActive = false; // is active
	public string pickUp = "None"; // type (real name, without PickUp)
	public PickUpData pickUpData; // this pickups data

	// private vars to animate & pickup & etc.
	private float deltaTime; // reference
	private float radius; // radius to pick it up
	private TargetData targetData; // target data

	// falling pickups (default false)
	private bool fallingPickup = false; // is this pickup falling?
	private Vector3 targetPosition = Vector3.zero; // position it will land
	private float gravity = 0f; // fake/simple gravity

	private float magnetProgression = 0f; // influence from magnet

	/// <summary>
	/// Initialize a pickup.
	/// <para>NEeds PickUpData, a position and if it is falling (this could be removed or through an overload)</para>
	/// </summary>
	/// <param name="aPickUpData">A pick up data.</param>
	/// <param name="aPosition">A position.</param>
	/// <param name="aFallingPickup">If set to <c>true</c> a falling pickup.</param>
	public void Initialize(PickUpData aPickUpData, Vector3 aPosition, bool aFallingPickup)
	{
		pickUpData = aPickUpData; // assign data
		pickUp = pickUpData.prefab.Substring(6, pickUpData.prefab.Length-6); // substring (no _Prefab) the actual name of the pickup
		pickUpActive = true; // activate

		radius = Mathf.Max(0.5f, gameObject.GetComponent<MeshRenderer>().bounds.min.sqrMagnitude); // calculate a rough radius

		gameObject.layer = PickUpManager.pickupLayer; // assign layer

		fallingPickup = aFallingPickup; // falling?

		// Position
		Vector3 rayPosition = aPosition + new Vector3(0, 0.5f, 0);
		if (fallingPickup) rayPosition = aPosition + new Vector3(0, 100, 0);

		// Raycast, result, etc.
		Ray tRay    = new Ray(rayPosition, new Vector3(0, -1, 0));
		RaycastHit tHitData;
		LayerMask  tLayerMask =  (1 << 0) | (1 << 13) | (1 << 14);
		bool tHitResult = Physics.Raycast(tRay, out tHitData, 200, tLayerMask);
		float tOffset = (Mathf.Sqrt(radius) * 1.0f + 0.02f); // calc rough offset to position pickup from floor or object
			
		// position pickup
		if (tHitResult)	{
			if (fallingPickup){ // if falling store target position and set pickup on spawnpos
				gameObject.transform.position = aPosition;
				targetPosition = tHitData.point + new Vector3(0, tOffset, 0);
			} else gameObject.transform.position =  tHitData.point + new Vector3(0, tOffset, 0); // set on raycastposition with offset
		} else gameObject.transform.position = aPosition + new Vector3(0, tOffset, 0); // set on spawnposition with offset.

		radius = radius * 2.5f; // enlarge the radius used for picking up this pickup. Could be better...
	}

	void Update()
	{
		if (Data.pause || !pickUpActive) return; // return if paused or pickup is inactive

		deltaTime = Time.deltaTime; // store

		// look at camera
		gameObject.transform.LookAt(CameraManager.activeCamera.transform.position, Vector3.up);

		// animate if falling
		if (fallingPickup){
			gravity = Mathf.Min(20f, 1f + (gravity*1.1f)); // fake increasing gravity
			gameObject.transform.Translate(0, 0, -gravity * deltaTime); // move pickup
			if (gameObject.transform.position.y <= targetPosition.y){ // stop when reaching the final position
				gameObject.transform.position = targetPosition;
				fallingPickup = false;
			}
		}

		// pickup
		if (pickUpActive){
			targetData = TargetManager.GetTargetData(gameObject); // update targetdata
			if (targetData.distance <= radius){ // in range
				PickUpManager.PickUpReward(targetData.gameObject, pickUp, pickUpData); // pickup and award
				DestroyPickup(true); // destroy this with an effect
			} else if (PickUpManager.pickUpMagnetActive && targetData.distance < 50.0f){ // If in magnet range.
				magnetProgression += deltaTime;
				gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetData.position, magnetProgression*magnetProgression);
				if (magnetProgression >= 1.0){ // forced pick up
					PickUpManager.PickUpReward(targetData.gameObject, pickUp, pickUpData); // pickup and award
					DestroyPickup(true); // destroy this with an effect
				}
			}
		}

		// timer, destroy when lifetime is over
		if (pickUpData.lifeTime > 0){
			pickUpData.lifeTime -= deltaTime;
			if (pickUpData.lifeTime <= 0) DestroyPickup(false);
		}
	}

	void DestroyPickup(bool anEffect)
	{
		// Visual Effect
		if (pickUpData.effect != "None" && anEffect){
			GameObject tEffect = Loader.LoadGameObject("Effects/" + pickUpData.effect + "_PS");
			tEffect.transform.position = gameObject.transform.position;
			Destroy(tEffect, 1.2f);	
		}

		PickUpManager.pickUps.Remove(gameObject); // remove from list
		Destroy(gameObject); // destroy the object
	}
}

	