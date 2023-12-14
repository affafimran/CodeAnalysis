using UnityEngine;
using System.Collections;

[System.Serializable]
public class ApacheData
{
	// Apache specifics
	public GameObject body, gun, mainRotor, tailRotor, rocketLauncher;
	public GameObject muzzleFlash5, muzzleFlash1, muzzleFlash2, muzzleFlash3, muzzleFlash4;
	public GameObject[] launchers;
	public string projectile2;
	public string muzzleFlashImage2;
	public float baseHeight = 25f;
	public float rotorSpeed = 0f;
	
	public Rigidbody rigidBody;
	public MeshCollider collider;
	
	public AudioSource rotorSound;


	// AI
	public float idleToAimRange		= 0f; // AI!
	public float aimSpeed			= 0f; // AI!
	public float aimToFireAngle		= 0f; // AI!
	
	// Weapons/firing and stuff
	public float rof				= 0f; // rate of fire
	public float currentRof			= 0f;

	public float rof2				= 0f; // rate of fire
	public float currentRof2		= 0f;
	
	public string projectile		= "";
	public string muzzleFlash       = "";
	public string sound             = "";

}

public class Apache : Vehicle {
	
	public ApacheData apacheData;
	
	public override void InitializeSpecific()
	{
		// new ApacheData
		apacheData = new ApacheData();
		apacheData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["HelicopterSettings"].d[vehicleData.typeSetting].d, apacheData) as ApacheData;
		apacheData.currentRof = apacheData.rof;
		
		//apacheData.projectile2			= Data.Shared["Units"].d[unitData.prefab].d["Projectile2"].s;
		//apacheData.muzzleFlashImage2	= Data.Shared["Units"].d[unitData.prefab].d["MuzzleFlash2"].s;
		
		// Get turret, barrel, MuzzleflashBarrel, Launcher, MuzzleflashsLauncher, mainRotor and tailRotor
		apacheData.body = transform.Find("Apache").gameObject;
		apacheData.gun = transform.Find("Apache/ApacheGun").gameObject;
		apacheData.rocketLauncher = transform.Find("Apache/ApacheMissilePods").gameObject;
		apacheData.mainRotor = transform.Find("Apache/ApacheMainRotor").gameObject;
		apacheData.tailRotor = transform.Find("Apache/ApacheTailRotor").gameObject;
		GameObject muzzleFlashDummy = transform.Find("Apache/ApacheGun/ApacheMuzzleFlash5").gameObject;
		GameObject muzzleFlashDummy1 = transform.Find("Apache/ApacheMissilePods/ApacheMuzzleFlash1").gameObject;
		GameObject muzzleFlashDummy2 = transform.Find("Apache/ApacheMissilePods/ApacheMuzzleFlash2").gameObject;
		GameObject muzzleFlashDummy3 = transform.Find("Apache/ApacheMissilePods/ApacheMuzzleFlash3").gameObject;
		GameObject muzzleFlashDummy4 = transform.Find("Apache/ApacheMissilePods/ApacheMuzzleFlash4").gameObject;
		
		apacheData.launchers = new GameObject[4]{muzzleFlashDummy1, muzzleFlashDummy2, muzzleFlashDummy3, muzzleFlashDummy4};
		
		apacheData.rigidBody = gameObject.AddComponent<Rigidbody> ();
		apacheData.rigidBody.mass = vehicleData.mass;
		apacheData.rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		
		apacheData.collider = vehicle.GetComponent<MeshCollider> ();
		apacheData.collider.convex = true;
		
		// Add the muzzleflashs to the barrel and launchers
		apacheData.muzzleFlash5 = Loader.LoadGameObject("Effects/"+apacheData.muzzleFlash+"_Prefab");
		apacheData.muzzleFlash5.transform.ResetToParent (muzzleFlashDummy);
		apacheData.muzzleFlash1 = Loader.LoadGameObject("Effects/"+apacheData.muzzleFlash+"_Prefab");
		apacheData.muzzleFlash1.transform.ResetToParent (muzzleFlashDummy1);
		apacheData.muzzleFlash2 = Loader.LoadGameObject("Effects/"+apacheData.muzzleFlash+"_Prefab");
		apacheData.muzzleFlash2.transform.ResetToParent (muzzleFlashDummy2);
		apacheData.muzzleFlash3 = Loader.LoadGameObject("Effects/"+apacheData.muzzleFlash+"_Prefab");
		apacheData.muzzleFlash3.transform.ResetToParent (muzzleFlashDummy3);
		apacheData.muzzleFlash4 = Loader.LoadGameObject("Effects/"+apacheData.muzzleFlash+"_Prefab");
		apacheData.muzzleFlash4.transform.ResetToParent (muzzleFlashDummy4);
		
		// Play the constant SFX sound the unit will have
		apacheData.rotorSound = Scripts.audioManager.PlaySFX3D("Vehicles/HeavyHelicopterLoop", vehicle, "Aiming");
		apacheData.rotorSound.enabled = false;
		
	}
	
	// There is no update here. Only the 'Specific'. Of course, for specific stuff.
	protected override void UpdateSpecific()
	{
		// Rotate the main rotor
		apacheData.mainRotor.transform.Rotate(new Vector3(0, apacheData.rotorSpeed, 0));
		// Rotate the tail rotor
		apacheData.tailRotor.transform.Rotate(new Vector3(apacheData.rotorSpeed*2, 0, 0));
	}

	protected override void EnterVehicleSpecific(){
		apacheData.body.transform.localRotation = Quaternion.identity;
	}

}