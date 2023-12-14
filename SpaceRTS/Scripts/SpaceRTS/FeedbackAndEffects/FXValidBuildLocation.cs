using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	public class FXValidBuildLocation : MaterialsCollector
	{
		/// <summary>
		/// Material to use when trying to place buildable in a correct position.
		/// </summary>
		public Material validMaterial;
		/// <summary>
		/// Material to use when trying to place buildable in an invalid position.
		/// </summary>
		public Material invalidMaterial;

		public bool useValidMaterial = true;

		public void OnEnable()
		{
			Refresh();
		}

		public void OnDisable()
		{
			Refresh();
		}

		public void SetAsValid()
		{
			useValidMaterial = true;
			Refresh();
		}

		public void SetAsInvalid()
		{
			useValidMaterial = false;
			Refresh();
		}

		protected override void OnRefreshMaterials()
		{
			foreach(MeshDataInfo mdi in collectedInfo)
			{
				if(mdi.renderer == null)
					continue;

				Material [] newMaterials = new Material[mdi.renderer.sharedMaterials.Length];
				for(int i=0; i<mdi.renderer.sharedMaterials.Length; i++)
				{
					if(i<mdi.materials.Count)
					{
						if(this.enabled)
							newMaterials[i] = useValidMaterial ? validMaterial : invalidMaterial;
						else
							newMaterials[i] = mdi.materials[i];
					}
				}
				mdi.renderer.sharedMaterials = newMaterials;
			}
		}
	}
}