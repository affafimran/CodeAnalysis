using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Power base class.
/// </summary>
[System.Serializable]
public class Power
{
	public string type        = "Default"; // type

	/// <summary>
	/// Initialize this Power (needs parent, actual user of the power)
	/// </summary>
	/// <param name="aParent">A parent.</param>
	public void Initialize(GameObject aParent)
	{
		type = this.GetType().Name; // type from class name
		InitializeSpecific(aParent); // Initialize specific power related stuff
	}

	public virtual void InitializeSpecific(GameObject aParent){} // init specific stuff
	public virtual bool Update(bool aPowerOn){return false;} // update it (default returns false, cause nothing happens)
	public virtual void UpdateEffects(){} // update effects if any
	public virtual void Destroy(){} // clear up power

}
