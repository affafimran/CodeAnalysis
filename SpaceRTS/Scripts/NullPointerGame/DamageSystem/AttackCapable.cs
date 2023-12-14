using System;
using GameBase;
using UnityEngine;
using NullPointerCore.CoreSystem;
using System.Collections.Generic;

namespace NullPointerGame.DamageSystem
{
	public class AttackCapable : GameEntityComponent
	{
		[Serializable]
		public class AttackInfo
		{
			public AttackActionConfig attackConfig;
			private float timeSinceLastAttack=0.0f;
			private Projectile lastProjectile;

			public float GetAttackRange() { return attackConfig.GetAttackRange(); }
			public bool CanUseAttack() { return (Time.timeSinceLevelLoad - timeSinceLastAttack) > attackConfig.GetAttackCooldown(); }
			public LayerMask GetAttackMask() { return attackConfig != null ? attackConfig.attackMask : (LayerMask)0; }
			public bool IsValidTarget(GameEntity entity, Damageable target) { return attackConfig.IsValidTarget(entity, target); }

			public void Attack(GameEntity entity, Damageable target)
			{
				timeSinceLastAttack = Time.timeSinceLevelLoad;
				lastProjectile = attackConfig.Attack(entity, target);
			}

			internal void Reset()
			{
				timeSinceLastAttack = 0.0f;
			}

			public void TryToDisarm()
			{
				if( lastProjectile != null && attackConfig!=null && attackConfig.disarmOnSourceDestroy )
					lastProjectile.Disarm();
			}
		}
		public List<AttackInfo> attacks = new List<AttackInfo>();

		public Vector3 Position { get { return transform.position; } }

		public int TotalAttacksCount { get { return attacks.Count; } }

		public IEnumerable<AttackActionConfig> AttackTypes 
		{
			get
			{
				foreach( AttackInfo attack in attacks )
				{
					if( attack==null )
						continue;
					if( attack.attackConfig == null )
						continue;
					yield return attack.attackConfig;
				}
			}
		}

		public static bool IsValid(AttackCapable attackCapable)
		{
			// Must be a valid reference
			if( attackCapable == null )
				return false;
			if( !attackCapable.isActiveAndEnabled )
				return false;
			// The lists of available attacks must not be empty
			if( attackCapable.attacks.Count == 0 )
				return false;
			// At least the attack zero must be valid
			if( !attackCapable.IsValidAttackType(0) )
				return false;
			return true;
		}

		public void Start()
		{
			foreach( AttackInfo attack in attacks )
			{
				if( attack != null )
					attack.Reset();
			}
		}

		public void TryToDisarmAllAttacks()
		{
			foreach( AttackInfo attack in attacks )
				attack.TryToDisarm();
		}

		public bool IsValidAttackType(int attackType)
		{
			if( attackType < 0 || attackType >= attacks.Count )
				return false;
			if(attacks[attackType].attackConfig == null)
				return false;
			return true;
		}

		public LayerMask GetAttackMask(int attackType)
		{
			if( attackType < 0 || attackType >= attacks.Count )
				return 0;
			return attacks[attackType].GetAttackMask();
		}

		/// <summary>
		/// The range distance between units for the attack to be performed. Doesn't take into account the target radius.
		/// </summary>
		/// <param name="attackType">The type of attack to use for the measurement.</param>
		/// <returns>The attack range of the given attack type.</returns>
		public float GetAttackRange(int attackType)
		{
			if( !IsValidAttackType(attackType) )
				return 0.0f;
			return attacks[attackType].GetAttackRange();
		}

		/// <summary>
		/// The distance required for this unit to be able to attack the given target at his center.
		/// </summary>
		/// <param name="target">Damageable target to analize distance.</param>
		/// <param name="attackType">The type of attack to use.</param>
		/// <returns>THe total distance from this unit to the center of the other unit (target).</returns>
		public float GetAttackDistance(Damageable target, int attackType)
		{
			if( target == null )
				return 0.0f;
			if( !IsValidAttackType(attackType) )
				return 0.0f;
			float targetRadius = target.GetRadius();
			return attacks[attackType].GetAttackRange()+targetRadius;
		}

		public bool IsInAttackRange(Damageable target, int attackType)
		{
			if( target == null )
				return false;
			float attackRange = GetAttackRange(attackType);
			float targetRadius = target.GetRadius();
			Vector3 attackVector = target.Position - this.Position;
			return attackVector.magnitude < attackRange+targetRadius;
		}

		public bool CanUseAttack(int attackType)
		{
			if( !IsValidAttackType(attackType) )
				return false;
			return attacks[attackType].CanUseAttack(); 
		}

		public void Attack(Damageable target, int attackType)
		{
			if( target && !target.IsAlive() )
				return;
			if ( !IsValidAttackType(attackType) )
				return;
			attacks[attackType].Attack(ThisEntity, target); 
		}

		public bool IsValidTarget(Damageable target, int attackType)
		{
			if( !IsValidAttackType(attackType) )
				return false;
			return attacks[attackType].IsValidTarget(ThisEntity, target); 
		}
	}
}
