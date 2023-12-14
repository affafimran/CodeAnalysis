using UnityEngine;
using System.Collections;

[System.Serializable]
public class HelicopterData
{
	// Helicopter specifics
	public GameObject body, barrel, mainRotor, tailRotor, muzzleFlash1, muzzleFlash2;
	public float baseHeight = 25, rotorSpeed = 0;
	
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
	
	public string projectile		= "";
	public string muzzleFlash       = "";
	public string sound             = "";
}

public class Helicopter : Vehicle {
	
	public HelicopterData helicopterData;


	private float maxSpeed = 50f;
	private Vector3 accelerationMultiplier = new Vector3(1.4f, 1.4f, 1.4f);
	private Vector3 dragMultiplier = new Vector3(0.9f, 0.95f, 0.9f);
	
	private float maxPitchAngle = 25f;
	private float maxRollAngle  = 25f; 

		
	public override void InitializeSpecific()
	{
		// New HelicopterData & some updates
		helicopterData = new HelicopterData();
		helicopterData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["HelicopterSettings"].d[vehicleData.typeSetting].d, helicopterData) as HelicopterData;
		helicopterData.currentRof = helicopterData.rof;

		// Add rigidbody and collider
		helicopterData.rigidBody = gameObject.AddComponent<Rigidbody> ();
		helicopterData.rigidBody.mass = vehicleData.mass;
		helicopterData.rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		//UnityEditorInternal.ComponentUtility.MoveComponentUp(helicopterData.rigidBody);
		
		helicopterData.collider = vehicle.GetComponent<MeshCollider> ();
		helicopterData.collider.convex = true;

		// Get turret, barrel, muzzleFlashs, mainRotor and the tailRotor
		helicopterData.body = transform.Find("Helicopter").gameObject;
		helicopterData.barrel = transform.Find("Helicopter/HelicopterGuns").gameObject;
		helicopterData.mainRotor = transform.Find("Helicopter/HelicopterMainRotor").gameObject;
		helicopterData.tailRotor = transform.Find("Helicopter/HelicopterTailRotor").gameObject;

		// Add the muzzleflashs to the gameObject
		GameObject muzzleFlashDummy1 = transform.Find("Helicopter/HelicopterGuns/HelicopterMuzzleFlash1").gameObject;
		GameObject muzzleFlashDummy2 = transform.Find("Helicopter/HelicopterGuns/HelicopterMuzzleFlash2").gameObject;
		helicopterData.muzzleFlash1 = Loader.LoadGameObject("Effects/"+helicopterData.muzzleFlash+"_Prefab");
		helicopterData.muzzleFlash1.transform.ResetToParent (muzzleFlashDummy1);
		helicopterData.muzzleFlash2 = Loader.LoadGameObject( "Effects/"+helicopterData.muzzleFlash+"_Prefab");
		helicopterData.muzzleFlash2.transform.ResetToParent (muzzleFlashDummy2);
		
		// Play the constant SFX sound the unit will have
		helicopterData.rotorSound = Scripts.audioManager.PlaySFX3D("Vehicles/HelicopterLoop", vehicle, "Aiming");
		helicopterData.rotorSound.enabled = false;
	}
	
	// There is no update here. Only the 'Specific'. Of course, for specific stuff.
	protected override void UpdateSpecific()
	{
		// Rotate mainRotor
		helicopterData.mainRotor.transform.Rotate(new Vector3(0, helicopterData.rotorSpeed * Time.deltaTime, 0));
		// Rotate tailRotor
		helicopterData.tailRotor.transform.Rotate(new Vector3(helicopterData.rotorSpeed*2 * Time.deltaTime, 0, 0));


		// Movement
		Vector3 velocity = gameObject.transform.InverseTransformDirection(helicopterData.rigidBody.velocity);
		float speedDifference;
		
		Vector3 force;
		Vector3 drag;
		
		// X: left/right
		if (controller.horAxis > 0){
			speedDifference = maxSpeed - velocity.x; // forward speed difference
			force = Vector3.right * (controller.horAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
			helicopterData.rigidBody.AddRelativeForce(force);
		} else if (controller.horAxis < 0) {
			speedDifference = -maxSpeed - velocity.x; // forward speed difference
			force = Vector3.left * (controller.horAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
			helicopterData.rigidBody.AddRelativeForce(force);
		}
		
		// Y: up/down
		if (controller.up){
			speedDifference = maxSpeed - velocity.y; // upward speed difference
			force = Vector3.up * (helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
			helicopterData.rigidBody.AddRelativeForce(force);
		}
		
		if (controller.down) {
			speedDifference = -maxSpeed - velocity.y; // upward speed difference
			force = Vector3.up * (helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
			helicopterData.rigidBody.AddRelativeForce(force);
		}
		
		// Z: forward/backward
		if (controller.verAxis > 0){
			speedDifference = maxSpeed - velocity.z; // forward speed difference
			force = Vector3.forward * (controller.verAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
			helicopterData.rigidBody.AddRelativeForce(force);
		} else if (controller.verAxis < 0) {
			speedDifference = -maxSpeed - velocity.z; // forward speed difference
			force = Vector3.back * (controller.verAxis * helicopterData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
			helicopterData.rigidBody.AddRelativeForce(force);
		}
		
		// Apply drag in all positions.
		// You can also opt to get the new velocity, invert it and have one force to add
		// But I would like to have different drag settings per axis.
		
		// left/right
		drag    = Vector3.left * (velocity.x * dragMultiplier.x * helicopterData.rigidBody.mass);
		helicopterData.rigidBody.AddRelativeForce(drag);
		
		// up/down
		drag    = Vector3.down * (velocity.y * dragMultiplier.y * helicopterData.rigidBody.mass);
		helicopterData.rigidBody.AddRelativeForce(drag);
		
		// forward/backward
		drag    = Vector3.back * (velocity.z * dragMultiplier.z * helicopterData.rigidBody.mass);
		helicopterData.rigidBody.AddRelativeForce(drag);
		
		// pitch and roll, based upon speed
		float roll  = Mathf.Max (-1f, -velocity.x/maxSpeed);
		float pitch = Mathf.Min(1f, velocity.z/maxSpeed);
		Vector3 localEuler = helicopterData.body.transform.localEulerAngles;
		helicopterData.body.transform.localEulerAngles = new Vector3(maxPitchAngle*pitch, localEuler.y, maxRollAngle*roll);
		
		// sound update
		if (controller.type != "Empty") {
			float perc = velocity.magnitude/maxSpeed;
			helicopterData.rotorSound.pitch = Mathf.Min(1f, 0.6f + perc);
			if (controller.type == "User") helicopterData.rotorSpeed = 1200; //Mathf.Min(1200, 700 + (1200*perc));
		}


	}

	public void Aim(Vector3 targetPosition) 
	{
		// aim muzzleflashes! funny? ;) 
		helicopterData.muzzleFlash1.transform.LookAt(targetPosition);
		helicopterData.muzzleFlash2.transform.LookAt(targetPosition);
		
		// update barrel for visual effect as wel
		Vector3 euler = new Vector3(helicopterData.muzzleFlash1.transform.eulerAngles.x, 0, 0);
		helicopterData.barrel.transform.localEulerAngles = euler;
	}

	public void Fire(GameObject aMuzzleFlash) {Fire (aMuzzleFlash, null);}
	public void Fire(GameObject aMuzzleFlash, GameObject aTarget)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(helicopterData.sound, helicopterData.barrel, "FireGun");
		ProjectileManager.AddProjectile (helicopterData.projectile, aMuzzleFlash.transform.position, aMuzzleFlash.transform.forward, new HitData (), new HitData().gameObject);
		aMuzzleFlash.PlayInChildren();
	}

	protected override void EnterVehicleSpecific(){
		helicopterData.body.transform.localRotation = Quaternion.identity;
	}
}