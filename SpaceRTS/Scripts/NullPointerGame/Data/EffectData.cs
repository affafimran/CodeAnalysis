using GameBase;
using System.Collections;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	public abstract class EffectData : ScriptableObject
	{
		public abstract IEnumerator ApplyEffect(AreaOfEffect area, GameEntity entity);
	}
}