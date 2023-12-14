// Description : ChangeModelsEditor.cs : Help finding car 3D model parent on the hierarchy.  
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ChangeModels))]
public class ChangeModelsEditor : Editor {
	SerializedProperty		SeeInspector;		// use to draw default Inspector
	SerializedProperty 		carBodyParent;
	SerializedProperty 		wheel_FL_Parent;
	SerializedProperty 		wheel_FR_Parent;
	SerializedProperty 		wheel_RL_Parent;
	SerializedProperty 		wheel_RR_Parent;

	private Texture2D MakeTex(int width, int height, Color col) {		// use to change the GUIStyle
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
		// Setup the SerializedProperties.
		SeeInspector 		= serializedObject.FindProperty ("SeeInspector");
		carBodyParent 			= serializedObject.FindProperty ("carBodyParent");
		wheel_FL_Parent 			= serializedObject.FindProperty ("wheel_FL_Parent");
		wheel_FR_Parent 			= serializedObject.FindProperty ("wheel_FR_Parent");
		wheel_RL_Parent 			= serializedObject.FindProperty ("wheel_RL_Parent");
		wheel_RR_Parent 			= serializedObject.FindProperty ("wheel_RR_Parent");
	}


	public override void OnInspectorGUI()
	{

		serializedObject.Update ();

		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Inspector :",GUILayout.Width(90));
		EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();


		GUILayout.Label("");

		EditorGUILayout.BeginVertical();

			EditorGUILayout.HelpBox("Don't forget to open the padlock in Hierarchy Tab (Top right corner)"
				+ "\n" + "\n"
				+ "Click on buttons to visualize wheel or car body parent in Hierarchy."
				+ "\n"
				+ "Replace wheel or car body inside those parents."
				+ "\n"
				,MessageType.Info);
			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Show Car Body parent in the Hierarchy :",GUILayout.Width(270));
				if (GUILayout.Button ("Click",GUILayout.Width(90))) {
					Selection.activeGameObject = (GameObject)carBodyParent.objectReferenceValue;
				}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Show  Front Left Wheel parent in the Hierarchy :",GUILayout.Width(270));
				if (GUILayout.Button ("Click",GUILayout.Width(90))) {
					Selection.activeGameObject = (GameObject)wheel_FL_Parent.objectReferenceValue;
				}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Show  Front Right Wheel parent in the Hierarchy :",GUILayout.Width(270));
				if (GUILayout.Button ("Click",GUILayout.Width(90))) {
					Selection.activeGameObject = (GameObject)wheel_FR_Parent.objectReferenceValue;
				}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Show  Back Left Wheel parent in the Hierarchy :",GUILayout.Width(270));
				if (GUILayout.Button ("Click",GUILayout.Width(90))) {
					Selection.activeGameObject = (GameObject)wheel_RL_Parent.objectReferenceValue;
				}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Show Back Right Wheel parent in the Hierarchy :",GUILayout.Width(270));
				if (GUILayout.Button ("Click",GUILayout.Width(90))) {
					Selection.activeGameObject = (GameObject)wheel_RR_Parent.objectReferenceValue;
				}
			EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical ();

		GUILayout.Label("");

		serializedObject.ApplyModifiedProperties ();
	}


	void OnSceneGUI( )
	{

	}
}
#endif