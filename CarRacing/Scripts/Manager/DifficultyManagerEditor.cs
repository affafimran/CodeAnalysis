// Description : DifficultyManagerEditor.cs : Works in association with DifficultyManager.cs . Manage the difficulty parameters for each car AI
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DifficultyManager))]
public class DifficultyManagerEditor : Editor {
	SerializedProperty		SeeInspector;								// use to draw default Inspector


	SerializedProperty		difficulties;
	SerializedProperty		selectedDifficulties;


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
	private Texture2D 		Tex_02;
	private Texture2D 		Tex_03;
	private Texture2D 		Tex_04;
	private Texture2D 		Tex_05;

	void OnEnable () {
		// Setup the SerializedProperties.
		SeeInspector 		= serializedObject.FindProperty ("SeeInspector");
		difficulties 		= serializedObject.FindProperty ("difficulties");
		selectedDifficulties= serializedObject.FindProperty ("selectedDifficulties");

		Tex_01 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		Tex_02 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.5f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.4f)); 
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();
		
		serializedObject.Update ();

		GUIStyle style_Yellow_01 		= new GUIStyle(GUI.skin.box);	style_Yellow_01.normal.background 		= Tex_01; 
		GUIStyle style_Blue 			= new GUIStyle(GUI.skin.box);	style_Blue.normal.background 			= Tex_03;
		GUIStyle style_Purple 			= new GUIStyle(GUI.skin.box);	style_Purple.normal.background 			= Tex_04;
		GUIStyle style_Orange 			= new GUIStyle(GUI.skin.box);	style_Orange.normal.background 			= Tex_05; 
		GUIStyle style_Yellow_Strong 	= new GUIStyle(GUI.skin.box);	style_Yellow_Strong.normal.background 	= Tex_02;

		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Inspector :",GUILayout.Width(90));
		EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();

		DifficultyManager myScript = (DifficultyManager)target; 

		GUILayout.Label("");
		EditorGUILayout.HelpBox("This script allow to setup car AI smartness for this race." +
			"\n" +
			"1- Select the dificulty (easy,medium, expert)" +
			"\n" +
			"2- Change parameters for each car AI" +
			"\n" +
			"\n" +
			"(read full doccumentation to have more information on each parameter)",MessageType.Info);
		GUILayout.Label("");

// --> Select the difficulty mode
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical (style_Yellow_01);
			if(GUILayout.Button("Easy"))
			{
				selectedDifficulties.intValue = 0;
				PlayerPrefs.SetInt ("DifficultyChoise",0); 
			}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical (style_Blue);
			if(GUILayout.Button("Medium"))
			{
				selectedDifficulties.intValue = 1;
				PlayerPrefs.SetInt ("DifficultyChoise",1); 
			}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical (style_Orange);
			if(GUILayout.Button("Expert"))
			{
				selectedDifficulties.intValue = 2;
				PlayerPrefs.SetInt ("DifficultyChoise",2); 
			}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndHorizontal();
		GUILayout.Label("");




		GUILayout.Label("");

		for (var i = 2; i < 5; i++) {


			if (selectedDifficulties.intValue == 0) {
				EditorGUILayout.BeginVertical (style_Yellow_01);
				EditorGUILayout.BeginHorizontal (style_Yellow_01);
			}
			else if (selectedDifficulties.intValue == 1) {
				EditorGUILayout.BeginVertical (style_Blue);
				EditorGUILayout.BeginHorizontal (style_Blue);
			}
			else if (selectedDifficulties.intValue == 2) {
				EditorGUILayout.BeginVertical (style_Orange);
				EditorGUILayout.BeginHorizontal (style_Orange);
			}

			GUILayout.Label ("Car " + i, EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal ();

// --> Global speed boost
			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Global Speed Boost  :", GUILayout.Width (155));
				EditorGUILayout.Slider (difficulties.GetArrayElementAtIndex (selectedDifficulties.intValue).FindPropertyRelative ("addGlobalSpeedOffset").GetArrayElementAtIndex (i-2), -.5f, .5f, new GUIContent (""));
				GUILayout.Label (" ", GUILayout.Width (20));
			EditorGUILayout.EndHorizontal ();

// --> Follow circuit Waypoints

			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Follow circuit Waypoints :", GUILayout.Width (155));
				EditorGUILayout.IntSlider (difficulties.GetArrayElementAtIndex (selectedDifficulties.intValue).FindPropertyRelative ("waypointSuccesRate").GetArrayElementAtIndex (i-2), 0, 100, new GUIContent (""));
				GUILayout.Label ("%", GUILayout.Width (20));
			EditorGUILayout.EndHorizontal ();

// --> Optimized speed
		
			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Optimized speed :", GUILayout.Width (155));
				EditorGUILayout.IntSlider (difficulties.GetArrayElementAtIndex (selectedDifficulties.intValue).FindPropertyRelative ("speedSuccesRate").GetArrayElementAtIndex (i-2), 0, 100, new GUIContent (""));
				GUILayout.Label ("%", GUILayout.Width (20));
			EditorGUILayout.EndHorizontal ();

// --> Optimized car rotation

			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Optimized car rotation :", GUILayout.Width (155));
				EditorGUILayout.IntSlider (difficulties.GetArrayElementAtIndex (selectedDifficulties.intValue).FindPropertyRelative ("rotationSuccesRate").GetArrayElementAtIndex (i-2), 0, 100, new GUIContent (""));
				GUILayout.Label ("%", GUILayout.Width (20));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical ();

			GUILayout.Label("");
		}
		serializedObject.ApplyModifiedProperties ();
	}


	void OnSceneGUI( )
	{
	}
}
#endif