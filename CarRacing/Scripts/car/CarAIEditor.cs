// Description : Work in association with CarAI.cs : Allow to setup some parameters from CarAI.cs
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CarAI))]
public class CarAIEditor : Editor {

	SerializedProperty 		SeeInspector;
	SerializedProperty 		CarAIEulerRotation;
	SerializedProperty 		carWaitBeforeBackwardDuration;
	SerializedProperty 		carBackwardDuration;

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
		SeeInspector 					= serializedObject.FindProperty ("SeeInspector");
		CarAIEulerRotation 				= serializedObject.FindProperty ("CarAIEulerRotation");
		carWaitBeforeBackwardDuration 	= serializedObject.FindProperty ("carWaitBeforeBackwardDuration");
		carBackwardDuration 			= serializedObject.FindProperty ("carBackwardDuration");
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();
		serializedObject.Update ();


		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Inspector :",GUILayout.Width(90));
			EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();

		GUILayout.Label("");


		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Car rotation speed :",GUILayout.Width(210));
			EditorGUILayout.PropertyField(CarAIEulerRotation, new GUIContent (""),GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Car Wait Before Backward (Duration) :",GUILayout.Width(210));
			EditorGUILayout.PropertyField(carWaitBeforeBackwardDuration, new GUIContent (""),GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Car Backward Duration :",GUILayout.Width(210));
			EditorGUILayout.PropertyField(carBackwardDuration, new GUIContent (""),GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties ();
	}


	void OnSceneGUI( )
	{

	}
}
#endif