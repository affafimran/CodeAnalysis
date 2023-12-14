using UnityEngine;
using System.Collections;

/// <summary>
/// Target data to be use in TargetManager
/// </summary>
[System.Serializable]
public class TargetData 
{
	public bool target = false; // is there a target?
	public GameObject gameObject = null; // this is the target
	public Vector3 position = Vector3.zero; // this is its position
	public float distance = Mathf.Infinity; // distance
	public float distanceLeveled = Mathf.Infinity; // distance y leveled with object requesting this data
	public float verticalOffset = Mathf.Infinity; // vertical offset to target
}