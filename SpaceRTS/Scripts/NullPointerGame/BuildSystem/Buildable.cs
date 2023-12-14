using GameBase;
using GameBase.AttributeExtension;
using NullPointerCore.CoreSystem;
using NullPointerGame.ParkingSystem;
using System;
using UnityEngine;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// GameEntityComponent that allows to this GameEntity to be builded.
	/// </summary>
	public class Buildable : GameEntityComponent
	{
		[SerializeField]
		private ProxyRef parking = new ProxyRef(typeof(Parking), "build_parking");
		[SerializeField][ReadOnly]
		private float current = 0.0f;
		/// <summary>
		/// unit's configuration of this buildable unit. once completed will be used as base configuration 
		/// for the final instantiated GameEntity.
		/// </summary>
		public UnitConfig buildType;
		/// <summary>
		/// points required to build this unit. The build time will be calculated between this property
		/// and the Builder's BuildRate.
		/// </summary>
		public float buildPoints = 100;
		/// <summary>
		/// Triggers each time the construction progress is changed by any builder.
		/// </summary>
		public Action ConstructProgressChanged;
		/// <summary>
		/// Triggered when the construction progress is completed.
		/// </summary>
		public Action ConstructionFinished;

		private Builder builder = null;

		/// <summary>
		/// Returns true if this builable has a builder currently assigned.
		/// </summary>
		public bool HasBuilder { get { return builder; } }
		/// <summary>
		/// Returns true if this builable has a builder currently assigned.
		/// </summary>
		public Builder Builder { get { return builder; } }

		/// <summary>
		/// Was the current build finished
		/// </summary>
		public bool IsConstructionFinished { get { return GetProgress() >= 1.0f; } }
		/// <summary>
		/// UnitConfig for the current type of buildable.
		/// </summary>
		public UnitConfig BuildType { get { return buildType; } }
		/// <summary>
		/// Returns the current used parking system.
		/// </summary>
		public Parking Parking { get { return parking.Get<Parking>(); } }

		/// <summary>
		/// Called when the Visual Module is setted. Here we need to initialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleSetted()
		{
			base.OnVisualModuleSetted();
			parking.SafeAssign(ThisEntity);
		}

		/// <summary>
		/// Called when the Visual Module is removed. Here we need to uninitialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleRemoved()
		{
			parking.SafeClear();
			base.OnVisualModuleRemoved();
		}

		/// <summary>
		/// Sets the current assigned builder for this buildable.
		/// </summary>
		/// <param name="builder">The builder to build this buildable.</param>
		public void SetBuilder(Builder builder)
		{
			this.builder = builder;
		}

		/// <summary>
		/// Increases the current value of progress according with the current build time
		/// </summary>
		/// <param name="value">Value to increase the current progress.</param>
		public void ChangeProgress(float value)
		{
			current += value;
			if(ConstructProgressChanged!=null)
				ConstructProgressChanged.Invoke();
		}

		/// <summary>
		/// Called when the builder is no longer building this buildable.
		/// </summary>
		internal void OnBuilderWorkEnded()
		{
			// Removes the builder reference once no longer working here.
			builder = null;
		}

		/// <summary>
		/// Returns the normalized current percent progress of construction of this buildable. 
		/// </summary>
		/// <returns>Normalized progress of construction. 0 means not started, 1 means completed.</returns>
		public float GetProgress()
		{
			if(buildPoints==0.0f)
				return 1.0f;
			return Mathf.Min( 1.0f,  current / buildPoints);
		}

		[ContextMenu("ChangeToFinalController")]
		public GameEntity ChangeToFinalController()
		{
			ChangeProgress(1.0f);

			if(buildType==null)
			{
				Debug.LogError("Unable to change buildable to Final gameplay controller. buildType is null.", this.gameObject);
				return ThisEntity;
			}

			GameEntity result = buildType.CreateGameplayEntity(	transform.position,
																transform.rotation, 
																transform.parent);
			ComponentProxy visualModule = ThisEntity.VisualProxy;
			ThisEntity.ChangeVisualModule(null);
			result.ChangeVisualModule(visualModule);
			result.transform.SetPositionAndRotation(ThisEntity.transform.position, ThisEntity.transform.rotation);
			result.gameObject.SetActive(true);
			UnitConfig.AssignSameOwnership(ThisEntity, result);

			GameObject.Destroy(gameObject);
			buildType.SetupGameEntity(result);
			return result;

		}
	}
}