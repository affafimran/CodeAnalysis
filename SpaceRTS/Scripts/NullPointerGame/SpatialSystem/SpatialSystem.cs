using GameBase;
using GameBase.RTSKit;
using NullPointerGame.Extras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// Act as a Manager for the spatial Navigation of the map.
	/// Can be overrided to allow different Navigation methods like the Unity's
	/// built-in NavMesh system or A* Pathfinding project.
	/// </summary>
	public class SpatialSystem : GameSceneSystem
	{
		private SceneBounds cachedSceneBounds = null;
		protected List<SpatialModifier> modifiers = new List<SpatialModifier>(); 
		IEnumerator rebuildOperation = null;
		bool requestRebuild = false;


		/// <summary>
		/// Action delegate called every time a SpatialModifier is registered
		/// </summary>
		public Action<SpatialModifier> OnSpatialModifierAdded;
		/// <summary>
		/// Action delegate called every time a SpatialModifier is unregistered
		/// </summary>
		public Action<SpatialModifier> OnSpatialModifierRemoved;
		/// <summary>
		/// Action dellegate called every time a rebuild is completed.
		/// </summary>
		public Action OnRebuildCompleted;

		/// <summary>
		/// Default value when all areas must be marked in a AreaMask
		/// </summary>
		public virtual int DefaultAllAreasMask { get { return -1; } }

		/// <summary>
		/// Returns the list of area names (useful for editor classes to display an int field mask
		/// with the proper layer names). Returning null implies that the current NavMesh layer 
		/// names must be used.
		/// </summary>
		/// <returns>A string array will a the layer area names for the spatial system.</returns>
		public virtual string [] GetAreaNames()
		{
			return null;
		}

		/// <summary>
		/// Finds the closest point into the navigation area.
		/// </summary>
		/// <param name="pingPosition">The origin of the sample query.</param>
		/// <param name="closestPoint">the resulting location.</param>
		/// <param name="masks">A mask specifying which areas are allowed when finding the nearest point.</param>
		/// <returns>True if a nearest point is found.</returns>
		public virtual bool SamplePosition(Vector3 pingPosition, out Vector3 closestPoint, int masks)
		{
			closestPoint = pingPosition;
			return true;
		}

		/// <summary>
		/// Locate the closest edge distance from a point on the Navigation spatial areas.
		/// </summary>
		/// <param name="pingPosition">The origin of the distance query.</param>
		/// <param name="distance">Holds the resulting distance.</param>
		/// <param name="masks">A bitfield mask specifying which areas can be passed when finding the nearest edge.</param>
		/// <returns>True if a nearest edge is found.</returns>
		public virtual bool GetClosestEdgeDistance(Vector3 pingPosition, out float distance, int masks)
		{
			SceneBounds bounds = GetSceneBounds();
			Bounds bound = bounds.boundsCollider.bounds;
			Vector3 [] vertices = MeshUtilities.GenerateBoxVertices(bound.size, bound.center);
			Vector3 closestPoint = pingPosition.GetClosestPoint(vertices);
			distance = Vector3.Distance(pingPosition, closestPoint);
			return true;
		}

		/// <summary>
		/// Locate the closest edge position from a point on the Navigation spatial areas.
		/// </summary>
		/// <param name="pingPosition">The origin of the distance query.</param>
		/// <param name="edgePosition">Holds the resulting position.</param>
		/// <param name="masks">A bitfield mask specifying which areas can be passed when finding the nearest edge.</param>
		/// <returns>True if a nearest edge is found.</returns>
		public virtual bool GetClosestEdgePosition(Vector3 pingPosition, out Vector3 edgePosition, int masks)
		{
			SceneBounds bounds = GetSceneBounds();
			Bounds bound = bounds.boundsCollider.bounds;
			Vector3 [] vertices = MeshUtilities.GenerateBoxVertices(bound.size, bound.center);
			Vector3 closestPoint = pingPosition.GetClosestPoint(vertices);
			//Vector3 closestPoint = MeshUtilities.GetClosestPoint(pingPosition, vertices);
			//Vector3 closestPoint = bounds.boundsCollider.ClosestPointOnBounds(pingPosition);
			edgePosition = closestPoint;
			return true;
		}

		/// <summary>
		/// Gets the current used SceneBounds.
		/// </summary>
		/// <returns>The current used SceneBounds.</returns>
		private SceneBounds GetSceneBounds()
		{
			if( cachedSceneBounds != null )
				return cachedSceneBounds;
			cachedSceneBounds = gameScene.Get<SceneBounds>();
			return cachedSceneBounds;
		}

		/// <summary>
		/// Forces to rebuild the entire spatial system.
		/// This method only marks the system as a rebuild required, the actual rebuild will be done
		/// at the of the frame so multiple calls to this function can be made without overhead penalty.
		/// </summary>
		public void Rebuild()
		{
			if(!this.isActiveAndEnabled)
				return;
			requestRebuild=true;
			if(rebuildOperation==null)
				StartCoroutine(rebuildOperation = DoRebuild());
		}

		public bool IsRebuilding()
		{
			return rebuildOperation!=null;
		}
		public void StopRebuild()
		{
			if(rebuildOperation!=null)
				StopCoroutine(rebuildOperation);
			rebuildOperation = null;
		}

		/// <summary>
		/// The actual Rebuild routine
		/// </summary>
		/// <returns></returns>
		private IEnumerator DoRebuild()
		{
			while(requestRebuild)
			{
				requestRebuild = false;
				yield return OnRebuild();
			}
			rebuildOperation = null;
			if(OnRebuildCompleted!=null)
				OnRebuildCompleted.Invoke();
		}

		/// <summary>
		/// Called along with the rebuild process.
		/// Should be overriden to implement a custon rebuild process.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator OnRebuild()
		{
			yield return null;
		}

		/// <summary>
		/// Creates an enumeration of each SpatialModifier that should shape the 
		/// navigation areas of the spatial system.
		/// TODO: I'm pretty sure that a lot of optimizations can be done here. 
		/// I'll mark it as a big TODO and come back later to this.
		/// </summary>
		public IEnumerable<SpatialModifier> Modifiers
		{
			get
			{
				HashSet<SpatialModifier> hash = new HashSet<SpatialModifier>();
				foreach( SpatialModifierCollector c in SpatialModifierCollector.collectors )
				{
					foreach(SpatialModifier m in c.modifiers)
						hash.Add(m);
				}
				foreach(SpatialModifier m in modifiers)
					hash.Add(m);
#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					foreach( SpatialUpdate su in GameObject.FindObjectsOfType<SpatialUpdate>() )
					{
						su.CollectModifiers();
						foreach(SpatialModifier m in su.Modifiers)
							hash.Add(m);
					}
				}
#endif
				return hash;
			}
		}

		/// <summary>
		/// Registers a bunch of SpatialModifiers that will shape the navigation areas of 
		/// this SpatialSystem after a rebuild.
		/// </summary>
		/// <param name="addedModifiers">An enumeration of the SpatialModifiers that need to be registered.</param>
		public virtual void AddVolumeModifiers(IEnumerable<SpatialModifier> addedModifiers)
		{
			foreach(SpatialModifier m in addedModifiers)
			{
				modifiers.Add(m);
				if(OnSpatialModifierAdded!=null)
					OnSpatialModifierAdded.Invoke(m);
			}
		}

		/// <summary>
		/// Unregisters a bunch of SpatialModifiers from the SpatialSystem.
		/// </summary>
		/// <param name="removedModifiers">The enumeration of SpatialModifiers that need to be unregistered.</param>
		public virtual void RemoveVolumeModifiers(IEnumerable<SpatialModifier> removedModifiers)
		{
			foreach(SpatialModifier m in removedModifiers)
			{
				modifiers.Remove(m);
				if(OnSpatialModifierRemoved!=null)
					OnSpatialModifierRemoved.Invoke(m);
			}
		}

	}
}