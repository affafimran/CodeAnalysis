//Description : gizmosCube.cs : Use to display a Cube on gameObject 
using UnityEngine;
using System.Collections;

public class gizmosCube : MonoBehaviour {

	public Color GizmoColor = new Color(0,.9f,1f,.5f);		

	void OnDrawGizmos()
	{
		Gizmos.color = GizmoColor;																						

		Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);		// Allow the gizmo to fit the position, rotation and scale of the gameObject
		Gizmos.matrix = cubeTransform;

		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}
