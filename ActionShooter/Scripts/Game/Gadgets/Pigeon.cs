using UnityEngine;
using System.Collections;

/// <summary>
/// Pigeon.
/// <para>Respawns the owner of the gadget when died.</para>
/// </summary>
public class Pigeon : Gadget
{
	// Try to avoid putting vars here
	// It is harder to get these. Define and set them in Gadget.cs and make the more general	
	protected override void InitializeSpecific(CharacterData aCharacterData, Weapon[] aWeaponArray)
	{
		// Initialize stuff here
		gadgetActive = true;
		Scripts.audioManager.PlaySFX("Interface/GadgetHolyPigeon");
	}
	
	public override void Update()
	{
		// Update stuff here
		if (!gadgetActive) return;
		UpdateEffects(); // if necessary?		
	}

	public override void ResetGadget()
	{
		// If necessary you can reset gagdet params, etc. here.
		// If we're never going to use this we'll remove it.
	}
	
	public override void SetGadgetActive(bool aState)
	{
		gadgetActive = aState; // set bool
		// You can do other stuff here as well
		// E.g. turn particles on/off if the gadget uses these.
	}
	
	public void UpdateEffects()
	{
		// Knock yourself out		
	}
	
	public override void Destroy()
	{
		// Clean up here when it needs to be destroyed.
	}
}