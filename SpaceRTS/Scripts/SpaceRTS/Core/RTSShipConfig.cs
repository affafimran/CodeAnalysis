using GameBase;
using NullPointerGame.NavigationSystem;
using UnityEngine;
using UnityEngine.AI;

namespace SpaceRTS.Core
{
	/// <summary>
	/// UnitConfig implementation for ships units (capable of movement)
	/// </summary>
	[CreateAssetMenu()]
	public class RTSShipConfig : RTSUnitConfig
	{
		/// <summary>
		/// Configuration for the ships movement
		/// </summary>
		[Header("Navigation Configuration")]
		public Navigation.Data movementData;
	
		public override void SetupGameEntity( GameEntity gameEntity )
		{
			base.SetupGameEntity(gameEntity);

			Navigation nav = gameEntity.GetComponent<Navigation>();
			if(nav) // Setups the navigation component if the Gameentity have one.
			{
				nav.moveConfig = movementData;
				nav.speed = movementData.maxSpeed;
			}
			NavMeshAgent agent =  gameEntity.GetComponent<NavMeshAgent>();
			if( agent )
			{
				agent.radius = radius;
			}
		}
	}
}