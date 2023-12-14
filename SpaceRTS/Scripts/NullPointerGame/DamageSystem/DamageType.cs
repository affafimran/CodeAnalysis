using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	[System.Serializable]
	public class DamageType
	{
		public GameEntity source;
		public float baseValue;
	}
}
