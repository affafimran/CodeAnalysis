using UnityEngine;
using System.Collections;

/// <summary>
/// Area damage data.
/// </summary>
[System.Serializable]
public class AreaDamageData
{
	public Vector3 position = Vector3.zero; // center location of area damage
	public float damage = 10f; // total damage
	internal float sourceDamage = 10f; // original damage
	public float strength = 5f; // strengt

	public float start = 3f; // start size
	public float end = 15f;	 // end size

	public float radius = 3f; // current radius
	public float size = 0f;	 // current size

	public float lifeTime = 0.3f; // lifetime/duration
	internal float sourceLifeTime = 0.3f; // original lifetime
	public float lifePercentage   = 1f; // percentage of lifetime
}
