using UnityEngine;
using System.Collections;

/// <summary>
/// Power: None.
/// <para>Empty power... you can still do stuff here.</para>
/// </summary>
public class PowerNone : Power
{
	/// <summary>
	/// Initializes anyhting specific to this power. Which is nothing for this Power
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public override void InitializeSpecific(GameObject aParent){}

	public override bool Update(bool aPowerOn){return false;}
	
	public override void UpdateEffects(){}

	public override void Destroy(){}
}

