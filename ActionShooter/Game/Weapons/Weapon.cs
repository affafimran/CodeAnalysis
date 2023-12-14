using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Weapon base class.
/// </summary>
[System.Serializable]
public class Weapon
{
	internal CharacterData characterData; // characterData who wields this weapon
	internal GameObject parent; // object from characterData
	internal Animator animator; // animator reference

	internal GameObject camera; // cameraReference

	internal GameObject weaponModel; // actual weapon model

	internal GameObject muzzleObject; // muzzleFlashObject
	internal ParticleSystem[] muzzleFlashEmitters; // all emitters for effects (should be removed and use PlayInChildren on the main muzzleObject)
	internal bool muzzleFlashAvailable = true; // do we have muzzleflashes
	
	internal GameObject shellObject; // shell object (holds the particle system for shells)
	internal ParticleSystem[] shellEmitters; // all emitters for effects (should be removed and use PlayInChildren on the main shellObject)
	internal bool shellAvailable = true; // do we have shells

	internal Dictionary<string, DicEntry> weaponData;
	internal Vector3 direction;
	internal float accuracyAngle = 35f;

	internal bool initialized = false;

	public string type        = "None";
	public string prefab      = "None";
	internal bool searchModel = false;
	public float rof          = 0.1f;
	internal float sourceRof;
	internal float rofTime;
	public string projectile  = "DefaultBullet";
	public int amount         = 1;
	public int burst		  = 1;
	internal int sourceBurst  = 1;
	internal float burstRof   = 0.1f;
	internal bool burstFire   = false;
	public float accuracy     = 1.0f;
	public float recoil       = 0f;
	public float shake        = 1f;
	public int ammo           = 30;
	public bool unlimitedAmmo = false;
	public string muzzleFlash = "";
	public string shell       = "";
	public string sound       = "Default";

	public void Initialize(CharacterData aCharacterData, GameObject aParent, Animator anAnimator)
	{
		// store characterData, parent and animator
		characterData = aCharacterData;
		parent = aParent; 
		animator = anAnimator;

		type = this.GetType().Name; // store weapon type/name by class name
		if (type == "EnemyWeapon") type = characterData.weapon; // unless it is EnemyWeapon then override from characterData

		// Ideally we should use a WeaponData class and parse the SharedData.txt dictionary to this class.
		// On hold for now. TBD

		weaponData = Data.Shared["Weapons"].d[type].d;
		prefab = weaponData["Prefab"].s;
		if (prefab == "ModelAvailable"){prefab = characterData.prefab+"Weapon"; searchModel = true;} // enemycharacters have their weapon already available in their prefab
		rof = weaponData["Rof"].f;
		sourceRof = rof;
		projectile = weaponData["Projectile"].s;
		amount = weaponData["Amount"].i; // amount of projectiles per shot e.g. shotgun has more than pistol

		if (weaponData.ContainsKey("Burst")) burst = weaponData["Burst"].i;
		sourceBurst = burst;
		burstRof = sourceRof;
		if (weaponData.ContainsKey("BurstRof")) burstRof = weaponData["BurstRof"].f;

		accuracy = weaponData["Accuracy"].f;
		recoil = weaponData["Recoil"].f; // unused
		shake = weaponData["Shake"].f;
		ammo = weaponData["Ammo"].i;
		unlimitedAmmo = (ShopItemManager.IsBought("ShopItem2")) ? true : weaponData["UnlimitedAmmo"].b; // not too happy with this

		muzzleFlash = weaponData["MuzzleFlash"].s;
		shell = weaponData["Shell"].s;
		sound = weaponData["Sound"].s;

		if (prefab == "Magnum") type = "DesertEagle"; 		// Last Minute [HACK] to implement the golden gun

		camera = CameraManager.activeCamera; // camera reference

		// get hand holding gun (sorry for the exceptions)
		string stringHand = characterData.prefab + " R Hand";
		if (characterData.prefab == "EnemyJuggernaut" || characterData.prefab == "EnemyBigBoss") stringHand = characterData.prefab + " L Hand"; 
		GameObject tHand = GenericFunctionsScript.FindChild(parent, stringHand);

		// get or load weaponModel
		if (searchModel) weaponModel = GenericFunctionsScript.FindChild(tHand, prefab); // see above
		else weaponModel = Loader.LoadGameObject("Weapons/"+prefab+"_Prefab");

		// muzzleflashes
		Transform tMuzzleObject = weaponModel.transform.Find(prefab+"Fire1_Dummy");
		if (tMuzzleObject == null) muzzleFlashAvailable = false; // no flashes
		else{ // store all ps references
			muzzleObject = tMuzzleObject.gameObject;
			GameObject tMuzzleFlash = Loader.LoadGameObject("Effects/"+muzzleFlash+"MuzzleFlash_Prefab");
			tMuzzleFlash.transform.ResetToParent(muzzleObject);
			muzzleFlashEmitters = tMuzzleFlash.GetComponentsInChildren<ParticleSystem>();
		}

		// shell
		Transform tShellObject = weaponModel.transform.Find(prefab+"Shell1_Dummy");
		if (tShellObject == null || shell == "None") shellAvailable = false; // no shells
		else{ // store all ps references
			shellObject  = tShellObject.gameObject;
			GameObject tShell = Loader.LoadGameObject("Effects/"+shell+"Shell_Prefab");
			tShell.transform.ResetToParent(shellObject);
			shellEmitters = tShell.GetComponentsInChildren<ParticleSystem>();		
		}

		// link weapon to hand
		if (weaponData["Prefab"].s != "ModelAvailable"){
			weaponModel.transform.parent = tHand.transform;
			weaponModel.transform.localPosition = Vector3.zero;
			weaponModel.transform.localRotation = Quaternion.identity;
		}


		if (!characterData.AI) SetWeaponActive(true); // set correct animation if not AI (they only have one weapon and one set of animation)

		InitializeSpecific(); // initialize anythign specific on the weapon

		initialized = true; // we're initialized
	}


	public virtual void InitializeSpecific(){}

	/// <summary>
	/// Sets the weapon active.
	/// </summary>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public void SetWeaponActive(bool aState)
	{
		weaponModel.SetActive(aState);
		if (animator.isActiveAndEnabled) animator.SetBool(type, aState);
	}

	/// <summary>
	/// Resets this weapon.
	/// <para>This resets ammo!</para>
	/// </summary>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public void ResetWeapon(bool aState)
	{
		// only ammo for now?
		if (aState == true)	ammo = weaponData["Ammo"].i; // reset ammo to default value
		else ammo += weaponData["Ammo"].i; // add more ammo. This is wat we use!

	}

	public Vector3 bulletOrigin { get { return muzzleObject.transform.position; } }
	public Vector3 bulletDirection { get { return muzzleObject.transform.forward; } }

	/// <summary>
	/// Update this weapon
	/// </summary>
	/// <param name="aFire">If set to <c>true</c> a fire.</param>
	/// <param name="aHitData">A hit data.</param>
	public bool Update(bool aFire, HitData aHitData)
	{
		if ((aFire || burstFire) && ammo > 0 && aHitData != null){ // don't like checking for aHitData (TBD)

			// [HACK] for rapidfire (TBD)
			rofTime = Time.deltaTime;
			if (WeaponManager.rapidFire && (!characterData.AI)) rofTime = rofTime* 4.0f; // Rapid fire should only work on the Hammer.
			rof -= rofTime; // Added this for extreme firing.

			// fire
			if (rof <= 0f){
				// get direction
				if (aHitData.result) direction = (aHitData.position - muzzleObject.transform.position).normalized;
				else direction = camera.transform.forward;

				// [HACK] to always should forward when in reverse view!
				if (!characterData.AI && CameraManager.activeCameraData.settings == "HammerReverse") direction = -muzzleObject.transform.right; 

				animator.SetBool("Fire1", true); // set animator
				animator.Play(0, 1, 0f); // reset the animation! so firing is always 'snappy'

				UpdateEffects(); // update effects
				FireWeapon(aHitData); // fire weapon

				// burstfire (see SharedData.txt - Characters - Swat for sample)
				burstFire = true;
				burst -= 1;
				if (burst <= 0){
					burst = sourceBurst; 
					burstFire = false;
					rof = sourceRof;
				} else rof = burstRof;

				return true; // return that we fired
			}
		
		} else{
			animator.SetBool("Fire1", false); // reset animator
			if (!characterData.AI) rof = 0f; // we reset this, because as an user I want to shoot instantly again
		}
		return false; // return we didn't fire
	}

	// emit effects
	public virtual void UpdateEffects()
	{
		// muzzle flashes
		if (muzzleFlashAvailable){
			foreach(ParticleSystem ps in muzzleFlashEmitters)
				ps.Play();
		}

		// shell
		if (shellAvailable){
			foreach(ParticleSystem ps in shellEmitters)
				ps.Emit(1);
		}

		// Shake
		if (!characterData.AI) CameraManager.activeCameraData.shakeIntensity = shake;
	}

	public virtual void FireWeapon(HitData aHitData)
	{
		// correct accuracy (to make sure the values in the editor stay comprehensible)
		float correctAccuracy = Mathf.Max(0.01f, accuracy);
		correctAccuracy = Mathf.Max(0f, 1.0f - correctAccuracy);

		// amount of projectiles
		int projectiles = amount;
		if (projectiles > ammo && !unlimitedAmmo) projectiles = ammo;
		// amount of bullets
		for (int i = 1; i <= projectiles; i++) {
			// accuracy/offset
			Vector3 finalDirection = Quaternion.AngleAxis(Random.Range(-accuracyAngle, accuracyAngle)*correctAccuracy, Random.onUnitSphere) * direction;
			//projectile
			ProjectileManager.AddProjectile(projectile, muzzleObject.transform.position, finalDirection, aHitData, aHitData.gameObject);
		}

		if (!unlimitedAmmo) ammo -= 1; // count ammo. Single shot now instead of counting all actual bullets

		Scripts.audioManager.PlaySFX3D("Weapons/"+sound, muzzleObject, "Weapon"); // sound effect
	}

	// destroy weapon
	public void Destroy()
	{
		animator.SetBool("Fire1", false); // reset animation
		if (!characterData.AI) animator.SetBool(type, false); // reset weapon animation

		weaponModel.transform.parent = null; // unlink
		weaponModel.AddComponent<BoxCollider>(); // add collider
		weaponModel.AddComponent<Rigidbody>(); // add rb

		// throw weapon away
		weaponModel.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 500f);
		weaponModel.GetComponent<Rigidbody>().AddForce(camera.transform.right * 150f);

		// destroy model in x sec.
		UnityEngine.Object.Destroy(weaponModel, 3f);
	}
}

