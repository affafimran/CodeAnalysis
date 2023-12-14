using NullPointerGame.Extras;
using NullPointerGame.Spatial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NullPointerGame.NavMeshIntegration
{
	/// <summary>
	/// Special implementation of the SpatialSystem to use with the Unity's built-in NavMesh.
	/// </summary>
	[DefaultExecutionOrder(-102)]
	public class NavMeshSpatialSystem : SpatialSystem
	{
		// Do not serialize - runtime only state.
		AsyncOperation m_Operation;

		/// <summary>
		/// Default value when all areas must be marked in a AreaMask
		/// </summary>
		public override int DefaultAllAreasMask { get { return NavMesh.AllAreas; } }

		/// <summary>
		/// Finds the closest point into the navigation area.
		/// </summary>
		/// <param name="pingPosition">The origin of the sample query.</param>
		/// <param name="closestPoint">the resulting location.</param>
		/// <param name="masks">A mask specifying which areas are allowed when finding the nearest point.</param>
		/// <returns>True if a nearest point is found.</returns>
		public override bool SamplePosition(Vector3 pingPosition, out Vector3 closestPoint, int masks)
		{
			NavMeshHit hit;
			closestPoint = pingPosition;
			if( NavMesh.SamplePosition(pingPosition, out hit, 1.0f, masks) )
			{
				closestPoint = hit.position;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Locate the closest edge distance from a point on the Navigation spatial areas.
		/// </summary>
		/// <param name="pingPosition">The origin of the distance query.</param>
		/// <param name="distance">Holds the resulting distance.</param>
		/// <param name="masks">A bitfield mask specifying which areas can be passed when finding the nearest edge.</param>
		/// <returns>True if a nearest edge is found.</returns>
		public override bool GetClosestEdgeDistance(Vector3 pingPosition, out float distance, int masks)
		{
			NavMeshHit hit;
			distance = 0.0f;
			if( NavMesh.FindClosestEdge(pingPosition, out hit, masks) )
			{
				distance = hit.distance;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Locate the closest edge position from a point on the Navigation spatial areas.
		/// </summary>
		/// <param name="pingPosition">The origin of the distance query.</param>
		/// <param name="edgePosition">Holds the resulting position.</param>
		/// <param name="masks">A bitfield mask specifying which areas can be passed when finding the nearest edge.</param>
		/// <returns>True if a nearest edge is found.</returns>
		public override bool GetClosestEdgePosition(Vector3 pingPosition, out Vector3 edgePosition, int masks)
		{
			NavMeshHit hit;
			edgePosition = pingPosition;
			if( NavMesh.FindClosestEdge(pingPosition, out hit, masks) )
			{
				edgePosition = hit.position;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Registers a bunch of SpatialModifiers that will shape the navigation areas of 
		/// this SpatialSystem and also forces the rebuild operation.
		/// </summary>
		/// <param name="modifiers">An enumeration of the SpatialModifiers that need to be registered.</param>
		public override void AddVolumeModifiers(IEnumerable<SpatialModifier> modifiers)
		{
			base.AddVolumeModifiers(modifiers);
			Rebuild();
		}

		/// <summary>
		/// Unregisters a bunch of SpatialModifiers that will shape the navigation areas of 
		/// this SpatialSystem and also forces the rebuild operation.
		/// </summary>
		/// <param name="modifiers">An enumeration of the SpatialModifiers that need to be registered.</param>
		public override void RemoveVolumeModifiers(IEnumerable<SpatialModifier> modifiers)
		{
			base.RemoveVolumeModifiers(modifiers);
			Rebuild();
		}

		/// <summary>
		/// Called along with the rebuild process.
		/// Handle the NavMesh rebuild operation for each defined surface.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerator OnRebuild()
		{
			foreach(NavMeshSurface surf in NavMeshSurface.activeSurfaces)
			{
				if(surf.navMeshData != null)
				{
					m_Operation = surf.UpdateNavMesh(surf.navMeshData);
					yield return m_Operation;
				}
				else
					surf.BuildNavMesh();
			}
		}

		/// <summary>
		/// Collect the NavMeshBuildSources that can be formed with the registered SpatialModifiers.
		/// This method it's called from NavMeshSurface for the proper NavMesh creation.
		/// </summary>
		/// <param name="sources"></param>
		public void CollectSources( ref List<NavMeshBuildSource> sources)
		{
			CollectSources(ref sources, Modifiers);
		}

		void CollectSources(ref List<NavMeshBuildSource> sources, IEnumerable<SpatialModifier> modifiers)
		{
			foreach(SpatialModifier m in modifiers)
			{
				CollectModifierSources(ref sources, m as SpatialBoxModifier);
				//CollectModifierSources(ref sources, m as SpatialMeshModifier);
				CollectModifierSources(ref sources, m as SpatialCylinderModifier);
				CollectModifierSources(ref sources, m as SpatialCircleModifier);
			}
		}

		void CollectModifierSources(ref List<NavMeshBuildSource> sources, SpatialBoxModifier volume)
		{
			if(volume==null)
				return;
			NavMeshBuildSource s = new NavMeshBuildSource();
			s.shape = NavMeshBuildSourceShape.Box;
            s.transform = Matrix4x4.TRS(volume.Center, volume.transform.rotation, Vector3.one);
            s.size = volume.Size;
            s.area = volume.area;

			sources.Add(s);	
		}

		//void CollectModifierSources(ref List<NavMeshBuildSource> sources, SpatialMeshModifier m)
		//{
		//	if (m == null || m.Mesh == null)
		//		return;
		//	NavMeshBuildSource s = new NavMeshBuildSource();
		//	s.shape = NavMeshBuildSourceShape.Mesh;
		//	s.sourceObject = m.Mesh;
		//	s.transform = m.transform.localToWorldMatrix;
		//	s.area = m.area;
		//	sources.Add(s);
		//}

		void CollectModifierSources(ref List<NavMeshBuildSource> sources, SpatialCylinderModifier m)
		{
			if(m==null )
				return;
			NavMeshBuildSource s = new NavMeshBuildSource();
            s.shape = NavMeshBuildSourceShape.Mesh;
			s.sourceObject = MeshUtilities.CreateCylinder(m.radius, m.radius, m.sides);
            s.transform = Matrix4x4.TRS(m.Center, m.transform.rotation, m.transform.lossyScale);
            s.area = m.area;
			sources.Add(s);	
		}

		void CollectModifierSources(ref List<NavMeshBuildSource> sources, SpatialCircleModifier m)
		{
			if(m==null )
				return;
			NavMeshBuildSource s = new NavMeshBuildSource();
            s.shape = NavMeshBuildSourceShape.Mesh;
			s.sourceObject = MeshUtilities.CreateCircle(m.radius, m.sides);
            s.transform = Matrix4x4.TRS(m.Center, m.transform.rotation, m.transform.lossyScale);
            s.area = m.area;
			sources.Add(s);	
		}
	}
}
