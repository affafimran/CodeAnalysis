using UnityEngine;
using System.Collections;

/// <summary>
/// Character parent class.
/// </summary>
public class Character : MonoBehaviour
{
	public bool allowInput = false; // allow to have input (move, etc.)
	public string type; // type as name. E.g. Hammer or EnemySuit

	public CharacterData characterData; // this character's data

	[HideInInspector] public CharacterController characterController = null; // reference to Controller (hidden)
	[HideInInspector] public CharacterMotor characterMotor = null; // reference to Motor (hidden)

	[HideInInspector] public CharacterInputController controller; // refernce to input controller (hidden)

	internal Animator animator; // reference to animator

	public HitData hitData; // public HitData so we can see what this character is hitting in the inspector.

	// Raycast setup
	internal Ray ray;
	internal RaycastHit rayCastHit;
	internal LayerMask layerMask = ProjectileManager.projectileLayerMask;

	internal SpawnData spawnData; // spawnData (see SpawnData)
		
	public virtual void Initialize(string aType, CharacterManager.CONTROLLER aController, CharacterData aCharacterData, SpawnData aSpawnData)
	{
		//--------------------------------------------------------------------------------
		// Assign vars
		//--------------------------------------------------------------------------------
		type = aType;
		
		//--------------------------------------------------------------------------------
		// Get playerdata
		//--------------------------------------------------------------------------------
		characterData = new CharacterData();
		characterData = aCharacterData;

		// setup hitData
		hitData = new HitData();
		
		//--------------------------------------------------------------------------------
		// Add (controller) components to man		
		//--------------------------------------------------------------------------------
		characterController = gameObject.AddComponent<CharacterController>();
		characterMotor = gameObject.AddComponent<CharacterMotor>();
		
		//--------------------------------------------------------------------------------
		// Setup CharacterController properties (default for Hammer)
		//--------------------------------------------------------------------------------
		gameObject.GetComponent<CharacterController> ().center = new Vector3 (0.0f, 1.01f, 0);
		gameObject.GetComponent<CharacterController>().slopeLimit = 90.0f;
		gameObject.GetComponent<CharacterController>().radius     = 0.25f;
		gameObject.GetComponent<CharacterController>().height     = 1.85f;
		
		//--------------------------------------------------------------------------------
		// Setup CharacterMotor properties (default for Hammer)
		//--------------------------------------------------------------------------------
		characterMotor.movement.maxGroundAcceleration = 1000000.0f;
		characterMotor.movement.maxAirAcceleration    = 1000000.0f;
		characterMotor.movement.maxForwardSpeed = characterMotor.movement.maxBackwardsSpeed = characterMotor.movement.maxSidewaysSpeed = characterData.speed;
		characterMotor.movement.gravity = 80;
		characterMotor.movement.maxFallSpeed = 40;

		characterMotor.jumping.baseHeight  = characterData.jumpHeight; 
		characterMotor.jumping.extraHeight = 0.0f;

		characterMotor.sliding.enabled = false;
		characterMotor.useFixedUpdate = false;

		//--------------------------------------------------------------------------------
		// animator & others
		//--------------------------------------------------------------------------------
		animator = gameObject.GetComponent<Animator>();

		//--------------------------------------------------------------------------------
		// Spawndata and positioning of the gameobject
		//--------------------------------------------------------------------------------
		spawnData = aSpawnData;
		gameObject.transform.position = spawnData.position;
		gameObject.transform.rotation = spawnData.rotation;
		
		
		//--------------------------------------------------------------------------------
		// Initialize Character specifics
		//--------------------------------------------------------------------------------
		InitializeSpecific();		
	}	

	public virtual void InitializeSpecific(){}

	public virtual void FixedUpdate()
	{
		// Pausing
		if (Data.pause) return;
		
		// Character movement, we do this here as we're updating physics stuff
		if (allowInput && !characterData.playerDead){ // is playerDead necessary
			Vector3 directionVector;
			directionVector = new Vector3(controller.horAxis, 1, controller.verAxis);
			characterMotor.inputMoveDirection = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * directionVector;
		}
	}

	// Update is called once per frame
	public virtual void Update ()
	{}

	public virtual void AllowInput(bool aState)
	{
		// set bool
		allowInput = aState;
		// reset characterController(?) & characterMotor
		characterMotor.inputMoveDirection = Vector3.zero;
		// reset all necessary
		controller.Reset();
		controller.enabled = aState; // set this instantly to avoid 'confusion' (still updated in the main loop)
	}
	
	public virtual string GetImpactType(float aDamage)
	{
		if (characterData.playerDead) return "EnemyDie";
		if (aDamage <= 1) return "EnemyHitSmall";
		else if (aDamage <= 5) return "EnemyHitMedium";
		else return "EnemyHitLarge";
	}	

	public virtual bool Damage(ProjectileData aProjectileData, HitData aHitData){return false;}
	
	public virtual void SetAnimation(string anAnimation)
	{
		// this function always assumes the incoming animation needs to be set to true
		animator.SetBool(anAnimation, true);		
	}
	
	public virtual void ResetAnimation(string anAnimation)
	{
		if (characterData.playerDead) Debug.Log("[Character] I am already dead!");
		// this function always assumes the incoming animation needs to be set to false
		animator.SetBool(anAnimation, false);		
	}
	
	public virtual void PlaySFX(string aSoundEffect)
	{}
}

