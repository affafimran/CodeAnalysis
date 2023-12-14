using GameBase;
using NullPointerCore.CoreSystem;
using NullPointerGame.DamageSystem;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	[CreateAssetMenu(menuName="SpaceRTS/AttackAction")]
	public class AttackActionConfig : ScriptableObject
	{
		public Projectile projectilePrefab;
		public float range = 5.0f;
		public bool disarmOnSourceDestroy=false;
		public float cooldown = 1.0f;
		public LayerMask attackMask=-1;

		public virtual float GetAttackRange()
		{
			return range;
		}

		public virtual float GetAttackCooldown()
		{
			return cooldown;
		}

		public virtual Projectile Attack(GameEntity entity, Damageable target)
		{
			if( projectilePrefab==null )
				return null;

			Projectile lastProjectileFired = GameObject.Instantiate<Projectile>( projectilePrefab );
			lastProjectileFired.InitSetup( entity, target );
			return lastProjectileFired;
		}

		public virtual bool IsValidTarget(GameEntity entity, Damageable target)
		{
			if (target == null || entity == null)
				return false;
			if (!target.IsAlive())
				return false;

			return CheckAsDifferentOwnership(entity, target.ThisEntity);
		}

		protected static bool CheckAsDifferentOwnership(GameEntity entity, GameEntity target)
		{
			PlayerControlled ownController = entity.GetComponent<PlayerControlled>();
			PlayerControlled targetController = target.GetComponent<PlayerControlled>();
			if( ownController == targetController )
				return false;
			return (ownController != null && targetController != null && targetController.Owner != ownController.Owner);
		}
	}
}