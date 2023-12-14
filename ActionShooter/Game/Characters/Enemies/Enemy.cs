using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Character
{

	
	public string currentWeapon;
	private string previousWeapon;
	public Weapon weapon;


	
	// huhuu
	[HideInInspector] public Vector3 targetLocation;


	// spine
	private GameObject spine;

	public override void InitializeSpecific()
	{
		// we're active on spawn/initializing
		allowInput = true;

		// we're ai
		characterData.AI = true;

		//--------------------------------------------------------------------------------
		// Update CharacterController properties
		// You could also have different sets per different types of characters if you'd
		// like. These settings work for all enemies
		//--------------------------------------------------------------------------------
		characterController.center = new Vector3 (0.0f, 1.07f, 0);
		characterController.slopeLimit = 45.0f;
		characterController.radius     = 0.25f;
		characterController.height     = 2.0f;
		
		//--------------------------------------------------------------------------------
		// Update CharacterMotor properties
		//--------------------------------------------------------------------------------
		characterMotor.jumping.enabled = false;
				
		//--------------------------------------------------------------------------------
		// set my to Enemies layer
		//--------------------------------------------------------------------------------
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("Enemies"));
	
		//--------------------------------------------------------------------------------
		// controller (this is the script that actually controls the player)
		//--------------------------------------------------------------------------------
		gameObject.transform.position = spawnData.gameObject.transform.position; /// blabalalba
		targetLocation = spawnData.position; // quickly update this as we need it in the characterAIInput // TEMP???
		string tComponent = "CharacterEnemyInput"; // Experiment with user input? (aController.ToString() == "User") ? "CharacterUserInput" : "CharacterAIInput";
		controller = gameObject.AddComponent(GenericFunctionsScript.GetType(tComponent)) as CharacterInputController;
		controller.Initialize();
		controller.enabled = false; // no movement!!
		
		//--------------------------------------------------------------------------------
		// weapon
		//--------------------------------------------------------------------------------
		// temp
		currentWeapon = characterData.weapon; //(WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), characterData.weapon);
		weapon = new EnemyWeapon() as Weapon;
		weapon.Initialize(characterData, gameObject, animator);
		previousWeapon = currentWeapon;
		
		//--------------------------------------------------------------------------------
		// other character related vars	
		//--------------------------------------------------------------------------------
		spine = GenericFunctionsScript.FindChild(gameObject, type + " Spine1");
	}


	public override void Update()
	{
		// Being dead
		if (characterData.playerDead) return;
		// Pausing
		gameObject.GetComponent<CharacterMotor>().enabled = !Data.pause;
		controller.enabled = !Data.pause;
		if (Data.pause) return;
		
		// controller
		controller.enabled = allowInput;

		// weaponswitch
		if (previousWeapon != currentWeapon)
		{
			weapon.Destroy();
			weapon = System.Activator.CreateInstance(GenericFunctionsScript.GetType(currentWeapon)) as Weapon; //WeaponManager.CreateWeaponInstance((WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), currentWeapon));
			weapon.Initialize(characterData, gameObject, animator);
			previousWeapon = currentWeapon;
		}
		
		// Character movement
		// Rest of the movement that is not physics depended
		animator.SetFloat("Horizontal", controller.horAxis);		
		animator.SetFloat("Vertical", controller.verAxis);		
			
		//animator.SetBool("Jump", !characterMotor.grounded);
		//animator.SetBool("Fire3", controller.dive);

		// Ray cast for shootin,... only when firing
		if (controller.primaryFire) {

			// This is a pretty HC test... keep this in mind

			Vector3 correctedPosition = spine.transform.position;// - (direction*0.7f);
			Vector3 direction = controller.GetBulletDirection(correctedPosition);
			ray = new Ray(correctedPosition, direction);

			hitData.result = Physics.Raycast(ray, out rayCastHit, 500, layerMask);
			hitData.position = Vector3.zero;
			hitData.distance = 0f;
			hitData.gameObject   = null;
			if (hitData.result)
			{
				hitData.position = rayCastHit.point;
				hitData.distance = (hitData.position - weapon.bulletOrigin).magnitude;
				hitData.gameObject   = rayCastHit.collider.gameObject;
			}
			controller.hitData = hitData;
			weapon.Update(controller.primaryFire, hitData);//.hitPoint, hitData.target);
		} else weapon.Update(controller.primaryFire, hitData); //this is OLD hitData! Let's see where this takes us (remember the burstFire)
	}

	public override bool Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		// do damage & kill
		if (!characterData.godMode) characterData.health -= aProjectileData.damage;
		if (characterData.health <= 0 && !characterData.playerDead){Kill(aProjectileData, aHitData);return true;}
		else {
			// (PIETER) Since the controllers are such a fucking mess and I don't have time to redo it properly...
			// ...I am implenting a hack for the most common animations issues
			for (int i = 1; i < 5; i++) {
				if (animator.isActiveAndEnabled) animator.SetBool("Hit"+i, false);
			}
			if (animator.isActiveAndEnabled) animator.SetBool("Hit"+UnityEngine.Random.Range(1, 5), true);//
		}
		return false;
	}

	public void Kill(ProjectileData aProjectileData, HitData aHitData)
	{
		// set to dead
		characterData.playerDead = true;

		// test stuff
		//		Destroy(animator);
		Destroy(characterMotor);
		Destroy(characterController);
		Destroy(controller);
		Destroy(this);

		// See script for details
		gameObject.AddComponent<CharacterAnimationEventCatcher>();

		// simple animation || fly away || gib
		float strength = aProjectileData.strength * aProjectileData.lifePercentage;
		if (strength < 1.0f){
			int random = UnityEngine.Random.Range(1, 5);
			animator.SetBool("Death"+random, true); 
			animator.Play("Death"+random); // play this animation instantly! DO NOT use setBool or any other way that would blend/fade animation. Some AnimationEvents can be triggered, but this component can be gone!!!
		}else{
			Destroy(animator);

			Vector3 tLookAt = aProjectileData.position;
			tLookAt.y = gameObject.transform.position.y;
			gameObject.transform.LookAt(tLookAt);

					
			GameObject bip = gameObject.transform.GetChild(1).transform.gameObject;
			Rigidbody rigidbody = bip.AddComponent<Rigidbody>();

			rigidbody.drag = 0.9f;
			rigidbody.angularDrag = 0.6f;

			ImpactManager.ImpactForce(rigidbody, aProjectileData.direction, aProjectileData.strength);
		}


		// spawn spawners
		SpawnerManager.AddSpawners(gameObject.transform.position, characterData.spawners);
		PickUpManager.AddPickUps(gameObject.transform.position, characterData.pickUps);


		Destroy(gameObject, 1.5f); // (DG) Changed this back from 5.0f. Not sure why it needed to take so long.

		MissionManager.ProcessTarget(type, characterData.score);
	}
}
