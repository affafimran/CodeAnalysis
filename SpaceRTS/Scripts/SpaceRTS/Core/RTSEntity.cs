using System;
using GameBase;
using NullPointerCore.Backend.ResourceGathering;
using NullPointerCore.CoreSystem;
using NullPointerGame.BuildSystem;
using NullPointerGame.Extras;
using NullPointerGame.ResourceSystem;
using UnityEngine;
using SpaceRTS.UI;
using NullPointerGame.DamageSystem;
using NullPointerGame.NavigationSystem;
using NullPointerCore.Backend.Commands;
using SpaceRTS.Commands;
using System.Linq;
using System.Collections;

namespace SpaceRTS.Core
{
	/// <summary>
	/// Implements GameEntity and provides extra functionality for the RTS.
	/// </summary>
	[RequireComponent(typeof(PlayerControlled))]
	public class RTSEntity : GameEntityComponent
	{
		/// <summary>
		/// The UnitConfig of this RTSEntity
		/// </summary>
		public RTSUnitConfig unitConfiguration;
		/// <summary>
		/// UnitConfig of this RTSEntity.
		/// </summary>
		public RTSUnitConfig Config { get { return unitConfiguration; } }
		/// <summary>
		/// Cache for the PlayerControlled component
		/// </summary>
		private PlayerOwner pc;
		/// <summary>
		/// Indicates that this unit has autotargeting by default, meaning that should 
		/// try to attack when enemy becomes at sight.
		/// </summary>
		public bool useAutoTargeting = true;
		public float autoAttackRefreshTime = 4.0f;
		public float sightDistance = 14.0f;

		//private float lastAutoAttackRefresh = 0;

		private Damageable damageable = null;
		private AttackCapable attack = null;
		private Navigation nav = null;
		private CommandController commands = null;

		protected AttackCapable Attack { get { if( attack==null ) attack = GetComponent<AttackCapable>(); return attack; } }
		protected Damageable Damage { get { if( damageable==null ) damageable = GetComponent<Damageable>(); return damageable; } }
		protected Navigation Nav { get { if( nav==null ) nav = GetComponent<Navigation>(); return nav; } }
		protected CommandController Commands { get { if( commands==null ) commands = GetComponent<CommandController>(); return commands; } }

		//protected bool ShouldUpdateAutoAttack { get { return Time.timeSinceLevelLoad-lastAutoAttackRefresh>autoAttackRefreshTime; } }
		/// <summary>
		/// Registers the GameEntity in the RTSPlayer controller.
		/// Setup all the required components.
		/// </summary>
		protected void Start()
		{
			pc = new PlayerOwner(this, null);
			foreach(Builder builder in GetComponents<Builder>())
			{
				builder.BuildStarted += delegate { ConsumeBuildedResources(builder); };
				builder.UnableToBuild += OnUnableToBuild;
				builder.BuildConditionals.Add( CanBeBuilt );
			}
			if( Damage != null )
				Damage.UnitDestroyed += OnUnitDestroyed;
		}

		private void OnUnitDestroyed()
		{
			if( Attack != null )
			{
				Attack.enabled = false;
				Attack.TryToDisarmAllAttacks();
			}
			if( Commands != null )
				Commands.Stop();
		}

		protected virtual void Update()
		{
			CheckAutoAttack();
		}

		private void CheckAutoAttack()
		{
			if (Commands == null || Commands.HasPendingCommands )
				return;
			//if( ShouldUpdateAutoAttack )
			//	return;
			if(!AttackCapable.IsValid(Attack))
				return;

			for (int i = 0; i < Attack.TotalAttacksCount; i++)
			{
				if (!Attack.CanUseAttack(i))
					continue;
				//lastAutoAttackRefresh = Time.timeSinceLevelLoad;
				float attackRange = Attack.GetAttackRange(i);
				float sightRange = sightDistance;
				LayerMask attackMask = Attack.GetAttackMask(i);
				// If the unit can't move (A.K.A. Doesn't have a Navigation component)
				if (Nav == null) // We use the same attack range as sight range.
					sightRange = attackRange;

				// First we check valid targets inside of the sight range
				Damageable bestTarget = RTSUtilities.GetBestAttackTarget(Attack, sightRange, attackMask);
				if (bestTarget != null && !IsAlreadyAttacking(Commands, bestTarget))
				{
					if (commands != null) // Attacking the unit
						commands.Set(new CmdAttack(bestTarget, i));
				}
			}
		}

		private bool CanBeBuilt(Buildable toBuild)
		{
			if(!PlayerOwner.HasValidOwner(pc))
				return true;
			// If the build cost wasn't paid, then we must check if we have enough resources to pay.
			if(toBuild.GetProgress() > 0.0f )
				return true;

			StorageContainer playerStorage = pc.Player.GetComponent<StorageContainer>();
			if(playerStorage!=null)
			{
				RTSUnitConfig unitConfig = toBuild.BuildType as RTSUnitConfig;
				return playerStorage.HasEnough(unitConfig.resourceRequirement);
			}

			//// Example code to show how to add more restrictions to the build construction.
			//// Maybe there is a max limit for certain units in the game. That can be solved with the next check.
			//// Take into account that you also need to add maxUnits for the UnitConfig script.
			//// Also probably you need to add some restrictions in the UI to prevent the user to add it to the
			//// queue in the first place.
			//int unitsCount = 0;
			//if( toBuild.maxUnits > 0 )
			//{
			//	foreach( RTSEntity ent in pc.Player.GetOwnUnits<RTSEntity>() )
			//	{
			//		if( ent.unitConfiguration == toBuild )
			//			unitsCount++;
			//	}
			//	if( unitsCount >= toBuild.maxUnits )
			//		return false;
			//}

			// If we reach this point it's because everything it's in order for the construction.
			return true;
		}

		/// <summary>
		/// Consumes the required resources to build the current targeted buildable.
		/// </summary>
		/// <param name="builder"></param>
		public void ConsumeBuildedResources(Builder builder)
		{
			if(!PlayerOwner.HasValidOwner(pc))
				return;

			StorageContainer playerStorage = pc.Player.GetComponent<StorageContainer>();
			if(builder!=null && builder.BuildTarget!=null)
			{
				// We must only pay the cost if the build progress was not already started.
				if(builder.BuildTarget.GetProgress() == 0.0f )
				{
					RTSUnitConfig unitConfig = builder.BuildTarget.BuildType as RTSUnitConfig;
					if(unitConfig!=null)
					{
						foreach(PlayerStorageSetter.Entry entry in unitConfig.resourceRequirement)
						{
							if( entry.action == PlayerStorageSetter.Action.Consume )
							{
								Storage storage = playerStorage.Get(entry.resourceID);
								if(storage!=null)
									storage.Consume(entry.amount);
							}
						}
					}
				}
			}
		}

		private void OnUnableToBuild()
		{
			RTSMessageDisplay.Show("There is not enough resources.");
		}

		public static bool IsAlreadyAttacking(CommandController attacker, Damageable bestTarget)
		{
			if( attacker == null )
				return false;
			if( !attacker.HasPendingCommands )
				return false;
			if( !(attacker.CurrentCommand is CmdAttack) )
				return false;
			CmdAttack attack = attacker.CurrentCommand as CmdAttack;
			if( attack.CurrentTargetedUnit == bestTarget )
				return true;
			return false;
		}

	}
}