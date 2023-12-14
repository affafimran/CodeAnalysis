using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NullPointerGame.Extras
{
	/// <summary>
	/// Helper class to modify a nav mesh surface when an entity is instanced or destroyed.
	/// </summary>
	public class NavMeshSurfaceRefresh : MonoBehaviour
	{
		private List<NavMeshSurface> parentSurfaces = new List<NavMeshSurface>();

		static List<NavMeshSurface> surfacesToRefresh = new List<NavMeshSurface>();
		static List<NavMeshLink> linksToRefresh = new List<NavMeshLink>();
		static bool refreshingInFrame = false;

		public void OnEnable()
		{
			CollectParentSurfaces();
			RequestRefresh();
		}

		public void OnDisable()
		{
			RequestRefresh();
		}

		public void OnTransformParentChanged()
		{
			RequestRefresh();
			CollectParentSurfaces();
			RequestRefresh();
		}

		private void CollectParentSurfaces()
		{
			parentSurfaces.Clear();
			parentSurfaces.AddRange(GetComponentsInParent<NavMeshSurface>());
		}

		[ContextMenu("RefreshSurfacesAndLinks")]
		private void RequestRefresh()
		{
			if(!this.gameObject.activeInHierarchy)
				return;

			foreach(NavMeshSurface surf in parentSurfaces)
			{
				if(!NavMeshSurfaceRefresh.surfacesToRefresh.Contains(surf))
					NavMeshSurfaceRefresh.surfacesToRefresh.Add(surf);
			}
			foreach(NavMeshLink link in GetComponentsInChildren<NavMeshLink>())
			{
				if(!linksToRefresh.Contains(link))
					linksToRefresh.Add(link);
			}
			bool hasSurfacesToRefresh = NavMeshSurfaceRefresh.surfacesToRefresh.Count > 0;
			bool hasLinksToRefresh = NavMeshSurfaceRefresh.linksToRefresh.Count > 0;

			if(!refreshingInFrame && hasSurfacesToRefresh || hasLinksToRefresh)
				NavMeshSurfaceRefresh.surfacesToRefresh[0].StartCoroutine(Refresh());
		}

		IEnumerator Refresh()
		{
			refreshingInFrame = true;
			yield return new WaitForEndOfFrame();

			refreshingInFrame = false;
			DoRefresh();
		}

		void DoRefresh()
		{
			//foreach(NavMeshSurface surf in NavMeshSurfaceRefresh.surfacesToRefresh)
			//	surf.UpdateNavMesh(surf.navMeshData);
			//NavMeshSurfaceRefresh.surfacesToRefresh.Clear();

			//foreach (NavMeshLink link in NavMeshSurfaceRefresh.linksToRefresh)
			//	link.UpdateLink();
			//NavMeshSurfaceRefresh.linksToRefresh.Clear();
		}
	}
}
