using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Explosive class.
/// This is NOT a monobehavior and you have to update it manually. See Hammer.cs for samples.
/// </summary>
[System.Serializable]
public class Explosive
{
	internal GameObject parent; // object that activates/uses the explosive
	internal GameObject camera; // activeCamera reference

	internal ExplosiveData explosiveData; // explosiveData reference

	/// <summary>
	/// Initialize explosive
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public void Initialize(GameObject aParent)
	{
		parent = aParent; // store parent

		// get correct name
		string typeName = this.GetType().Name; 
		string type = typeName.Substring(9, typeName.Length-9);

		explosiveData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Explosives"].d[type].d, new ExplosiveData()) as ExplosiveData; // update/set class
		explosiveData.type = type;
		explosiveData.sourceRof = explosiveData.rof; // store rof 
		explosiveData.unlimitedAmmo = (ShopItemManager.IsBought("ShopItem3")) ? true : explosiveData.unlimitedAmmo; // set unlimited ammo to true if shopitem has been bought

		camera = CameraManager.activeCamera; // camera reference

		InitializeSpecific();// do explosive specific initalizations (could be anything)
	}

	public virtual void InitializeSpecific(){}

	public bool Update(bool aFire, HitData aHitData)
	{
		if (aFire && explosiveData.ammo > 0){ // if firebutton & enough ammo
			explosiveData.rof -= Time.deltaTime; // rate of fire
			if (explosiveData.rof <= 0f){ // do it!
				UpdateEffects();
				UseExplosive(aHitData);
				explosiveData.rof = explosiveData.sourceRof; // reset
				return true;
			}
		} else explosiveData.rof = 0f; // I always set the rof to 0 when you don't fire as I like to fire/shoot/activate as fast as I can click! :)
		return false;
	}

	// relay
	public virtual void UpdateEffects()
	{}

	// relay
	public virtual void UseExplosive(HitData aHitData)
	{
		// Calculate rough position from where to use the explosive
		Vector3 position = (parent.transform.position + Vector3.up) + (parent.transform.forward * 0.3f);

		// Create explosive through projectile manager
		ProjectileManager.AddProjectile(explosiveData.type, position, parent.transform.forward, aHitData, aHitData.gameObject);

		if (!explosiveData.unlimitedAmmo) explosiveData.ammo -= 1; // count ammo
		Scripts.audioManager.PlaySFX3D("Weapons/"+explosiveData.sound, parent, "Weapon"); // sound effect

	}

	// destroy explosive
	public void Destroy(){}
}

