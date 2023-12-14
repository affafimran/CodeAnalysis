//MeshCombinerEditor : use with MeshComnier.cs
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(Meshcombinervtwo))]
public class MeshcombinervtwoEditor : Editor {
	SerializedProperty		SeeInspector;
	SerializedProperty		list_Tags;
	SerializedProperty		CombineDone;

	private bool 			b_Combine = false;
	private bool 			b_Uncombine = false;
	private bool			b_AddTag = false;
	private bool			b_DeleteTag = false;
	private int				deleteNum = 0;

	//Quaternion oldRot = Quaternion.identity;						// Save the original position and rotation of obj
	//Vector3 oldPos =  new Vector3(0,0,0);

	private Texture2D MakeTex(int width, int height, Color col) {
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	// Use this for initialization
	void OnEnable () {
		SeeInspector 	= serializedObject.FindProperty ("SeeInspector");
		list_Tags		= serializedObject.FindProperty ("list_Tags");
		CombineDone		= serializedObject.FindProperty ("CombineDone");
	}

	public override void OnInspectorGUI(){
		Meshcombinervtwo myScript = (Meshcombinervtwo)target;
		GUIStyle style = new GUIStyle(GUI.skin.box);

		style.normal.background = MakeTex(2, 2, new Color(1,1,0,.6f));

		serializedObject.Update ();

		EditorGUILayout.LabelField("");

		if(SeeInspector.boolValue)
			DrawDefaultInspector();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("See Variables : ",GUILayout.Width(90));
			EditorGUILayout.PropertyField (SeeInspector, new GUIContent (""));
		EditorGUILayout.EndHorizontal();	

		EditorGUILayout.LabelField("");

		EditorGUILayout.HelpBox("\n" +"Combine Meshes : " +
			"\n" +
			"\n1 - GameObject inside this gameObject are combine." +
			"\n" +
			"\nAll the gameObjects with the same material are combine in a single mesh." +
			"\n" +
			"\nCombining Process could take time if there are a lots of gameObjects to combine." +
			"\n" +
			"\n2 - Press button Combine to start the process" +
			"\n",MessageType.Info);


		EditorGUILayout.BeginVertical(style);

		EditorGUILayout.LabelField("");
		if(!CombineDone.boolValue){
			if(GUILayout.Button("Combine"))
			{
				b_Combine = true;
			}
		}
		else{
			if(GUILayout.Button("Reset")){
				b_Uncombine = true;
			}
		}
		EditorGUILayout.LabelField("");
		EditorGUILayout.EndVertical();
		//

		EditorGUILayout.HelpBox("3 - After the combining process gameObjects are created for each material inside this folder.",MessageType.Info);
		EditorGUILayout.HelpBox("INFO 1 : Mesh renderer are disabled for the gameObjects that have been used in the combining process." +
		"\nINFO 2 : After combinig process, colliders stay activated.",MessageType.Info);


		EditorGUILayout.LabelField("");
		EditorGUILayout.LabelField("");
		style.normal.background = MakeTex(2, 2, new Color(.5f,.8f,0,.3f));
		EditorGUILayout.BeginVertical(style);
		EditorGUILayout.HelpBox("(Optional) : Exclude gameObjects with specific TAG",MessageType.Info);
			EditorGUILayout.LabelField("");
			
			if(GUILayout.Button("Add new tag"))
			{
				b_AddTag = true;
			}
			EditorGUILayout.LabelField("");

			EditorGUILayout.LabelField("Exclude gameObjects with these Tags :");

			for(int i = 0; i < myScript.list_Tags.Count; i++){
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PropertyField (list_Tags.GetArrayElementAtIndex(i), new GUIContent (""));
				if(GUILayout.Button("-",GUILayout.Width(20)))
				{
					b_DeleteTag = true;
					deleteNum  = i;
					break;
				}
				EditorGUILayout.EndHorizontal();
			}
		EditorGUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties ();

		if(b_Combine){
			Undo.RegisterCompleteObjectUndo(myScript,"MeshCombiner" + myScript.gameObject.name);
			myScript.CombineDone = true;
			Component[] ChildrenMesh = myScript.GetComponentsInChildren(typeof(MeshRenderer), true);

			myScript.list_Materials.Clear();

			foreach (MeshRenderer child in ChildrenMesh){				// Find all the different materials
				myScript.list_Materials.Add(child.sharedMaterial);
			}

			for(int i = 0;i<myScript.list_Materials.Count;i++){				// remove materials using multiple time
				for(int k = 0;k<myScript.list_Materials.Count;k++){
					if(k != i && myScript.list_Materials[i] == myScript.list_Materials[k]){
						myScript.list_Materials[k] = null;
					}
				}
			}

			List<Material> 	Tmp_list_Materials = new List<Material>();				// List of materials

			for(int i = 0;i<myScript.list_Materials.Count;i++){				// Update Materials List
				if(myScript.list_Materials[i]){
					Tmp_list_Materials.Add(myScript.list_Materials[i]);
				}
			}

			myScript.list_Materials.Clear();

			for(int i = 0;i<Tmp_list_Materials.Count;i++){				// Update Materials List
				myScript.list_Materials.Add(Tmp_list_Materials[i]);
			}

			myScript.list_CreatedObjects.Clear();
			myScript.list_CombineObjects.Clear();

			Quaternion oldRot =  myScript.transform.rotation;						// Save the original position and rotation of obj
			Vector3 oldPos =  myScript.transform.position;

			myScript.transform.rotation = Quaternion.identity;
			myScript.transform.position = new Vector3(0,0,0);

			for(int i = 0;i<myScript.list_Materials.Count;i++){
				CombineMeshes(myScript.list_Materials[i]);
			}

			myScript.transform.rotation = oldRot;
			myScript.transform.position = oldPos;

			b_Combine = false;

		}

		if(b_Uncombine){
			Undo.RegisterCompleteObjectUndo(myScript,"MeshCombiner" + myScript.gameObject.name);
			myScript.CombineDone = false;

			for(int i = 0; i < myScript.list_CombineObjects.Count; i++){	
				if(myScript.list_CombineObjects[i] != null){
					SerializedObject serializedObject3 = new UnityEditor.SerializedObject(myScript.list_CombineObjects[i].gameObject.GetComponents<Renderer>());
					serializedObject3.Update ();
					SerializedProperty tmpSer2 = serializedObject3.FindProperty("m_Enabled");
					tmpSer2.boolValue  = true;
					serializedObject3.ApplyModifiedProperties ();
				}
			}

			for(int i = 0; i < myScript.list_CreatedObjects.Count; i++){	
				if(myScript.list_CreatedObjects[i] != null){
					Undo.DestroyObjectImmediate(myScript.list_CreatedObjects[i]);
				}
			}

			myScript.list_CreatedObjects.Clear();
			myScript.list_CombineObjects.Clear();
			b_Uncombine = false;
		}

		if(b_AddTag){
			Undo.RegisterCompleteObjectUndo(myScript,"Save" + myScript.gameObject.name);
			myScript.list_Tags.Add("Your Tag");
			b_AddTag = false;
		}

		if(b_DeleteTag){
			Undo.RegisterCompleteObjectUndo(myScript,"Delete" + myScript.gameObject.name);
			myScript.list_Tags.RemoveAt(deleteNum);
			b_DeleteTag = false;
		}
	}

	public void CombineMeshes (Material mat) {											// -> Combine all the maesh with a specif material.
		
		Meshcombinervtwo myScript = (Meshcombinervtwo)target;

		GameObject newGameObject = new GameObject();

		newGameObject.AddComponent<MeshFilter>();
		newGameObject.AddComponent<MeshRenderer>();

		newGameObject.GetComponent<Renderer>().sharedMaterial = null;

		newGameObject.name = "Combine_" + mat.name;
		Undo.RegisterCreatedObjectUndo(newGameObject,"CombineMat" + mat.name);

		myScript.list_CreatedObjects.Add(newGameObject);

		bool OneMesh = false;												// This variable is used to know if there is at least one mesh to combine

		newGameObject.transform.rotation = Quaternion.identity;						// Init position to zero

		newGameObject.transform.SetParent(myScript.transform);
		newGameObject.transform.localPosition = new Vector3(0,0,0);								// Init position to Vector3(0,0,0)
		newGameObject.isStatic = true;

		MeshFilter[] filters = myScript.gameObject.GetComponentsInChildren<MeshFilter>();	// Find all the children with MeshFilter component

		Mesh finalMesh = new Mesh();										// Create the new mesh

		CombineInstance[] combiners = new CombineInstance[filters.Length];	// Struct used to describe meshes to be combined using Mesh.CombineMeshes.


		for(int i = 0; i < filters.Length; i++){							// Check all the children
			if(filters[i].transform ==  myScript.gameObject.transform)						// Do not select the parent himself
				continue;
			if(filters[i].gameObject.GetComponent<Renderer>() ==  null)		// Check if there is Renderer component
				continue;
			bool checkTag = false;								
			for(int j = 0; j < myScript.list_Tags.Count; j++){						// Check tag to know if you need to ignore this gameobject 
				if(filters[i].gameObject.tag ==  myScript.list_Tags[j]){
					checkTag = true;
				}
			}

			if(mat == filters[i].gameObject.GetComponent<Renderer>().sharedMaterial && !checkTag	// Add this gameObject to the combiner
				&& filters[i].gameObject.GetComponent<Renderer>().enabled){
				combiners[i].subMeshIndex = 0;
				combiners[i].mesh = filters[i].sharedMesh;

				combiners[i].transform = filters[i].transform.localToWorldMatrix;

				myScript.list_CombineObjects.Add(filters[i].gameObject);

				SerializedObject serializedObject3 = new UnityEditor.SerializedObject(filters[i].gameObject.GetComponents<Renderer>());
				serializedObject3.Update ();
				SerializedProperty tmpSer2 = serializedObject3.FindProperty("m_Enabled");
				tmpSer2.boolValue  = false;
				serializedObject3.ApplyModifiedProperties ();

				OneMesh = true;
			}

		}

		finalMesh.CombineMeshes(combiners);						// Combine the new mesh
		newGameObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;		// Create the new Mesh Filter
		newGameObject.GetComponent<Renderer>().material = mat;				// ADd the good material

		if(!OneMesh){											// If there is nothing to combine delete the object
			if(myScript.gameObject.GetComponent<MeshFilter>())myScript.gameObject.GetComponent<MeshFilter>().sharedMesh = null;
		}
		else{
			UnwrapParam param = new UnwrapParam();				// enable lightmap
			UnwrapParam.SetDefaults( out param );
			Unwrapping.GenerateSecondaryUVSet( finalMesh, param );
		}
	}

}
#endif