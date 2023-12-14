// MeshCombiner.cs Description : Combine meshes for a selected group
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;

using System.Collections.Generic;

public class  Meshcombinervtwo : MonoBehaviour {
	public bool 			SeeInspector 	= false;							// use to draw default Inspector
	public List<Material> 	list_Materials 	= new List<Material>();				// List of materials
	public List<string> 	list_Tags 		= new List<string>();				// List of tags

	public List<GameObject> list_CombineObjects 	= new List<GameObject>();
	public List<GameObject> list_CreatedObjects 	= new List<GameObject>();

	public bool				CombineDone = false;
}
#endif
