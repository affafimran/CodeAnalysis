using System.Collections;
using NullPointerCore.Backend.Commands;
using NullPointerGame.DamageSystem;
using NullPointerGame.NavigationSystem;
using UnityEngine.Assertions;
using UnityEngine;
using NullPointerGame.Spatial;
using System;
using NullPointerGame.Extras;

namespace SpaceRTS.Commands
{
	public class CmdAttack : Command<AttackCapable>
	{
		const float timeToChangeManeuverPosition = 5.0f;
		Navigation nav;
		Damageable targetedUnit;
		int attackType;
		float timeFromLastManeuver = 0;

		public Damageable CurrentTargetedUnit { get { return targetedUnit; } }
		public int CurrentAttackType { get { return attackType; } }

		public CmdAttack( Damageable target, int attackType )
		{
			this.targetedUnit = target;
			this.attackType = attackType;
		}

		protected override IEnumerator OnStarted()
		{
			base.OnStarted();

			Assert.IsNotNull(ComponentTarget, "The AttackCapable component is null.");
			nav = Context.GetComponent<Navigation>();
			
			if( nav != null )
			{
				if( !ComponentTarget.IsInAttackRange(targetedUnit, attackType) )
				{
					nav.speed = nav.moveConfig.maxSpeed;
					nav.stoppingDistance = ComponentTarget.GetAttackDistance(targetedUnit, attackType);
					nav.PrepareToPursuit(targetedUnit.transform, Vector3.zero, Vector3.zero);
					nav.EngageMovement(OnMoveEnded);
				}
			}
			yield return null;
		}

		private void OnMoveEnded(bool successful)
		{
			if( nav != null )
				SetupNewAttackPosition();
		}

		private void SetupNewAttackPosition()
		{
			if( targetedUnit == null )
			{
				Cancel();
				return;
			}
			// if we are already in attack range we are going to move a little to avoid being an "easy" 
			// target at the same time. For that we are going to move to a random point inside of the 
			// attack range which is going to be perceived as maneuvers around the target.
			Vector3 currentOffsetDirection = nav.BasePosition - targetedUnit.transform.position;
			float angle = UnityEngine.Random.Range(-110.0f, 110.0f);
			float attackDistance = ComponentTarget.GetAttackDistance(targetedUnit, attackType);
			float newDistance = UnityEngine.Random.Range(targetedUnit.GetRadius()*1.3f, attackDistance*0.96f);
			Vector3 newTargetOffset = Quaternion.Euler(0, angle, 0) * currentOffsetDirection.normalized * newDistance;

			nav.stoppingDistance = 0.1f;
			nav.speed = 0.6f * nav.moveConfig.maxSpeed;
			nav.PrepareToPursuit(targetedUnit.transform, newTargetOffset, Vector3.zero);
			nav.EngageMovement(OnMoveEnded);
			timeFromLastManeuver = Time.timeSinceLevelLoad;
		}

		protected override void OnUpdate(float time)
		{
			if( !ComponentTarget.IsValidTarget(targetedUnit, attackType) )
				End();
			if( ComponentTarget.IsInAttackRange(targetedUnit, attackType) )
			{
				if( ComponentTarget.CanUseAttack(attackType) )
					ComponentTarget.Attack(targetedUnit, attackType);
			}
			else
				RTSUtilities.DoAutoAttackEnemiesInRange(ComponentTarget);
			// This little trick is to avoid uggly behaviors if the ships is stuck during the attack
			if( nav != null && Time.timeSinceLevelLoad - timeFromLastManeuver > timeToChangeManeuverPosition )
			{
				// If there was too much time since the last time that the maneuver position was reached, 
				// then we force to change the maneuver destination
				SetupNewAttackPosition();
			}
			base.OnUpdate(time);
		}

		protected override IEnumerator OnCanceled()
		{
			if( nav != null )
				nav.StopMovement();
			return base.OnCanceled();
		}

		protected override IEnumerator OnEnded()
		{
			if( nav != null )
				nav.StopMovement();
			return base.OnEnded();
		}
	}
}
