using GameBase;
using NullPointerGame.NavigationSystem;
using System.Collections;
using UnityEngine;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// Gives the capacity to the entity to build mobile units in a fixed position and 
	/// expel it after completed.
	/// <p>This component assumes that the builder unit is a static entity (meaning that 
	/// can’t move through the map), so it will built the specific units in its interior 
	/// and will “eject” the units after the build process it’s complete. The most common 
	/// use will be a static structure building some ships as we can see in the starbase unit.</p>
	/// </summary>
	public class StaticBuilder : Builder
	{
		/// <summary>
		/// Container for the transform holding the spawn position position.
		/// </summary>
		[Tooltip("Container for the transform holding the spawn position position.")]
		[SerializeField]
		private ProxyRef buildPoint = new ProxyRef(typeof(Transform), "cp_spawn");

		/// <summary>
		/// Container for the transform holding the expel point position.
		/// </summary>
		[Tooltip("Container for the transform holding the expel point position.")]
		[SerializeField]
		private ProxyRef expelPoint = new ProxyRef(typeof(Transform), "cp_buildout");

		/// <summary>
		/// Container for the transform holding the rally point position.
		/// </summary>
		[Tooltip("Buildable spawn position.")]
		[SerializeField]
		private ProxyRef rallyPoint = new ProxyRef(typeof(Transform), "cp_rallypoint");
		
		/// <summary>
		/// The expel speed for the built units.
		/// </summary>
		public float expelSpeed = 4.0f;

		/// <summary>
		/// Returns the position where the units will be built in world coordinates.
		/// </summary>
		public Vector3 BuildLocation { get { return buildPoint.Get<Transform>().position; } }
		/// <summary>
		/// Returns the position in world coordinates where the units will be expelled once built.
		/// </summary>
		public Vector3 BuildExpelLocation { get { return expelPoint.Get<Transform>().position; } }

		public Vector3 BuildLookAtDir { get { return (BuildExpelLocation - BuildLocation).normalized; }}
		/// <summary>
		/// Indicated if the builder must delete the buildable unit when interrupted.
		/// </summary>
		public override bool RemoveOnInterrupt { get { return true; } }
		/// <summary>
		/// The position of the rally point that will be used by the units once completed. 
		/// </summary>
		public Vector3 RallyPointPosition { get { return rallyPoint.Get<Transform>().position; } }
		/// <summary>
		/// Changes the rally point position for the built units after been "expelled".
		/// </summary>
		/// <param name="newPosition"></param>
		public void ChangeRallyPointPosition(Vector3 newPosition)
		{
			rallyPoint.Get<Transform>().position = newPosition;
		}
		/// <summary>
		/// Returns the transform that must be used as parent for the instantiated buildable.
		/// overrided method to make this function to return the current transform to be used as parent.
		/// </summary>
		/// <returns>The transform that must be used as parent for the instantiated buildable.</returns>
		protected override Transform GetBuildableParent()
		{
			return this.transform;
		}

		override public void OnVisualModuleSetted()
		{
			base.OnVisualModuleSetted();
			buildPoint.SafeAssign(ThisEntity);
			expelPoint.SafeAssign(ThisEntity);
			rallyPoint.SafeAssign(ThisEntity);
		}

		public override void OnVisualModuleRemoved()
		{
			buildPoint.SafeClear();
			expelPoint.SafeClear();
			rallyPoint.SafeClear();
			base.OnVisualModuleRemoved();
		}

		protected override void OnBuildRequested(UnitConfig toBuild)
		{
			AddUnitToBuild(toBuild, BuildLocation, BuildLookAtDir);
		}

		protected override void OnWorkOverBuildableRequested(Buildable buildable)
		{
			StartBuildProgress();
		}

		protected override void OnBuildCompleted()
		{
			ExpelUnitBuilded();
			base.OnBuildCompleted();
		}

		private void ExpelUnitBuilded()
		{
			if(!HasTarget)
				return;

			StartCoroutine(DoExpelingUnitBuilded());
		}

		protected IEnumerator DoExpelingUnitBuilded()
		{
			Buildable expeledUnit = BuildTarget;
			Vector3 expelPoint = BuildExpelLocation;
			float stopDistance  = 1.0f;
			Transform tr = expeledUnit.transform;

			while( Vector3.SqrMagnitude(tr.position - expelPoint) > (stopDistance * stopDistance) )
			{
				tr.position = Vector3.Lerp(tr.position, expelPoint, expelSpeed * Time.deltaTime);
				yield return null;
			}
			expeledUnit.transform.SetParent(this.transform.parent, true);
			GameEntity finalUnit = expeledUnit.ChangeToFinalController();

			Navigation nav = finalUnit.GetComponent<Navigation>();
			if (nav != null)
			{
				nav.PrepareToMove(RallyPointPosition, RallyPointPosition - expeledUnit.transform.position);
				nav.EngageMovement();
			}
		}
	}
}