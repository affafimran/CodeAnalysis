using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEnemyData
{
	public float homeRadius = 50f;
	public float attackRadius = 10f;
	[HideInInspector] public float sourceAttackRadius = 10f;
}