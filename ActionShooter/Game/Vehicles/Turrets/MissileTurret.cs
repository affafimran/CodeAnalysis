using UnityEngine;
using System.Collections;

[System.Serializable]
public class MissileTurretData
{
	// MissileTurret specifics
	public GameObject turret, barrel, muzzleFlash1, muzzleFlash2;
	
	public Rigidbody rigidBody;
	public MeshCollider collider;
	
	public AudioSource aimingSound;

	public float idleToAimRange		= 0;
	public float aimSpeed			= 0;
	public float aimToFireAngle		= 0;
	
	public float rof				= 0;
	public float currentRof			= 0;
	
	public string projectile		= "";
	public string muzzleFlash       = "";
	public string sound             = "";
}

public class MissileTurret : Vehicle {
	
	public MissileTurretData missileTurretData;
	
	public override void InitializeSpecific()
	{
		// new MissileTurretData
		missileTurretData = new MissileTurretData ();
		missileTurretData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["TurretSettings"].d[vehicleData.typeSetting].d, missileTurretData) as MissileTurretData;
		missileTurretData.currentRof = missileTurretData.rof;

		// Get turret, barrel and muzzleflash objects
		missileTurretData.turret = transform.Find("MissileTurret/MissileTurretTurret").gameObject;
		missileTurretData.barrel = transform.Find("MissileTurret/MissileTurretTurret/MissileTurretPods").gameObject;
		GameObject muzzleFlash1Dummy = transform.Find("MissileTurret/MissileTurretTurret/MissileTurretPods/MissileTurretMuzzleFlash1").gameObject;
		GameObject muzzleFlash2Dummy = transform.Find("MissileTurret/MissileTurretTurret/MissileTurretPods/MissileTurretMuzzleFlash2").gameObject;
		
		missileTurretData.rigidBody = gameObject.AddComponent<Rigidbody> ();
		missileTurretData.rigidBody.mass = vehicleData.mass;
		missileTurretData.rigidBody.constraints = RigidbodyConstraints.FreezeAll;
		
		missileTurretData.collider = vehicle.GetComponent<MeshCollider> ();
		missileTurretData.collider.convex = true;
		
		// Add the muzzleflash effects to the gameObject
		missileTurretData.muzzleFlash1 = Loader.LoadGameObject("Effects/"+missileTurretData.muzzleFlash+"_Prefab");
		missileTurretData.muzzleFlash1.transform.ResetToParent (muzzleFlash1Dummy);
		missileTurretData.muzzleFlash2 = Loader.LoadGameObject("Effects/"+missileTurretData.muzzleFlash+"_Prefab");
		missileTurretData.muzzleFlash2.transform.ResetToParent (muzzleFlash2Dummy);
		
		// aiming sound
		missileTurretData.aimingSound = Scripts.audioManager.PlaySFX3D("Weapons/Aim", missileTurretData.turret, "Aiming");
		missileTurretData.aimingSound.enabled = false;
	}
	
	// There is no update here. Only the 'Specific'. Of course, for specific stuff.
	protected override void UpdateSpecific()
	{}

}