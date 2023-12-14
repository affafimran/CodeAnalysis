using GameBase;
using NullPointerGame.BuildSystem;
using NullPointerGame.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using System;
using NullPointerGame;
using NullPointerGame.DamageSystem;

namespace SpaceRTS.Core
{
	/// <summary>
	/// ScriptableObject that will be used as base configuration for every unit in the game.
	/// </summary>
	[CreateAssetMenu()]
	public class RTSUnitConfig : UnitConfig
	{
		[Header("RTS Entity Configuration")]
		/// <summary>
		/// The sight radius of this unit
		/// </summary>
		public float sightRadius = 14.0f;

		/// <summary>
		/// The list of resources required to build the unit
		/// </summary>
		public List<PlayerStorageSetter.Entry> resourceRequirement;

		/// <summary>
		/// Image to be displayed in the ui.
		/// </summary>
		[Header("UI Configuration")]
		public Sprite uiImage;
				
		/// <summary>
		/// List of Units that this Static Builder is capable to build.
		/// </summary>
		[Header("Builder Configuration")]
		[FormerlySerializedAs("buildables")]
		public List<UnitConfig> staticBuildables = new List<UnitConfig>();

		/// <summary>
		/// List of Units that this Mobile Builder is capable to build.
		/// </summary>
		public List<UnitConfig> mobileBuildables = new List<UnitConfig>();
		/// <summary>
		/// The build speed for this builder.
		/// </summary>
		public float buildRate = 1.0f;
		/// <summary>
		/// Resource loading rate
		/// </summary>
		[Header("Resource Carrier Configuration")]
		public float loadingRate = 1.0f;
		/// <summary>
		/// Resource unloading rate
		/// </summary>
		public float unloadingRate = 1.0f;

		[Header("Attack Configuration")]
		public List<AttackActionConfig> attacks = new List<AttackActionConfig>(); 
		/// <summary>
		/// The max HP of the unit
		/// </summary>
		[Header("Damageable Configuration")]
		public float damageResistance = 100.0f;

		/// <summary>
		/// Returns the amount of the given resource that is requested to build this unit.
		/// </summary>
		/// <param name="id">ResourceID of the requested resource to be found.</param>
		/// <returns>The amount of resource needed or zero if it fails.</returns>
		public float GetResourceRequirementQuantity(ResourceID id)
		{
			foreach(PlayerStorageSetter.Entry entry in resourceRequirement)
			{
				if(entry.resourceID!=id)
					continue;
				if(entry.action == PlayerStorageSetter.Action.Expand)
					continue;
				return entry.amount;
			}
			return 0.0f;
		}

		/// <summary>
		/// Overrided implementation that extends the setup of a GameEntity with the 
		/// properties provided by this class.
		/// </summary>
		/// <param name="gameEntity">The game entity that needs to be configured.</param>
		public override void SetupGameEntity( GameEntity gameEntity )
		{
			base.SetupGameEntity(gameEntity);

			//// Setups the NavMeshAgent component with the info provided by the UnitConfig.
			NavMeshAgent navAgent = gameEntity.GetComponent<NavMeshAgent>();
			if (navAgent)
				navAgent.radius = radius;

			StaticBuilder staticBuilder = gameEntity.GetComponent<StaticBuilder>();
			if(staticBuilder)
			{
				staticBuilder.buildables = new List<UnitConfig>(staticBuildables);
				staticBuilder.UnitRadius = radius;
				staticBuilder.buildRate = buildRate;
			}
			MobileBuilder mobileBuilder = gameEntity.GetComponent<MobileBuilder>();
			if(mobileBuilder)
			{
				mobileBuilder.buildables = new List<UnitConfig>(mobileBuildables);
				mobileBuilder.UnitRadius = radius;
				mobileBuilder.buildRate = buildRate;
			}
			ResourceCarrier resourceCarrier = gameEntity.GetComponent<ResourceCarrier>();
			if(resourceCarrier)
			{
				resourceCarrier.loadRate = loadingRate;
				resourceCarrier.unloadRate = unloadingRate;
			}
			PlayerStorageSetter playerStorage = gameEntity.GetComponent<PlayerStorageSetter>();
			if(playerStorage)
			{
				foreach(PlayerStorageSetter.Entry entry in resourceRequirement)
				{
					if(entry.action != PlayerStorageSetter.Action.Consume)
						playerStorage.AddStorageInfo(entry);
				}
			}
			AttackCapable attackCapable = gameEntity.GetComponent<AttackCapable>();
			if( attackCapable )
			{
				attackCapable.attacks.Clear();
				foreach( AttackActionConfig attackConfig in attacks )
				{
					AttackCapable.AttackInfo newAttackInfo = new AttackCapable.AttackInfo();
					newAttackInfo.attackConfig = attackConfig;
					attackCapable.attacks.Add(newAttackInfo);
				}
			}

			Damageable damageable = gameEntity.GetComponent<Damageable>();
			if( damageable )
			{
				damageable.damageResistance = damageResistance;
			}

			RTSEntity rtsEntity = gameEntity.GetComponent<RTSEntity>();
			if( rtsEntity )
			{
				rtsEntity.sightDistance = sightRadius;
				rtsEntity.unitConfiguration = this;
			}
			//RTSUnitHUD unitHUD = gameEntity.GetComponent<RTSUnitHUD>();
			//if(unitHUD)
			//{
			//	unitHUD.unitRadius = radius;
			//}
		}
	}
}