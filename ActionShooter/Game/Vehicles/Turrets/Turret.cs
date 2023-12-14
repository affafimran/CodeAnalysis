using UnityEngine;
using System.Collections;

[System.Serializable]
public class TurretData 
{
	// Turret specifics
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

public class Turret : Vehicle {
	
	public TurretData turretData;
	
	public override void InitializeSpecific()
	{
		// new TurretData
		turretData = new TurretData ();
		turretData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["TurretSettings"].d[vehicleData.typeSetting].d, turretData) as TurretData;
		turretData.currentRof = turretData.rof;
		
		// Get turret, barrel and muzzleflash objects
		turretData.turret = transform.Find("Turret/TurretTurret").gameObject;
		turretData.barrel = transform.Find("Turret/TurretTurret/TurretGuns").gameObject;
		GameObject muzzleFlash1Dummy = transform.Find("Turret/TurretTurret/TurretGuns/TurretMuzzleFlash1").gameObject;
		GameObject muzzleFlash2Dummy = transform.Find("Turret/TurretTurret/TurretGuns/TurretMuzzleFlash2").gameObject;
		
		turretData.rigidBody = gameObject.AddComponent<Rigidbody> ();
		turretData.rigidBody.mass = vehicleData.mass;
		turretData.rigidBody.constraints = RigidbodyConstraints.FreezeAll;
		
		turretData.collider = vehicle.GetComponent<MeshCollider> ();
		turretData.collider.convex = true;
		
		// Add the muzzleflash effects to the gameObject
		turretData.muzzleFlash1 = Loader.LoadGameObject ("Effects/" + turretData.muzzleFlash + "_Prefab");
		turretData.muzzleFlash1.transform.ResetToParent(muzzleFlash1Dummy);
		turretData.muzzleFlash2 = Loader.LoadGameObject ("Effects/" + turretData.muzzleFlash + "_Prefab");
		turretData.muzzleFlash2.transform.ResetToParent(muzzleFlash2Dummy);
		
		// aiming sound
		turretData.aimingSound = Scripts.audioManager.PlaySFX3D("Weapons/Aim", turretData.turret, "Aiming");
		turretData.aimingSound.enabled = false;
	}
	
	// There is no update here. Only the 'Specific'. Of course, for specific stuff.
	protected override void UpdateSpecific()
	{}
}