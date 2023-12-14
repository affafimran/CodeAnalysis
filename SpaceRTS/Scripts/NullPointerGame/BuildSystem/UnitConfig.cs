using GameBase;
using NullPointerCore.CoreSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// Base config for any unit that needs to built by any derived Builder Component
	/// </summary>
	public class UnitConfig : ScriptableObject
	{
		/// <summary>
		/// Name to be displayed in the ui.
		/// </summary>
		[Header("Basic Configuration")]
		public string userName;
		/// <summary>
		/// points required to build this unit. The build time will be calculated between this property
		/// and the Builder's BuildRate.
		/// </summary>
		[FormerlySerializedAs("buildTime")]
		public float buildPoints;
		/// <summary>
		/// Prefab to be used when creating new units of this type. This will be the gameplay part of the unit.
		/// </summary>
		[Header("Build Configuration")]
		public GameObject gameplayPrefab;
		/// <summary>
		/// Prefab to be used when creating new units of this type. This will be the VisualModule part of the unit.
		/// </summary>
		public GameObject visualPrefab;
		/// <summary>
		/// Prefab to be used by the GhostControl component when the builders requires a location for the structure
		/// when trying to build it.
		/// </summary>
		public GameObject placementPrefab;
		/// <summary>
		/// Base radius of the unit to prevent collisions.
		/// </summary>
		public float radius = 2.4f;


		public virtual void SetupGameEntity( GameEntity gameEntity )
		{
			Buildable buildable = gameEntity.GetComponent<Buildable>();
			if(buildable)
			{
				buildable.buildPoints = buildPoints;
			}
		}

		public GameEntity SpawnFinal(GameEntity builder, Vector3 position, Quaternion rotation)
		{
			Transform parent = builder!=null ? builder.transform.parent : GameScene.EntitiesParent;
			GameEntity resultEntity = CreateGameplayEntity( position, rotation, parent);
			ComponentProxy visualModule = CreateVisual(resultEntity.transform);
			resultEntity.ChangeVisualModule(visualModule);
			resultEntity.gameObject.SetActive(true);
			UnitConfig.AssignSameOwnership(builder, resultEntity);
			SetupGameEntity(resultEntity);
			return resultEntity;
		}

		public GameEntity CreateGameplayEntity(Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = GameObject.Instantiate<GameObject>(gameplayPrefab, position, rotation, parent);
			GameEntity ge = go.GetComponent<GameEntity>();
			ge.gameObject.name = name;
			return ge;
		}

		/// <summary>
		/// Creates the VisualModule GameObject specified in the VisualPrefab property.
		/// </summary>
		/// <param name="parent">Transform that'll become the parent of the created VisualModule.</param>
		/// <returns>Returns the ComponentProxy component attached to the Root of the created VisualModule.</returns>
		public ComponentProxy CreateVisual(Transform parent)
		{
			GameObject goVisual = GameObject.Instantiate<GameObject>(visualPrefab, parent.position, parent.rotation, parent);
			goVisual.name = userName;
			return goVisual.GetComponent<ComponentProxy>();
		}

		/// <summary>
		/// Takes the src GameEntity owner and assigns it to the dst GameEntity.
		/// Useful for units construction to assign same ownership.
		/// </summary>
		/// <param name="src">The source game entity twhere to take the player owner.</param>
		/// <param name="dst">The destination game entity at which assign the player owner.</param>
		public static void AssignSameOwnership(GameEntity src, GameEntity dst)
		{
			if( src==null || dst==null )
				return;
			PlayerControlled pcDst = dst.GetComponent<PlayerControlled>();
			PlayerControlled pcSrc = src.GetComponent<PlayerControlled>();
			if( pcDst != null && pcSrc != null )
				pcDst.Owner = pcSrc.Owner;
		}
	}
}