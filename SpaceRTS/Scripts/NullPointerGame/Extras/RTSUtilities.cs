using GameBase;
using NullPointerCore.Backend.ResourceGathering;
using NullPointerCore.CoreSystem;
using NullPointerGame.DamageSystem;
using NullPointerGame.NavigationSystem;
using NullPointerGame.ResourceSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NullPointerGame.Extras
{
	/// <summary>
	/// Common helper methods for SpaceRTS specific jobs.
	/// </summary>
	public class RTSUtilities
	{
		static public bool CanMoveAndAttack( GameEntity ge )
		{
			Navigation nav = ge.GetComponent<Navigation>();
			if( nav == null )
				return false;
			AttackCapable attackCapable = ge.GetComponent<AttackCapable>();
			return AttackCapable.IsValid(attackCapable);
		}

		static public void TransferStoredToLocalPlayer(GameEntity ge)
		{
			if(ge==null)
				return;
			PlayerControlled pc = ge.GetComponent<PlayerControlled>();
			if(pc==null)
			{
				Debug.LogWarning("Unable to transfer. Requires a PlayerControlled attached.", ge);
				return;
			}
			if( pc.Owner == null )
			{
				Debug.LogWarning("Unable to Transfer. This GameEntity doesn't belong to any player. ", ge);
				return;
			}
			StorageContainer localContainer = ge.GetComponent<StorageContainer>();
			StorageContainer playerContainer = pc.Owner.GetComponent<StorageContainer>();
			TransferAll(localContainer, playerContainer);
		}

		static public void TransferAll(StorageContainer source, StorageContainer dest)
		{
			if(source==null || dest==null | source==dest)
				return;
			foreach(Storage sourceStorage in source.Storages)
				sourceStorage.Transfer(dest.Get(sourceStorage.ResourceID));
		}

		static public ResourceWarehouse GetNearestPlayerStorageWarehouse(ResourceCarrier carrier)
		{
			if(carrier==null)
				return null;
			PlayerControlled pc = carrier.GetComponent<PlayerControlled>();
			if(pc == null || pc.Owner == null)
				return null;

			ResourceWarehouse bestWarehouse = null;
			float bestDistance = 0;
			foreach(ResourceWarehouse rw in pc.Owner.GetOwnUnits<ResourceWarehouse>())
			{
				if( rw.ThisEntity == carrier.ThisEntity )
					continue;
				foreach( ResourceID rid in carrier.GetLoadedCargoIDs() )
				{
					if( rw.CanInsert(rid) )
					{
						float dist = (carrier.transform.position - rw.transform.position).sqrMagnitude;
						if( bestWarehouse == null || dist < bestDistance)
						{
							bestWarehouse = rw;
							bestDistance = dist;
						}
					}
				}
			}
			return bestWarehouse;
		}

		static public List<T> PingNearestGameEntitiesFromPlayer<T>(Vector3 centerPosition, Player player, float maxDistance=1000.0f) where T : Component 
		{
			return new List<T>(player.GetOwnUnits<T>().OrderBy( x => (centerPosition - x.transform.position).sqrMagnitude ));
		}

		static public Damageable GetBestAttackTarget(AttackCapable attacker, float sightRange, LayerMask sightMask)
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

		static public void DoAutoAttackEnemiesInRange(AttackCapable attacker)
		{
			if (attacker != null && /*ShouldUpdateAutoAttack &&*/ AttackCapable.IsValid(attacker))
			{
				for (int i = 0; i < attacker.TotalAttacksCount; i++)
				{
					if (!attacker.CanUseAttack(i))
						continue;
					//lastAutoAttackRefresh = Time.timeSinceLevelLoad;
					float attackRange = attacker.GetAttackRange(i);
					LayerMask attackMask = attacker.GetAttackMask(i);
					// First we check valid targets inside of the sight range
					Damageable bestTarget = RTSUtilities.GetBestAttackTarget(attacker, attackRange, attackMask);
					if (bestTarget != null)
					{
						if (attacker.CanUseAttack(i))
							attacker.Attack(bestTarget, i);
					}
				}
			}
		}
	}
}