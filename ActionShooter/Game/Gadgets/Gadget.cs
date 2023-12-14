using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gadget base class.
/// </summary>
[System.Serializable]
public class Gadget
{
	public string type       = "Default"; // type of gadget
	public bool gadgetActive = false; // active
	public int activationTime = 0; // store the time of activation (for whatever reason you see fit.

	public void Initialize(CharacterData aCharacterData, Weapon[] aWeaponArray) // [HARDCODED] for now we opted to send over CharacterData and Weapon(Data) since all gadgets relate to these
	{
		type = this.GetType().Name; // store type based upon classname
		activationTime = MissionManager.missionData.time; // Get the time in the mission that this thing was activated.
		InitializeSpecific(aCharacterData, aWeaponArray); // initialize anything specific for this gadget (see other classes)
	}

	// relay
	protected virtual void InitializeSpecific(CharacterData aCharacterData, Weapon[] aWeaponArray){} // initialize specific
	public virtual void Update(){} // update
	public virtual void ResetGadget(){} // reset the gadget
	public virtual void SetGadgetActive(bool aState) {} // set it (in)active
	public virtual void Destroy(){} // Clear up
}
