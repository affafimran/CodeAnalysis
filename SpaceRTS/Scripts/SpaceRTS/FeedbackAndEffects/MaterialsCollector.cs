using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	public class MaterialsCollector : MonoBehaviour
	{
		/// <summary>
		/// Base class structure to hold the data for each MeshRendererand its materials that will be changed.
		/// </summary>
		[System.Serializable]
		public class MeshDataInfo
		{
			/// <summary>
			/// Symbolic name for the mesh. usually will be the GameObject name of the renderer.
			/// </summary>
			public string name;
			/// <summary>
			/// Mesh renderer owner of the materials that will be changed.
			/// </summary>
			public MeshRenderer renderer;
			/// <summary>
			/// The saved list of material of the MeshRenderer, needed for the complete restoration of each material.
			/// </summary>
			public List<Material> materials = new List<Material>();
		}

		[Header("MeshData In Childs")]
		public bool scanAllDownInHierarchy = true;
		[ContextMenuItem("Test", "Refresh")]
		[ContextMenuItem("Scan In Childs", "Scan")]
		public Transform scanRoot = null;
		public List<MeshDataInfo> collectedInfo = new List<MeshDataInfo>();

		public Transform TargetTransform { get { return scanRoot!=null ? scanRoot : transform; } }

		[ContextMenu ("Scan In Childs")]
		void ScanMeshDataInChilds ()
		{
			collectedInfo.Clear();
			ScanMeshDataInChilds(TargetTransform);
		}

		[ContextMenu ("Test")]
		public void Refresh()
		{
			OnRefreshMaterials();
		}

		protected virtual void OnRefreshMaterials()
		{

		}

		public void Scan()
		{
			ScanMeshDataInChilds(TargetTransform);
		}

		private void ScanMeshDataInChilds(Transform parent)
		{
			for( int i=0; i<parent.childCount; i++)
			{
				Transform tr = parent.GetChild(i);
				if(tr==null)
					continue;

				if (scanAllDownInHierarchy)
					ScanMeshDataInChilds(tr);

				MeshRenderer mr = tr.GetComponent<MeshRenderer>(); 
				if(mr==null)
					continue;

				MeshDataInfo mdi = new MeshDataInfo();
				mdi.name = tr.name;
				mdi.renderer = mr;
				mdi.materials.AddRange(mr.sharedMaterials);
				collectedInfo.Add(mdi);
			}
		}

		public int GetMaterialsCount()
		{
			int result = 0;
			foreach(MeshDataInfo mdi in collectedInfo)
			{
				if( mdi.renderer == null )
					continue;
				result += mdi.materials.Count;
			}
			return result;
		}
	}
}