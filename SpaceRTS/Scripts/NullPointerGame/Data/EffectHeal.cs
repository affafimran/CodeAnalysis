using GameBase;
using System.Collections;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	[CreateAssetMenu(menuName="SpaceRTS/Effect/HealOverTime")]
	public class EffectHeal : EffectData
	{
		public float healRate = 3;

		public override IEnumerator ApplyEffect(AreaOfEffect area, GameEntity entity)
		{
			Damageable damageable = entity.GetComponent<Damageable>();
			if( damageable == null )
				yield break;

			while( area.Contains(entity) )
			{
				damageable.ApplyFixedHeal(healRate*Time.deltaTime);
				yield return null;
			}
		}
	}
}
