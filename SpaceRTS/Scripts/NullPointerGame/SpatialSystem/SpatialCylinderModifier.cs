using NullPointerGame.Extras;
using UnityEngine;

namespace NullPointerGame.Spatial
{
	public class SpatialCylinderModifier : SpatialModifier
	{
		public Vector3 center = Vector3.zero;
		public float radius = 1.0f;
		public float height = 0.0f;
		public int sides = 18;

		public Vector3 Center { get { return transform.TransformPoint(center); } }

		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			if(sides<3) sides = 3;
			Gizmos.matrix = Matrix4x4.TRS(Center, transform.rotation, transform.lossyScale);
			Gizmos.DrawWireMesh(MeshUtilities.CreateCylinder(radius, height, sides));
			//GizmosExt.DrawWireCircle(Center-Vector3.up*height*0.5f, radius, sides);
			//GizmosExt.DrawWireCircle(Center+Vector3.up*height*0.5f, radius, sides);
		}
	}
}