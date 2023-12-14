using UnityEngine;
using System.Collections;

public class VehicleInputController: MonoBehaviour
{
	public string type = VehicleManager.CONTROLLER.Empty.ToString();
	
	// Movement Vars
	public float verAxis = 0f;
	public float horAxis = 0f;
	
	public bool verAxisAsBool = false;
	public bool horAxisAsBool = false;
	
	public bool drift = false;
	public bool primaryFire = false;
	public bool secondaryFire = false;
	public bool use = false;

	public bool up = false;
	public bool down = false;

	public virtual void Initialize(){}
	public virtual void Update(){}
	public virtual void Reset(){}
	public virtual void Destroy(){Destroy(this);}
}