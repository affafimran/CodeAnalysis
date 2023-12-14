using UnityEngine;
using System.Collections;

/// <summary>
/// Power: FastMotion.
/// </summary>
public class PowerFastMo : Power
{
	/// <summary>
	/// Initializes anyhting specific to this power.
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public override void InitializeSpecific(GameObject aParent){}

	public override bool Update(bool aPowerOn)
	{
		// change timescale when activated
		if (aPowerOn)Time.timeScale *= 1.1f; // up timescale
		else Time.timeScale *= 0.9f; // return time scale
		Time.timeScale = Mathf.Max(1f, Mathf.Min(3.0f, Time.timeScale)); // limit to 1 (lowest), 3 (highest)
		return aPowerOn;
	}
	
	public override void UpdateEffects(){}
	public override void Destroy(){}
}

