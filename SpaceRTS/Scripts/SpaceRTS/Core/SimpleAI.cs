using GameBase;
using NullPointerCore.Backend.Commands;
using NullPointerCore.CoreSystem;
using NullPointerGame.BuildSystem;
using NullPointerGame.DamageSystem;
using NullPointerGame.NavigationSystem;
using NullPointerGame.ResourceSystem;
using SpaceRTS.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceRTS.Core
{
	public class SimpleAI : PlayerSystem
	{
		public Damageable preferedTarget;
		public GameEntity mainStarbase;
		public LayerMask sightMask=-1;
		public int minCarriers = 2;
		public int count;

		private List<AttackCapable> attackers = new List<AttackCapable>();

		// I'm using start in the form of a coroutine in order to optimize the code for the ia
		// if you don't know what it is that IEnumerator:
		// https://forum.unity.com/threads/why-change-void-start-to-ienumerator-start.455280/
		// https://docs.unity3d.com/Manual/Coroutines.html
		public IEnumerator Start ()
		{
			yield return new WaitForSeconds(2.0f);
			// Storing the current idle attackers that the AI has at default when the game starts.
			attackers.AddRange(GetIdleUnits<AttackCapable>(ThisPlayer));
			// Also we are going to need to control all the future spawned units.
			IEnumerable<UnitSpawner> spawners = ThisPlayer.GetOwnUnits<UnitSpawner>();
			foreach( UnitSpawner spawner in spawners)
				spawner.Spawned += OnUnitSpawned;

			while(true)
			{
				// It's not necesary for the ia to have the precision to execute every single frame
				// so, for that we wait five seconds each frame to check if everything it's ok.
				// https://docs.unity3d.com/ScriptReference/WaitForSeconds.html
				// Of course, all this its optional according to the gameplay.
				yield return new WaitForSeconds(1.0f);

				HandleUnitsConstruction();
				// Here you can add the ia code to handle the different aspects of the game
				// also to give order for your ships
				HandleResourceGathering();

				HandleAttackUnits();
			}
			//foreach( UnitSpawner spawner in ThisPlayer.GetOwnUnits<UnitSpawner>())
			//	spawner.Spawned -= OnUnitSpawned;
		}

		private void OnUnitSpawned(GameEntity spawnedEntity)
		{
			AttackCapable attacker = spawnedEntity.GetComponent<AttackCapable>();
			if( attackers!=null )
				attackers.Add(attacker);
		}

		private void HandleAttackUnits()
		{
			foreach( AttackCapable attacker in attackers )
			{
				if( !AttackCapable.IsValid(attacker) )
					continue;
				// Yes, other little hardcode, we asume that it only has one attack, the attack index zero.
				if( attacker.CanUseAttack(0) )
				{
					Navigation nav = attacker.GetComponent<Navigation>();
					CommandController commands = attacker.GetComponent<CommandController>();
					float attackRange = attacker.GetAttackRange(0);
					float sightRange = attackRange * 2;
					// If the unit can't move (A.K.A. Doesn't have a Navigation component)
					if (nav == null) // We use the same attack range as sight range.
						sightRange = attackRange;

					// First we check valid targets inside of the sight range
					Damageable bestTarget = GetBestTarget(attacker, sightRange);
					if (bestTarget != null && !IsAlreadyAttacking(commands, bestTarget))
					{
						if (commands != null) // Attacking the unit
							commands.Set(new CmdAttack(bestTarget, 0));
					}
					else
					if ( preferedTarget != null && nav != null ) // Can we move? and the prefered target is still valid?
					{
						if( commands != null && !commands.HasPendingCommands )
							commands.Set( new CmdAttack(preferedTarget, 0) );
					}
				}
			}
		}

		private bool IsAlreadyAttacking(CommandController attacker, Damageable bestTarget)
		{
			if( !attacker.HasPendingCommands )
				return false;
			if( !(attacker.CurrentCommand is CmdAttack) )
				return false;
			CmdAttack attack = attacker.CurrentCommand as CmdAttack;
			if( attack.CurrentTargetedUnit == bestTarget )
				return true;
			return false;
		}

		private Damageable GetBestTarget(AttackCapable attacker, float sightRange)
		{
			//Damageable result = null;
			Vector3 center = attacker.transform.position;
			Collider [] hits = Physics.OverlapSphere(attacker.transform.position, sightRange, sightMask);
			IOrderedEnumerable<Collider> orderedHits = hits.OrderBy(x => Vector3.SqrMagnitude(x.transform.position-center) );
			foreach( Collider hit in orderedHits )
			{
				Damageable damageable = hit.GetComponentInParent<Damageable>();
				// TODO: Again, the attack type is hardcoded
				if( attacker.IsValidTarget(damageable, 0) )
					return damageable;
			}
			return null;
		}

		private void HandleUnitsConstruction()
		{
			if( mainStarbase != null )
			{
				StaticBuilder builder = mainStarbase.GetComponent<StaticBuilder>();
				// It's the starbase idle?
				if(builder.BuildsInQueue==0)
				{
					// How many carriers we have right now?
					List<ResourceCarrier> carriers = new List<ResourceCarrier>(ThisPlayer.GetOwnUnits<ResourceCarrier>());

					if(carriers.Count < minCarriers)
					{
						// Building two freighters
						// Of course, this is extremly hardcoded but it's just to test porpouse.
						for( int i=carriers.Count; i<minCarriers; i++)
							builder.Build(builder.buildables[1]);
					}
				}
			}
		}

		void HandleResourceGathering()
		{
			IEnumerable<ResourceCarrier> idleMiners = GetIdleUnits<ResourceCarrier>(ThisPlayer);
			foreach(ResourceCarrier miner in idleMiners)
			{
				ResourceWarehouse warehouse = GetClosestResourceDepositFor(miner);
				if(warehouse!=null)
				{
					CommandController commands = miner.GetComponent<CommandController>();
					if(commands!=null)
						commands.Set(new CmdExtractResource(warehouse));
				}
				else
				{
					// There is no warehouse that suits the miner? maybe they are all depleted.
					// Could be best to just decommision of the unit
					// but i don't have that feature yet. maybe delete the unit? ugly. that's your call.
				}
			}
		}

		// Probably this method could be handy to have in the RTSUtilities files, 
		// i see a lot of situations were could be usefull.
		private IEnumerable<T> GetIdleUnits<T>(Player player) where T : GameEntityComponent
		{
			// Iterates through all the Resource carriers 
			foreach(T unit in player.GetOwnUnits<T>())
			{
				CommandController commands = unit.GetComponent<CommandController>();
				if(commands != null && !commands.HasPendingCommands)
					yield return unit;
			}
		}


		// Another handy method that could be in RTSUtilities. I will try to add it in the next version
		private ResourceWarehouse GetClosestResourceDepositFor(ResourceCarrier miner)
		{
			if(miner==null)
				return null;
			PlayerControlled pc = miner.GetComponent<PlayerControlled>();
			if(pc == null || pc.Owner == null)
				return null;

			ResourceWarehouse bestWarehouse = null;
			float bestDistance = 0;
			Player player = pc.Owner;

			// Here i found a problem i don't have any handy method to find the resource deposits
			// I mean i can't use player.GetOwnUnits<ResourceWarehouse>() because the resource deposits
			// doesn't belong to any player.
			//
			// Take into account that GameObject.FindObjectsOfType<ResourceWarehouse>() could be 
			// very expensive so it doesn't be the best solution.
			// but this is a quick help/tutorial so it will serve it's porpuse for now.
			// Just remember to use player.GetOwnUnits<>() the most possible instead of this.
			foreach(ResourceWarehouse rw in GameObject.FindObjectsOfType<ResourceWarehouse>())
			{
				if( miner.IsAbleToLoadCargoFrom(rw) )
				{
					float dist = (miner.transform.position - rw.transform.position).sqrMagnitude;
					if( bestWarehouse == null || dist < bestDistance)
					{
						bestWarehouse = rw;
						bestDistance = dist;
					}
				}
			}
			return bestWarehouse;
		}
	}
}