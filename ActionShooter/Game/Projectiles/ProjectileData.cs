using UnityEngine;
using System.Collections;

[System.Serializable]
public class ProjectileData
{
	public string  prefab         	= "";

	public Vector3 startPosition  	= Vector3.zero;
	public Vector3 endPosition    	= Vector3.zero;
	public Vector3 position       	= Vector3.zero;

	public float   speed          	= 0f;
	public Vector3 direction      	= Vector3.zero;

	public float   damage         	= 0f;
	public float   strength       	= 1f;
	internal float sourceStrength 	= 1f;

	public float   lifetime       	= 3f;
	internal float sourceLifeTime 	= 3f;
	public float lifePercentage   	= 1f;

	public string sound			  	= "None";
	public string trail             = "None";
	public string explosion         = "None";
	public string areaDamage        = "None";

	public GameObject target      	= null; // Don't get confused, this one is necessary for (homing) missiles/projectiles.
	public GameObject sender        = null; // this is the sender, to be used to exclude from (area)damage
}