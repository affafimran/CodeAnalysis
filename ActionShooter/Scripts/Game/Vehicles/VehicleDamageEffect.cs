using UnityEngine;
using System.Collections;

public class VehicleDamageEffect : MonoBehaviour
{
	private Vehicle vehicle;
	private VehicleData vehicleData;

	private GameObject referenceObject;
	private GameObject smoke, fire;
	private bool onFire;

	public float damagePerSecondWhenOnFire = 1f;
	private float damageTimer = 1f;

	private AudioSource fireAudio;

	public void Initialize(Vehicle aVehicle)
	{
		vehicle = aVehicle;
		vehicleData = vehicle.vehicleData;
		vehicleData.damageEffect = true;

		// get referenceObject to parent on
		string name = vehicleData.prefab+"Smoke";
		referenceObject = GenericFunctionsScript.FindChild(gameObject, name);
		// Create smoke
		AddSmokeOrFire("Smoke");
	}

	void Update()
	{
		if(Data.pause) return;
		if (vehicleData.health <= 5.0f && !onFire) AddSmokeOrFire("Fire");
		if (onFire) {
			damageTimer -= Time.deltaTime;
			if (damageTimer <= 0f){
				damageTimer = 1f;
				ProjectileData projectileData = new ProjectileData();
				projectileData.damage = 1f;
				vehicle.Damage(projectileData, new HitData());
			}
		}
	}

	void AddSmokeOrFire(string aType)
	{
		switch(aType)
		{
			case "Smoke":
				smoke = Loader.LoadGameObject("Effects/VehicleSmoke_prefab");
				smoke.transform.ResetToParent(referenceObject);
				smoke.AddComponent<ParticleOffset>();	
				break;
			case "Fire":
				fire = Loader.LoadGameObject("Effects/VehicleFire_prefab");
				fire.transform.ResetToParent(referenceObject);
				fireAudio = Scripts.audioManager.PlaySFX3D ("Effects/FireLoop", gameObject, "Medium");
				fire.AddComponent<ParticleOffset>();
				onFire = true;
				break;
		}
	}
	
	public void Destroy()
	{
		// remove visual
		smoke.StopInChildren(); // stop emitting
		Destroy (smoke, 2.5f); // give time to fade out and then remove
		if (onFire) Destroy(fire); // instakill
		Destroy (fireAudio);
		Destroy(this);
	}
}
