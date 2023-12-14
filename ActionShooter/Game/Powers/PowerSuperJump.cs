using UnityEngine;
using System.Collections;

/// <summary>
/// Power: SuperJump
/// </summary>
public class PowerSuperJump : Power
{		
	public float jumpboost = 0f; // jumpboost to charge
	public bool previousOn = false; // toggle to check if we released the poweOn button
	public Hammer parentScript; // [HARDCODED] Hammer reference

	/// <summary>
	/// Initializes anyhting specific to this power.
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public override void InitializeSpecific(GameObject aParent)
	{
		parentScript = aParent.GetComponent<Hammer>(); // Store reference
	}

	public override bool Update(bool aPowerOn)
	{
		if (aPowerOn) jumpboost = 0.001f + (jumpboost*1.15f); // charge
		else jumpboost *= 0.9f; // lower
		jumpboost = Mathf.Min(100f, Mathf.Max(0.001f, jumpboost)); // limit boost
		if (!aPowerOn && previousOn) parentScript.characterMotor.inputJump = true; // jump when released
		previousOn = aPowerOn; // store
		parentScript.characterMotor.jumping.baseHeight = parentScript.characterData.jumpHeight + jumpboost; // change character jumpheight
		return aPowerOn;
	}
	
	public override void UpdateEffects(){}
	public override void Destroy(){}
}

