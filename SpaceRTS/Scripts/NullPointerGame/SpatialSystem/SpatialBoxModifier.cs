using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// SpatialModifier that defines a rectangle area in the spatial navigation.
	/// </summary>
	public class SpatialBoxModifier : SpatialModifier
	{
		public Vector3 center = Vector3.zero;
		public Vector3 size = Vector3.one;

		public Vector3 Center { get { return transform.TransformPoint(center); } }
		public Vector3 Size { get { return Vector3.Scale(transform.lossyScale,size); } }

		public void OnDrawGizmosSelected()
		{
			Gizmos.color =  Color.yellow;
			Gizmos.DrawWireCube(Center, Size);
		}
	}
}
