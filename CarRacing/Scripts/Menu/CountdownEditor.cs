//Description : CountdownEditor.cs : Work in association with Countdown.cs . Use to setup the countdown
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Countdown))]
public class CountdownEditor : Editor {
	SerializedProperty		SeeInspector;											// use to draw default Inspector

	SerializedProperty		list_Txt;
	SerializedProperty		list_Audio;
	SerializedProperty		txt_Countdown;

	private Texture2D MakeTex(int width, int height, Color col) {					// use to change the GUIStyle
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
		SeeInspector 			= serializedObject.FindProperty ("SeeInspector");

		list_Txt 				= serializedObject.FindProperty ("list_Txt");
		list_Audio				= serializedObject.FindProperty ("list_Audio");
		txt_Countdown 			= serializedObject.FindProperty ("txt_Countdown");

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

		Countdown myScript = (Countdown)target; 

		GUILayout.Label("");
		EditorGUILayout.HelpBox("This script allow to setup the countdown before the starting signal.",MessageType.Info);
		GUILayout.Label("");


// --> gameObject to display countdown on scene
		EditorGUILayout.BeginVertical (style_Yellow_01);
			EditorGUILayout.HelpBox("Next field represents the gameObject where the countdown text is displayed.",MessageType.Info);
			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label( "Countdown text :",GUILayout.Width(100));
				EditorGUILayout.PropertyField(txt_Countdown, new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();

		GUILayout.Label("");


// --> Countdown setup : Text and audio
		EditorGUILayout.BeginVertical (style_Blue);
			EditorGUILayout.HelpBox("Modify, add or delete text and sound for each countdown step.",MessageType.Info);
			EditorGUILayout.BeginHorizontal ();

				GUILayout.Label("",GUILayout.Width(30));
				GUILayout.Label("Text : ",GUILayout.Width(50));
				GUILayout.Label("Audio : ",GUILayout.Width(120));

			EditorGUILayout.EndHorizontal ();

			for (var i = 0; i < list_Txt.arraySize; i++) {
				EditorGUILayout.BeginHorizontal ();
			
					GUILayout.Label((i+1) + " :",GUILayout.Width(30));
					EditorGUILayout.PropertyField(list_Txt.GetArrayElementAtIndex(i), new GUIContent (""),GUILayout.Width(50));
					EditorGUILayout.PropertyField(list_Audio.GetArrayElementAtIndex(i), new GUIContent (""),GUILayout.Width(120));

					GUILayout.Label("Add",GUILayout.Width(30));
					if(GUILayout.Button("^",GUILayout.Width(30)))
					{
						AddBefore(i);
					}
					if(GUILayout.Button("v",GUILayout.Width(30)))
					{
						AddAfter(i);
					}
					if(GUILayout.Button("Delete",GUILayout.Width(50)))
					{
						DeleteText (i);
					}

				EditorGUILayout.EndHorizontal ();
			}

		EditorGUILayout.EndVertical ();

		GUILayout.Label("");
		serializedObject.ApplyModifiedProperties ();
	}

	void AddBefore(int value){
		list_Txt.InsertArrayElementAtIndex (value);
		list_Txt.GetArrayElementAtIndex(value).stringValue = "";
		list_Audio.InsertArrayElementAtIndex (value);
		list_Audio.GetArrayElementAtIndex (value).objectReferenceValue = null;
	}
	void AddAfter(int value){
		list_Txt.InsertArrayElementAtIndex (value+1);
		list_Txt.GetArrayElementAtIndex(value+1).stringValue = "";
		list_Audio.InsertArrayElementAtIndex (value+1);
		list_Audio.GetArrayElementAtIndex (value+1).objectReferenceValue = null;
	}
	void DeleteText(int value){
		list_Txt.DeleteArrayElementAtIndex (value);
		list_Audio.MoveArrayElement (value,list_Audio.arraySize-1);
		list_Audio.arraySize --;
	}


	void OnSceneGUI( )
	{
	}
}
#endif