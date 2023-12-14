using UnityEngine;
using System.Collections;

/// <summary>
/// Power: SlowMotion
/// </summary>
public class PowerSlowMo : Power
{
	/// <summary>
	/// Initializes anyhting specific to this power.
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public override void InitializeSpecific(GameObject aParent){}
	
	public override bool Update(bool aPowerOn)
	{
		if (aPowerOn) Time.timeScale *= 0.9f; // lower time scale
		else Time.timeScale *= 1.1f; // restore timesacle
		Time.timeScale = Mathf.Min(1f, Mathf.Max(0.3f, Time.timeScale)); // limit timescale between 0.3 and 1
		return aPowerOn;
	}

	public override void UpdateEffects(){}
	public override void Destroy(){}
}