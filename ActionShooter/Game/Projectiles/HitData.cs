using UnityEngine;
using System.Collections;

[System.Serializable]
public class HitData
{
	public bool result = false;
	public GameObject gameObject = null;
	public Vector3 direction = Vector3.up;
	public Vector3 position = Vector3.zero;
	public float distance = Mathf.Infinity;
}
