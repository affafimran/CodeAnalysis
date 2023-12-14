using NullPointerCore.Extras;
using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// SpatialModifier that defines a circular area in the spatial navigation.
	/// </summary>
	public class SpatialCircleModifier : SpatialModifier
	{
		public Vector3 center = Vector3.zero;
		public float radius = 1.0f;
		public int sides = 18;

		public Vector3 Center { get { return transform.TransformPoint(center); } }

		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			GizmosExt.DrawWireCircle(Center, radius, sides);
		}
	}
}