using GameBase;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NullPointerGame.DamageSystem
{
	public class Projectile : MonoBehaviour
	{
		public string launchSocket = "cp_socket";
		public GameObject triggerEffectPrefab;
		public bool attachToSocket = false;
		public float disarmTime = 5.0f;
		public float destroyTime = 1.0f;

		[Header("Emit Effect")]
		public GameObject emitEffectPrefab;
		public bool attachEmitEffectToSocket;

		[Header("On Hit Effect")]
		public GameObject hitEffectPrefab;
		public bool attachHitEffectToTarget = true;
		public float damage = 10.0f;

		protected float runTime = 0.0f;
		protected bool dissarmed = false;
		protected Vector3 direction = Vector3.forward;

		protected GameEntity sourceEntity = null;
		protected Transform sourceEmitter = null;
		protected Transform trackedTarget;
		protected Vector3 lockedPointOnTarget;
		//protected GameObject hittedTargetEffect = null;

		protected virtual Transform GetEmitter(GameEntity source, Damageable target)
		{
			return GetBestFireSource( source, target.transform.position );
		}

		protected virtual Vector3 GetTargetOffset(GameEntity source, Damageable target)
		{
			return Vector3.zero;
		}

		public virtual void InitSetup(GameEntity source, Damageable target)
		{
			if( source == null )
				return;
			this.sourceEntity = source;
			Transform emitter = GetEmitter(source, target);
			this.sourceEmitter = emitter != null ? emitter : sourceEntity.transform;
			this.transform.position = sourceEmitter.position;
			if( target != null )
			{
				trackedTarget = target.transform;
				lockedPointOnTarget = GetTargetOffset(source, target);
				direction = Vector3.Normalize( target.transform.position - transform.position );
			}
		}

		public void Disarm()
		{
			dissarmed = true;
			OnDisarmed();
		}

		public virtual void FixedUpdate()
		{
			if( !dissarmed )
			{
				runTime += Time.fixedDeltaTime;
				if( runTime > disarmTime )
					Disarm();
			}
		}

		protected virtual void OnDisarmed() {}

		protected virtual void AutoDestroy()
		{
			GameObject.Destroy( this.gameObject );
		}

		public void ApplyHitEffect(Damageable damageable)
		{
			OnApplyHitEffect(damageable, Vector3.zero);
		}

		public void ApplyHitEffect(Damageable damageable, Vector3 offset)
		{
			OnApplyHitEffect(damageable, offset);
		}

		protected virtual void OnApplyHitEffect(Damageable damageable, Vector3 offset)
		{
			if( damageable == null )
				return;
			if( hitEffectPrefab != null )
			{
				Transform hitParent = attachHitEffectToTarget ? damageable.transform : null;
				GameObject hittedTargetEffect = GameObject.Instantiate( hitEffectPrefab, hitParent ) as GameObject;
				hittedTargetEffect.transform.localPosition = offset;
			}
			damageable.ApplyFixedDamage(damage);
		}

		public Transform GetBestFireSource( GameEntity entity, Vector3 targetPoint )
		{
			if(entity == null || entity.VisualProxy==null)
				return null;

			IEnumerable<Transform> emitters = entity.VisualProxy.GetAllPropertyValues<Transform>(launchSocket);

			Transform bestSource = null;
			float bestAngle = 0;
			foreach( Transform emitter in emitters )
			{
				float angle = Vector3.Angle( emitter.position-entity.transform.position, targetPoint-entity.transform.position );
				if( bestSource==null || angle < bestAngle )
				{
					bestSource = emitter;
					bestAngle = angle;
				}
			}
			return bestSource;
		}
	}
}