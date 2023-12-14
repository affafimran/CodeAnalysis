using System.Collections;
using GameBase;
using UnityEngine;
using NullPointerGame.DamageSystem;

namespace NullPointerGame.DamageSystem
{
	[CreateAssetMenu(menuName="SpaceRTS/Effect/DamageOverTime")]
	public class EffectDamage : EffectData
	{
		public float damageRate = 3;

		public override IEnumerator ApplyEffect(AreaOfEffect area, GameEntity entity)
		{
			Damageable damageable = entity.GetComponent<Damageable>();
			if( damageable == null )
				yield break;

			while( area.Contains(entity) )
			{
				damageable.ApplyFixedDamage(damageRate*Time.deltaTime);
				yield return null;
			}
		}
	}
}