using UnityEngine;	
using System.Collections;
using System.Collections.Generic;

public static class CharacterManager
{
	public enum CONTROLLER {User, AI}

	public static int maximumAllowedCharacters = 250; // Webplayer
	public static GameObject characterHolder;

	public static void Initialize()
	{
		GameObject tCharacterHolder = GameObject.Find("Characters");
		if (tCharacterHolder == null) characterHolder = new GameObject("Characters");
		else characterHolder = tCharacterHolder;

		// [MOBILE] Just like WebGL we limit the amount of characters to 21 (1 hammer and 20 max. enemies)
		// This will work fine for spawners. They'll just wait until an enemy dies...
		if (Application.platform == RuntimePlatform.WebGLPlayer || GameData.mobile) maximumAllowedCharacters = 21;
	}

	public static void AddCharacter(string aType, CONTROLLER aController, GameObject aSpawnObject)
	{
		SpawnData spawnData = SpawnerManager.GetExistingSpawnData(null, aSpawnObject);
		AddCharacter(aType, aController, spawnData);
	}

	public static bool AddCharacter(string aType, CONTROLLER aController, SpawnData aSpawnData)
	{
		// no more characters when maximumAllowed is reached.
		if (characterHolder.transform.childCount >= maximumAllowedCharacters) return false;

		GameObject characterObject;
		Character character;
		CharacterData characterData = new CharacterData();
		characterData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Characters"].d[aType].d, characterData) as CharacterData;

		switch (aType){
			// User
			case "Hammer":
				characterData.godMode = GameData.godMode; // some cheat override for debug/testing or actual cheat
				characterData.autoAim = GameData.mobile; // [MOBILE] a test to see if autoAiming on mobile would be easier to play. (PIETER) I like it...
				if (ShopItemManager.IsBought("ShopItem12")) characterData.weapon = "Magnum"; // Replace default weapon with Golden Gun if correct shopitem is bought
				characterObject = Loader.LoadGameObject("Hammer/"+characterData.prefab+"_Prefab"); // load user object
				character = characterObject.AddComponent<Hammer>() as Character; // add 'Hammer' component
				character.Initialize(aType, aController, characterData, aSpawnData); // initialize
				characterObject.transform.parent = characterHolder.transform; // parent to character holder
				break;

			// Enemies
			case "EnemySuit":
			case "EnemySwat":
			case "EnemyArmored":
			case "EnemySoldier":
				if (!GameData.buildAI) return false; // return when debug buildAI is false - play without enemies
				characterObject = Loader.LoadGameObject("Enemies/"+characterData.prefab+"_Prefab");		
				character = characterObject.AddComponent<Enemy>() as Character;
				character.Initialize(aType, aController, characterData, aSpawnData);
				characterObject.transform.parent = characterHolder.transform;
				Scripts.map.CreateMapIcon(characterObject, aType); // add enemy to minimap
				break;

			// Bosses
			case "EnemyKatie":
			case "EnemyJuggernaut":
			case "EnemyBigBoss":
				if (!GameData.buildAI) return false; // return when debug buildAI is false - play without enemies
				characterObject = Loader.LoadGameObject("Enemies/"+characterData.prefab+"_Prefab");		
				character = characterObject.AddComponent<Boss>() as Character;
				character.Initialize(aType, aController, characterData, aSpawnData);
				characterObject.transform.parent = characterHolder.transform;
				Scripts.map.CreateMapIcon(characterObject, aType); // add boss to minimap
				break;		
		}
		return true;
	}



	public static Enemy AutoAimEnemy(GameObject anObject, float aMaxSqrMagnitude, float aMaxAngle){
		// all enemies
		Enemy[] enemies = characterHolder.GetComponentsInChildren<Enemy>();

		// position & direction vector
		Vector3 position = anObject.transform.position; // working position
		Vector3 direction = anObject.transform.forward; // working direction
		direction.y = 0;
		direction.Normalize();
		// references so we don't make this each iterations
		Vector3 offset;
		float distance = 0f;
		float currentMinDistance = aMaxSqrMagnitude * aMaxSqrMagnitude;
		float angle= 0f;

		Enemy closestEnemy = null;
		List<Enemy> enemiesInAngle = new List<Enemy>(); // list to store enemies 'in angle'

		// We're going to do this in 2 steps
		// First we get the enemies 'in angle'
		// Then we get the closest one of those. This is more natural for the user and autoaim won't jump all pver the place
		foreach(Enemy enemy in enemies){
			offset = enemy.transform.position-position;
			offset.y = 0;
			angle = Vector3.Angle(offset, direction);
			if (angle <= aMaxAngle) {
				// hold it! 
				// now we know the enemy is more or less in front of us
				// however, we should look if the enemy is not too much below or too high.
				float relativeY = anObject.transform.InverseTransformPoint(enemy.transform.position).y;
				if (relativeY < 0 && relativeY >= -2.0f) enemiesInAngle.Add(enemy);				
			}
		}

		// now get the closest one
		foreach(Enemy enemy in enemiesInAngle){
			offset = enemy.transform.position-position;
			offset.y = 0;
			distance = Vector3.SqrMagnitude(offset);
			if (distance <= currentMinDistance){
				currentMinDistance = distance;
				closestEnemy = enemy;
			}
		}

		// return my enemy
		return closestEnemy;
	}


	// Temp? or final?
	public static void AllowInputEnemies(bool aState)
	{
		Enemy[] enemies = characterHolder.GetComponentsInChildren<Enemy>();
		foreach(Enemy enemy in enemies)
			enemy.AllowInput(aState);
	}


		

}

