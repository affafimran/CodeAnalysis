using UnityEngine;
using System.Collections;

/// <summary>
/// Explosive data.
/// </summary>
public class ExplosiveData
{
	public string type        = "Default"; // type (or prefab) reference
	public float rof          = 0.1f; // current rate of fire
	internal float sourceRof  = 0.1f; // original rate of fire
	public int ammo           = 30; // ammo
	public bool unlimitedAmmo = false; // unlimited ammo
	public string sound       = "Default"; // sound effect to be played when activating explosige

}	

