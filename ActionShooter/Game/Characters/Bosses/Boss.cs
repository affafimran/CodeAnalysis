using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Boss : Character
{


	public CharacterBossData bossData;

	public string currentWeapon;
	private string previousWeapon;
	public Weapon weapon;
	
		// huhuu
	[HideInInspector] public Vector3 targetLocation;

	public override void InitializeSpecific()
	{
		// we're active on spawn/initializing
		allowInput = true;

		characterData.AI = true;
		bossData = new CharacterBossData ();
		bossData.prefab = characterData.prefab;

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
		// ....

		//--------------------------------------------------------------------------------
		// set my to player layer
		//--------------------------------------------------------------------------------
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("Enemies"));

		//--------------------------------------------------------------------------------
		// controller (this is the script that actually controls the player)
		//--------------------------------------------------------------------------------
		targetLocation = spawnData.position; // quickly update this as we need it in the characterAIInput // TEMP???
		string tComponent = "CharacterBossInput";
		controller = gameObject.AddComponent(GenericFunctionsScript.GetType(tComponent)) as CharacterInputController;
		controller.Initialize();
		controller.enabled = false; // no movement!!
		
		//--------------------------------------------------------------------------------
		// weapon
		//--------------------------------------------------------------------------------
		currentWeapon = characterData.weapon; //(WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), characterData.weapon);
		weapon = new EnemyWeapon() as Weapon;
		weapon.Initialize(characterData, gameObject, animator);
		previousWeapon = currentWeapon;
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
	
		// switch
		if (previousWeapon != currentWeapon)
		{
			weapon.Destroy();
			weapon = System.Activator.CreateInstance(GenericFunctionsScript.GetType(currentWeapon)) as Weapon; //WeaponManager.CreateWeaponInstance((WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), currentWeapon));
			weapon.Initialize(characterData, gameObject, animator);
			previousWeapon = currentWeapon;
		}

		// Jump
		characterMotor.inputJump = controller.jump;	
		
		// Character movement
		// Rest of the movement that is not physics depended
		animator.SetFloat("Horizontal", controller.horAxis);		
		animator.SetFloat("Vertical", controller.verAxis);
//		if (controller.horAxis != 0 || controller.verAxis != 0) 
//		{
//			animator.SetBool("Hit1", false);
//			animator.SetBool("Hit2", false);
//			animator.SetBool("Hit3", false);
//			animator.SetBool("Hit4", false);
//		}
		
		//animator.SetBool("Jump", !characterMotor.grounded);
		//animator.SetBool("Fire3", controller.dive);
		
		// Ray cast for shootin?
		if (controller.primaryFire) {
			if (characterData.AI) ray  = new Ray(weapon.bulletOrigin, controller.GetBulletDirection(weapon.bulletOrigin));
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
			weapon.Update(controller.primaryFire, hitData);//.hitPoint, hitData.target);
		} else weapon.Update(controller.primaryFire, null);//.hitPoint, hitData.target);
	}
	
	public override bool Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		// boss isn't active yet
		if (!bossData.active) return false;
		// keep track of the amount of hits taken (Dodge variable)
		bossData.amountOfHitsTaken++;
		// do damage & kill
		if (!characterData.godMode) characterData.health -= aProjectileData.damage;
		if (characterData.health <= 0 && !characterData.playerDead){Kill(aProjectileData, aHitData);return true;}
		else 
		{
			int randomHit = UnityEngine.Random.Range(1, 5);
			if (characterData.prefab != "EnemyKatie") randomHit = 1;
//			Scripts.audioManager.PlaySFX3D("Characters/Enemies/"+bossData.prefab+"/"+bossData.prefab+"Hit"+randomHit, this.gameObject,"Taunt");
			Scripts.audioManager.PlaySFX3D("Characters/Enemies/EnemyHit"+randomHit, this.gameObject);
			animator.SetBool("Hit"+randomHit, true);
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
			animator.Play("Death"+random);
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
		
		
		Destroy(gameObject, 5f); //, 1.5f);

		MissionManager.ProcessTarget(type, characterData.score);
	}
}

