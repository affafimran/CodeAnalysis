using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// Base class for all the available SpatialModifiers that can re-shape the different navigation areas.
	/// </summary>
	public abstract class SpatialModifier : MonoBehaviour
	{
		/// <summary>
		/// Type of area to set by this SpatialModifier.
		/// </summary>
		[SpatialArea]
		public int area = 0;

		

		public virtual void OnEnable()
		{
			SpatialModifierCollector collector = GetComponentInParent<SpatialModifierCollector>();
			if( collector != null )
				collector.Register(this);
		}

		public virtual void OnDisable()
		{
			SpatialModifierCollector collector = GetComponentInParent<SpatialModifierCollector>();
			if( collector != null )
				collector.Unregister(this);
		}
	}
}
