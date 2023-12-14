using UnityEngine;
using System.Collections;

public class CharacterInputController : MonoBehaviour
{
	//--------------------
	// Input
	//--------------------
	// movement
	public float verAxis = 0f;
	public float horAxis = 0f;

	// actions
	public bool primaryFire = false;
	public bool secondaryFire = false;
	public bool powerOn = false;
	public bool use = false;
	public bool jump = false;
	public bool dive = false;
	public bool bulletTime = false;

	public bool nextPrimary = false; // NOT VERY GOOD NAME
	public bool previousPrimary = false;  // NOT VERY GOOD NAME

	// AI specific (hidden)
	[HideInInspector] public HitData hitData;
	public bool targetInSight = true;
	public bool verticallyInRange; 

	[HideInInspector]
	public int selectedVehicle = 0;
	public bool buildVehicle = false;
	[HideInInspector]

	public void Initialize()
	{
		InitializeSpecific();
	}

	protected virtual void InitializeSpecific()
	{}

	// This is AI
	public virtual Vector3 GetBulletDirection(Vector3 aPosition) { return Vector3.zero; }

	public virtual void Reset()
	{}
}

