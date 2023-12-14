using UnityEngine;

[System.Serializable]
public class CharacterVehicleData
{
	public bool isInVehicle = false;
	public bool canEnterVehicle = false;
	public float canEnterRange = 2.5f;
	public GameObject vehicle = null;
	public string name = "";
	public string type = "";
	
	// not sure....
	[HideInInspector] public Ray ray;
	[HideInInspector] public RaycastHit rayCastHit;
	[HideInInspector] public LayerMask layerMask = (1 << 14) | (1 << 15); // MUST OPTIMIZE
}