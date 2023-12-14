using GameBase;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// Collects all the child SpatialModifier that matches with the current layer mask criteria.
	/// Later will be used by the current SpatialSystem implementation.
	/// </summary>
	[ExecuteInEditMode]
	public class SpatialModifierCollector : MonoBehaviour
	{
		public static readonly List<SpatialModifierCollector> collectors = new List<SpatialModifierCollector>();

		/// <summary>
		/// Recollect all the SpatialModifier from each SpatialModifierCollector in the scene.
		/// </summary>
		public static void ReCollectAll()
		{
			foreach(SpatialModifierCollector c in collectors)
				c.Collect();
		}



		/// <summary>
		/// Only take into account the modifiers that belongs to the given layer.
		/// </summary>
		public LayerMask layerMask = -1;
		/// <summary>
		/// The list of the collected SpatialModifiers
		/// </summary>
		public List<SpatialModifier> modifiers = new List<SpatialModifier>();

		public void OnEnable()
		{
			Collect();
			if (!collectors.Contains(this))
                collectors.Add(this);
		}

		public void OnDisable()
		{
			collectors.Remove(this);
		}

		public void Reset()
		{
			Revalidate();
		}

		public void OnValidate()
		{
			Revalidate();
		}

		private void Revalidate()
		{
			if (isActiveAndEnabled && !collectors.Contains(this))
			{
				collectors.Add(this);
				Collect();
			}
		}

		/// <summary>
		/// Collects all the child SpatialModifier that matches with the current mask layer criteria.
		/// </summary>
		[ContextMenu("Collect")]
		public void Collect()
		{
			modifiers.Clear();
			foreach(SpatialModifier m in GetComponentsInChildren<SpatialModifier>() )
			{
				// Discard all the SpatialModifiers that doesn't match with the allowed mask layer.
				if ((layerMask.value & (1 << m.gameObject.layer)) == 0)
                    continue;
				if( !m.isActiveAndEnabled )
					continue;
				Register(m);
			}
		}

		
		public void Register(SpatialModifier m)
		{
			if(!modifiers.Contains(m))
				modifiers.Add(m);
		}

		public void Unregister(SpatialModifier m)
		{
			if(modifiers.Contains(m))
				modifiers.Remove(m);
		}
	}
}