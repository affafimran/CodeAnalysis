using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	/// <summary>
	/// Component that will change progressively the sharedMaterials of all of its childrens to
	/// produce the effect of being built step by step according to the progress value.
	/// </summary>
	public class FXUnderConstruction : MaterialsCollector
	{
		/// <summary>
		/// Current build progress. Value range between (0, 1)
		/// </summary>
		[Space]
		[Header("Under Construction")]
		[ContextMenuItem("Test", "Refresh")]
		[Range(0f,1f)]
		public float progress = 1.0f;

		/// <summary>
		/// Material to use when unit part is not built yet.
		/// </summary>
		public Material constructMaterial;
		/// <summary>
		/// List of meshrenderers that contains the materials to change.
		/// Deprecated. Use CollectedInfo instead.
		/// </summary>
		[Tooltip("Renderers to change. Use scan in Childs context menu.")]
		[HideInInspector]
		[System.Obsolete("Use CollectedInfo instead.")]
		public List<MeshRenderer> renderers = new List<MeshRenderer>();
		/// <summary>
		/// The list of materials that will be changed
		/// Deprecated. Use CollectedInfo instead.
		/// </summary>
		[Tooltip("materials to change. Use scan in Childs context menu.")]
		[HideInInspector]
		[System.Obsolete("Use CollectedInfo instead.")]
		public List<Material> materials = new List<Material>();

		void Start()
		{
			Refresh();
		}

		private void OnValidate()
		{
			TransferOldMethodToMeshDataInfo();
		}

		/// <summary>
		/// Sets the current progress and refresh the materials to reflect that.
		/// </summary>
		/// <param name="newProgress"></param>
		public void SetProgress(float newProgress)
		{
			newProgress = Mathf.Clamp01(newProgress);
			if(newProgress != progress)
			{
				progress = newProgress;
				Refresh();
			}
		}

		protected override void OnRefreshMaterials()
		{
			// We need to see at least one piece
			int materialsToChange = Mathf.FloorToInt( progress * GetMaterialsCount() );
			foreach(MeshDataInfo mdi in collectedInfo)
			{
				if(mdi.renderer == null)
					continue;

				Material [] newMaterials = new Material[mdi.renderer.sharedMaterials.Length];
				for(int i=0; i<mdi.renderer.sharedMaterials.Length; i++)
				{
					if(i<mdi.materials.Count)
						newMaterials[i] = materialsToChange>0 ? mdi.materials[i] : constructMaterial;
					materialsToChange--;
				}
				mdi.renderer.sharedMaterials = newMaterials;
			}
		}

		#pragma warning disable 618
		private void TransferOldMethodToMeshDataInfo()
		{
			if(renderers.Count != 0 && collectedInfo.Count == 0)
			{
				for(int i=0; i<renderers.Count; i++)
				{
					if(renderers[i]==null || materials[i] == null)
						continue;

					MeshDataInfo mdi = new MeshDataInfo();
					mdi.name = renderers[i].name;
					mdi.renderer = renderers[i];
					mdi.materials.Add(materials[i]);
					collectedInfo.Add(mdi);
				}
				renderers.Clear();
				materials.Clear();
			}
		}
		#pragma warning restore 618
	}
}