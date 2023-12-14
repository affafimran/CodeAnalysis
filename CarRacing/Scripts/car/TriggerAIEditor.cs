// Description : TriggerAIEditor.cs : Works in association with TriggerAIEditor.cs . Allow to force AI reactions
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TriggersAI))]
public class TriggerAIEditor : Editor {
	SerializedProperty		SeeInspector;								// use to draw default Inspector


	//SerializedProperty		l_Cars;
	SerializedProperty		l_allowRandomValue;
	SerializedProperty		l_targetsValues;
	SerializedProperty		successRate_BestTargetPos;


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

	private Texture2D 		Tex_01;
	/*private Texture2D 		Tex_02;
	private Texture2D 		Tex_03;
	private Texture2D 		Tex_04;
	private Texture2D 		Tex_05;*/

	void OnEnable () {
		// Setup the SerializedProperties.
		SeeInspector 				= serializedObject.FindProperty ("SeeInspector");
		//l_Cars 						= serializedObject.FindProperty ("l_Cars");
		l_allowRandomValue			= serializedObject.FindProperty ("l_allowRandomValue");
		l_targetsValues				= serializedObject.FindProperty ("l_targetsValues");
		successRate_BestTargetPos	= serializedObject.FindProperty ("successRate_BestTargetPos");

		Tex_01 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		/*Tex_02 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.5f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.4f)); */
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();

		serializedObject.Update ();

		GUIStyle style_Yellow_01 		= new GUIStyle(GUI.skin.box);	style_Yellow_01.normal.background 		= Tex_01; 
		/*GUIStyle style_Blue 			= new GUIStyle(GUI.skin.box);	style_Blue.normal.background 			= Tex_03;
		GUIStyle style_Purple 			= new GUIStyle(GUI.skin.box);	style_Purple.normal.background 			= Tex_04;
		GUIStyle style_Orange 			= new GUIStyle(GUI.skin.box);	style_Orange.normal.background 			= Tex_05; 
		GUIStyle style_Yellow_Strong 	= new GUIStyle(GUI.skin.box);	style_Yellow_Strong.normal.background 	= Tex_02;*/

		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Inspector :",GUILayout.Width(90));
		EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();

		TriggersAI myScript = (TriggersAI)target; 

		GUILayout.Label("");
		EditorGUILayout.BeginVertical (style_Yellow_01);
		EditorGUILayout.HelpBox("This script allow to force car AI reaction." +
			"\n" +
			"1- If false : Disable car random reaction" +
			"\n" +
			"2- Choose the position for each car Target" +
			"\n" +
			"3- Success rate for each car to reach the target"
			,MessageType.Info);

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("", GUILayout.Width (155));
		GUILayout.Label ("Car 2", GUILayout.Width (50));
		GUILayout.Label ("Car 3", GUILayout.Width (50));
		GUILayout.Label ("Car 4", GUILayout.Width (50));
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("1- Allow random values  :", GUILayout.Width (155));
		for (var i = 1; i < 4; i++) {
			EditorGUILayout.PropertyField (l_allowRandomValue.GetArrayElementAtIndex(i), new GUIContent (""), GUILayout.Width (50));

		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("2- Car target Offset  :", GUILayout.Width (155));
		for (var i = 1; i < 4; i++) {
			EditorGUILayout.PropertyField (l_targetsValues.GetArrayElementAtIndex(i), new GUIContent (""), GUILayout.Width (50));

		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("3- Target success rate  :", GUILayout.Width (155));
		for (var i = 1; i < 4; i++) {
			EditorGUILayout.PropertyField (successRate_BestTargetPos.GetArrayElementAtIndex(i), new GUIContent (""), GUILayout.Width (50));

		}
		EditorGUILayout.EndHorizontal ();
		serializedObject.ApplyModifiedProperties ();
		EditorGUILayout.EndVertical ();
		GUILayout.Label("");
		GUILayout.Label("");

	}


	void OnSceneGUI( )
	{
	}
}
#endif