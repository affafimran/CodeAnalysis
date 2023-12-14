//Description : PathCircuitEditor.cs : Work in association with PathCircuit.cs . Allow to create a car path
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(PathCircuit))]
public class PathCircuitEditor : Editor
{
	public bool 										SeeInspector = false;	// use to draw default Inspector

	public UnityStandardAssets.Utility.WaypointCircuit 	waypoint;				// Access component


	private Texture2D MakeTex(int width, int height, Color col) {				// use to change the GUIStyle
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}
		
	void OnEnable () {
		PathCircuit myScript = (PathCircuit)target; 
		waypoint = myScript.gameObject.GetComponent<UnityStandardAssets.Utility.WaypointCircuit>();
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		PathCircuit myScript = (PathCircuit)target; 

		serializedObject.Update ();
		GUIStyle style = new GUIStyle(GUI.skin.box);

		GUILayout.Label("");

		style.normal.background = MakeTex(2, 2, new Color(1,.2f,0,.3f));


		EditorGUILayout.HelpBox("How to create a new path :" +
			"\n1 - Select this gameobject on the Hierarchy (" + myScript.name + ")." +
			"\n2 - Select the ''Scene Tab'' (click on the icon named Scene)" +
			"\n3 - Press keyboard ''F'' button to activate the focus mode" +
			"\n4 - Put your mouse where you want to create a new checkpoint." +
			"\n5 - Press keyboard ''J'' button to create a new checkpoint" +
			"\nCheckpoint is created only if there a gameObject under the mouse position",MessageType.Info);

	

		EditorGUILayout.HelpBox(
			"\n7 - Press button ''Create'' when all the checkpoints are created"
			,MessageType.Info);


// --> Create the path
		if (GUILayout.Button("Create Path"))																			// --> Create path using checkpoints in Track_Path gameObject
		{
			var track = waypoint.waypointList.circuit;																		// Access Waypoint list
			var checkPoints = new Transform[track.transform.childCount];													// Know the number of checkpoints 
			int n = 0;
			foreach (Transform checkpoint in track.transform)
			{
				checkPoints[n++] = checkpoint;
			}
			Array.Sort(checkPoints, new TransformNameComparer());
			track.waypointList.items = new Transform[checkPoints.Length];
			for (n = 0; n < checkPoints.Length; ++n)
			{
				track.waypointList.items[n] = checkPoints[n];
			}
				
			for(int i = 0;i < waypoint.waypointList.items.Length-1;i++){													//checkpoints look forward
				waypoint.waypointList.items[i].LookAt(waypoint.waypointList.items[i+1]); 
			}

			waypoint.waypointList.items[waypoint.waypointList.items.Length-1].LookAt(waypoint.waypointList.items[0]);

			for(int i = 0;i < waypoint.waypointList.items.Length;i++){
				waypoint.waypointList.items[i].localEulerAngles = new Vector3(0,
					waypoint.waypointList.items[i].localEulerAngles.y,
					waypoint.waypointList.items[i].localEulerAngles.z); 
			}


			GameObject tmpObj = GameObject.Find ("Grp_StartLine");														// --> Update StartLine group position
			if (tmpObj) {
				Undo.RegisterFullObjectHierarchyUndo (tmpObj, tmpObj.name);
				SerializedObject serializedObject0 = new UnityEditor.SerializedObject (tmpObj.GetComponent<Transform> ());
				serializedObject0.Update ();
				SerializedProperty m_LocalPos = serializedObject0.FindProperty ("m_LocalPosition");

				m_LocalPos.vector3Value = new Vector3 (
					waypoint.waypointList.items[0].transform.position.x,
					waypoint.waypointList.items[0].transform.position.y + .5f,
					waypoint.waypointList.items[0].transform.position.z);


				serializedObject0.ApplyModifiedProperties ();

				tmpObj.transform.eulerAngles = waypoint.waypointList.items [0].transform.eulerAngles;
			}
		}
			
		GUILayout.Label("");

		EditorGUILayout.HelpBox(
			"Checkpoints are used to respawn cars. So : " +
			"\n-Add your checkpoints on road." +
			"\n-Not to close a jump" +
			"\n-Not on a cliff",MessageType.Info);


		GUILayout.Label("");

		EditorGUILayout.HelpBox(
			"You need to ''open'' the script ''Waypoint Circuit'' on the Inspector to see the path. Click on gray rectangle beside the logo c#",MessageType.Warning);


		GUILayout.Label("");

// --> Delete the path
		if (GUILayout.Button("Delete Path"))
		{
			Undo.RegisterFullObjectHierarchyUndo (myScript, "Reset_" + myScript.name);

			int children = myScript.transform.childCount;

			for (int i = children-1; i >= 0 ; i--) {
				Undo.DestroyObjectImmediate (myScript.transform.GetChild (i).gameObject);
			}
				
			waypoint.waypointList.items = new Transform[0];
		}


		GUILayout.Label("");

		serializedObject.ApplyModifiedProperties ();


	}

	void OnSceneGUI()
	{
		PathCircuit myScript = (PathCircuit)target; 
		CreateNewCheckpoint(myScript);											// create a new CheckPoint 
	}

// --> Create a new CheckPoint when button J is pressed
	void CreateNewCheckpoint(PathCircuit myScript){
		if (Event.current.type == EventType.KeyUp && Event.current.isKey && Event.current.keyCode == KeyCode.J){
			Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit)){
				GameObject instance = new GameObject();
				instance.transform.position = hit.point;

				instance.AddComponent<gizmosName>();

				int children = myScript.transform.childCount;

				if(children < 10)instance.name = "0" + children.ToString();
				else instance.name = children.ToString();

				instance.transform.SetParent(myScript.transform);
				instance.gameObject.tag = "Checkpoint";

				Undo.RegisterCreatedObjectUndo(instance,"instance");
			}
		}
	}
		

	// comparer for check distances in ray cast hits
	public class TransformNameComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			return ((Transform) x).name.CompareTo(((Transform) y).name);
		}
	}

}
#endif