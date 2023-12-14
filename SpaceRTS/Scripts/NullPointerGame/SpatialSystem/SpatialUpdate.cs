using GameBase;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// Collects all the SpatialModifiers that it can found in the VisualProxy and registers
	/// it at the SpatialSystem to re-shape the Navigation areas of the map.
	/// </summary>
	public class SpatialUpdate : GameEntityComponent
	{
		private List<SpatialModifier> modifiers = new List<SpatialModifier>();
		private SpatialSystem spatial = null;

		/// <summary>
		/// An enumeration of all the already collected modifiers that it can found in the VisualProxy.
		/// </summary>
		public IEnumerable<SpatialModifier> Modifiers { get { return modifiers; } }

		/// <summary>
		/// Cache for the SpatialSystem
		/// </summary>
		public SpatialSystem Spatial {
			get
			{
				if(spatial==null)
					spatial = GetSceneSystem<SpatialSystem>();
				return spatial;
			}
		}

		/// <summary>
		/// Collects and registers all the SpatialModifiers that can find in the VisualProxy.
		/// </summary>
		public void OnEnable()
		{
			CollectModifiers();
			if (Spatial != null)
				Spatial.AddVolumeModifiers(modifiers);
		}

		/// <summary>
		/// Unregisters all the cached SpatialModifiers from the SpatialSystem
		/// </summary>
		public void OnDisable()
		{
			if(Spatial!=null)
				Spatial.RemoveVolumeModifiers(modifiers);
		}

		/// <summary>
		/// Collects all the modifiers in the VisualProxy and registers it at the SpatialSystem
		/// </summary>
		public override void OnVisualModuleSetted()
		{
			CollectModifiers();
			if(Spatial!=null)
				Spatial.AddVolumeModifiers(modifiers);
			base.OnVisualModuleSetted();
		}

		/// <summary>
		/// Unregister all the cached modifiers from the spatial system
		/// </summary>
		public override void OnVisualModuleRemoved()
		{
			base.OnVisualModuleRemoved();
			if(Spatial!=null)
				Spatial.RemoveVolumeModifiers(modifiers);
			modifiers.Clear();
		}

		/// <summary>
		/// Collects all the SpatialModifiers that can found in the VisualProxy.
		/// </summary>
		public void CollectModifiers()
		{
			if( VisualProxy != null )
				VisualProxy.GetComponentsInChildren<SpatialModifier>(false, modifiers);
		}
	}
}