using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Destructible data.
/// </summary>
[System.Serializable]
public class DestructibleData
{
	public string type = "Default"; // type name
	public bool destroyed = false; // is it destroyed?
	public float health = 100f; // health 
	public int score = 10; // score to be gained afters destruction
	public bool dynamic = false; // it is dynamic if it has a mass (a rigidbody will be added as well)
	public int mass = 0; // mass (a rigidbody will be added and dynamic will be true)
	public string impact = "Default"; // what impact to spawn when you shoot/damage it
	public string explosion = "Default"; // what explosion to spawn when it is destroyed
	public float lifetime = 3f; // time left when it is destroyed (after passing it will be removed from the scene)
	public List<string> spawners = new List<string>(); // list of spawners to be created when destroyed (think bonusses, enemies ;))
	public List<string> pickUps  = new List<string>(); // list of pickups to be created when destroyed, easier/faster then through spwaners
	internal bool reParent = false; // an odd one... Sometimes destructible need to be reparent to a certain object so you can destroy them. E.g. a turret on top of a building.
}