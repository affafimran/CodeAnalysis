using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	[RequireComponent(typeof(Collider))]
	public class ProjectileMissile : Projectile
	{
		public float speed = 5.0f;

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if( trackedTarget != null && !dissarmed && speed > 0 )
				direction = Vector3.Normalize( trackedTarget.position - transform.position );
			if( speed > 0 )
				transform.position += direction * speed * Time.fixedDeltaTime;
		}

		public void OnTriggerEnter(Collider other)
		{
			if( dissarmed )
				return;
			if( other == null )
				return;
			GameEntity collidedEntity = other.GetComponentInParent<GameEntity>();
			if( collidedEntity != sourceEntity )
			{
				dissarmed = true;
				if( triggerEffectPrefab != null )
					GameObject.Instantiate(triggerEffectPrefab, transform.position, Quaternion.identity, collidedEntity.transform );
				AutoDestroy();
				ApplyHitEffect(collidedEntity.GetComponent<Damageable>());
			}
		}

		protected override void OnDisarmed()
		{
			base.OnDisarmed();
			Invoke("AutoDestroy", destroyTime);
		}
	}
}
