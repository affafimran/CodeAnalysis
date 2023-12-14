using UnityEngine;
using System.Collections;

public class F22Data
{
	// F22 specifics
	public GameObject turret, barrel, mainRotor, tailRotor, F22Trail1, F22Trail2;
	public GameObject muzzleFlash1, muzzleFlash2, muzzleFlash3, muzzleFlash4, patrolObject;
	public float baseHeight = 200f;
	public Vector3 liftOffTarget;
	
	public Rigidbody rigidBody;
	public MeshCollider collider;
	
	public AudioSource flyingSound;

	public float idleToAimRange		= 0;
	public float aimSpeed			= 0;
	public float aimToFireAngle		= 0;
	
	public float rof				= 0;
	public float currentRof			= 0;
	
	public string projectile		= "";
	public string muzzleFlash       = "";
	public string sound				= "";

	public float rof2				= 0;
	public float currentRof2		= 0;
	
	public string projectile2		= "";
	public string muzzleFlashImage2 = "";
	public string sound2			= "";

}

public class F22 : Vehicle {
	
	public F22Data f22Data;
	
	public override void InitializeSpecific()
	{
		// New F22Data
		f22Data = new F22Data ();
		f22Data = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["PlaneSettings"].d[vehicleData.typeSetting].d, f22Data) as F22Data;
		f22Data.currentRof = f22Data.rof;
		f22Data.currentRof2 = f22Data.rof2;

		// Make sure the F22 doesn't start on the ground.
		transform.position = transform.position + new Vector3 (0, 10, 0);
		f22Data.liftOffTarget = transform.position + transform.forward * 300 + new Vector3 (0, f22Data.baseHeight, 0);
		// Left Trail
		f22Data.F22Trail1 = Loader.LoadGameObject("Effects/F22Trail_Prefab");
		f22Data.F22Trail1.transform.ResetToParent(vehicle);
		f22Data.F22Trail1.transform.localPosition = new Vector3 (0.5f, 1, -6);
		f22Data.F22Trail1.transform.Rotate (new Vector3 (0, 90, 0));
		// Right Trail
		f22Data.F22Trail2 = Loader.LoadGameObject("Effects/F22Trail_Prefab");
		f22Data.F22Trail2.transform.ResetToParent(vehicle);
		f22Data.F22Trail2.transform.localPosition = new Vector3 (-0.5f, 1, -6);
		f22Data.F22Trail2.transform.Rotate (new Vector3 (0, 90, 0));
		DeactivateRearThrust ();
		// turret, barrel, muzzleFlashes
		f22Data.turret = transform.Find("F22").gameObject;
		f22Data.barrel = transform.Find("F22/F22Missiles").gameObject;
		GameObject muzzleFlashDummy1 = transform.Find("F22/F22Missiles/F22MuzzleFlash1").gameObject;
		GameObject muzzleFlashDummy2 = transform.Find("F22/F22Missiles/F22MuzzleFlash2").gameObject;
		GameObject muzzleFlashDummy3 = transform.Find("F22/F22Missiles/F22MuzzleFlash3").gameObject;
		GameObject muzzleFlashDummy4 = transform.Find("F22/F22Missiles/F22MuzzleFlash4").gameObject;
		
		f22Data.rigidBody = gameObject.AddComponent<Rigidbody> ();
		f22Data.rigidBody.mass = vehicleData.mass;
		f22Data.rigidBody.drag = 0;
		f22Data.rigidBody.angularDrag = 0;
		f22Data.rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		
		f22Data.collider = vehicle.GetComponent<MeshCollider> ();
		f22Data.collider.convex = true;
		
		// Add the muzzleFlashs to the gameObject
		f22Data.muzzleFlash1 = Loader.LoadGameObject("Effects/"+f22Data.muzzleFlash+"_Prefab");
		f22Data.muzzleFlash1.transform.ResetToParent (muzzleFlashDummy1);
		f22Data.muzzleFlash2 = Loader.LoadGameObject("Effects/"+f22Data.muzzleFlash+"_Prefab");
		f22Data.muzzleFlash2.transform.ResetToParent (muzzleFlashDummy2);
		f22Data.muzzleFlash3 = Loader.LoadGameObject("Effects/"+f22Data.muzzleFlash+"_Prefab");
		f22Data.muzzleFlash3.transform.ResetToParent (muzzleFlashDummy3);
		f22Data.muzzleFlash4 = Loader.LoadGameObject("Effects/"+f22Data.muzzleFlash+"_Prefab");
		f22Data.muzzleFlash4.transform.ResetToParent (muzzleFlashDummy4);
		
		// Play the constant SFX sound the unit will have
		f22Data.flyingSound = Scripts.audioManager.PlaySFX3D("Vehicles/F22Loop", gameObject, "Aiming");
		f22Data.flyingSound.enabled = false;
	}
		
	// There is no update here. Only the 'Specific'. Of course, for specific stuff.
	protected override void UpdateSpecific()
	{}
	
	// Activate the Thrust particle effects
	public void ActivateRearThrust()
	{
		f22Data.F22Trail1.transform.Find("MuzzleFlashRear").GetComponent<ParticleSystem>().enableEmission = true;
		f22Data.F22Trail1.transform.Find("Trail").GetComponent<ParticleSystem>().enableEmission = true;
		f22Data.F22Trail2.transform.Find("MuzzleFlashRear").GetComponent<ParticleSystem>().enableEmission = true;
		f22Data.F22Trail2.transform.Find("Trail").GetComponent<ParticleSystem>().enableEmission = true;
	}
	
	// Deactivate the Thrust particle effects
	public void DeactivateRearThrust()
	{
		f22Data.F22Trail1.transform.Find("MuzzleFlashRear").GetComponent<ParticleSystem>().enableEmission = false;
		f22Data.F22Trail1.transform.Find("Trail").GetComponent<ParticleSystem>().enableEmission = false;
		f22Data.F22Trail2.transform.Find("MuzzleFlashRear").GetComponent<ParticleSystem>().enableEmission = false;
		f22Data.F22Trail2.transform.Find("Trail").GetComponent<ParticleSystem>().enableEmission = false;
	}
}