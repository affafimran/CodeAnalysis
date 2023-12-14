using System.Collections;
using NullPointerCore.Backend.Commands;
using NullPointerGame.DamageSystem;
using NullPointerGame.NavigationSystem;
using UnityEngine.Assertions;
using UnityEngine;
using NullPointerGame.Extras;

namespace SpaceRTS.Commands
{
	public class CmdAttackMove : Command<Navigation>
	{
		const float autoAttackRefreshTime = 2.0f;
		Vector3 targetedPosition;
		Vector3 targetedDirection;
		int attackType;
		//float timeFromLastManeuver = 0;
		float lastAutoAttackRefresh = 0;
		AttackCapable attacker;

		public Vector3 CurrentTargetedPosition { get { return targetedPosition; } }
		public int CurrentAttackType { get { return attackType; } }
		protected bool ShouldUpdateAutoAttack { get { return Time.timeSinceLevelLoad-lastAutoAttackRefresh>autoAttackRefreshTime; } }

		public CmdAttackMove( Vector3 destination, Vector3 dir, int attackType=0 )
		{
			this.targetedPosition = destination;
			this.targetedDirection = dir;
			this.attackType = attackType;
		}

		protected override IEnumerator OnStarted()
		{
			base.OnStarted();

			Assert.IsNotNull(ComponentTarget, "The Navigation component is null.");
			attacker = Context.GetComponent<AttackCapable>();
			ComponentTarget.PrepareToMove(targetedPosition, targetedDirection);
			ComponentTarget.EngageMovement(OnMoveEnded);
			yield return null;
		}

		private void OnMoveEnded(bool successful)
		{
			End();
		}

		protected override void OnUpdate(float time)
		{
			RTSUtilities.DoAutoAttackEnemiesInRange(attacker);
			base.OnUpdate(time);
		}

		protected override IEnumerator OnCanceled()
		{
			ComponentTarget.StopMovement();
			yield return base.OnCanceled();
		}
	}
}
