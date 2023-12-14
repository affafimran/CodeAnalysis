//Description : SetupDefaultInputsEditor.cs : Work in association with SetupDefaultInputs.cs . Use to setup default inputs keyborad and gamepad
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SetupDefaultInputs))]
public class SetupDefaultInputsEditor : Editor {
	SerializedProperty		SeeInspector;											// use to draw default Inspector
	SerializedProperty		defaultInputsValues;	

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
		defaultInputsValues 	= serializedObject.FindProperty ("defaultInputsValues");

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

		SetupDefaultInputs myScript = (SetupDefaultInputs)target; 

		GUILayout.Label("");


// --> display default the default Inputs (keyboard and gamepad)
		EditorGUILayout.BeginVertical (style_Yellow_01);
		EditorGUILayout.HelpBox("This script allow to setup the default Inputs (keyboard and gamepad).",MessageType.Info);
		EditorGUILayout.BeginHorizontal ();
		//GUILayout.Label( "Countdown text :",GUILayout.Width(100));
		EditorGUILayout.PropertyField(defaultInputsValues, new GUIContent (""));
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();

		GUILayout.Label("");

		if (myScript.defaultInputsValues != null) {
			// --> display default the default Inputs (keyboard and gamepad)

			SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.defaultInputsValues);
			serializedObject0.Update ();
			SerializedProperty m_gamepadPlayer1 = serializedObject0.FindProperty ("ListOfInputs");

			EditorGUILayout.BeginVertical (style_Blue);
// --> Gamepad Player 1 Default
			EditorGUILayout.HelpBox ("Player 1 Gamepad", MessageType.Info);

// --> Use preset for PC and Mac
			EditorGUILayout.BeginHorizontal ();
			if(GUILayout.Button("XBOX 360 PC"))
			{F_XBOX360_PC_P1();}
			if(GUILayout.Button("XBOX 360 MAC"))
			{F_XBOX360_MAC_P1();}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Left :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Right :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Acceleration :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Break :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Respawn :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Validate :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Back :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Pause :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();


// --> Keyboard Player 1 Default
			EditorGUILayout.BeginVertical (style_Blue);
			EditorGUILayout.HelpBox ("Player 1 Desktop", MessageType.Info);
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Left :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (0), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Right :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (1), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Acceleration :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (2), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Break :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (3), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Respawn :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (5), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			GUILayout.Label ("");

			EditorGUILayout.BeginVertical (style_Orange);
// --> Gamepad Player 2 Default
			EditorGUILayout.HelpBox ("Player 2 Gamepad", MessageType.Info);
// --> Use preset for PC and Mac
			EditorGUILayout.BeginHorizontal ();
			if(GUILayout.Button("XBOX 360 PC"))
			{F_XBOX360_PC_P2();}
			if(GUILayout.Button("XBOX 360 MAC"))
			{F_XBOX360_MAC_P2();}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Left :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Right :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Acceleration :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Break :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Respawn :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Validate :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Back :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Pause :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();


// --> Keyboard Player 2 Default
			EditorGUILayout.BeginVertical (style_Orange);
			EditorGUILayout.HelpBox ("Player 2 Desktop", MessageType.Info);
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Left :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (0), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Right :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (1), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Acceleration :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (2), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Break :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (3), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Respawn :", GUILayout.Width (100));
			EditorGUILayout.PropertyField (m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Desktop").GetArrayElementAtIndex (5), new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			GUILayout.Label ("");

			serializedObject0.ApplyModifiedProperties ();
		}

		GUILayout.Label("");
		serializedObject.ApplyModifiedProperties ();
	}


	void F_XBOX360_PC_P1(){
		SetupDefaultInputs myScript = (SetupDefaultInputs)target; 
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.defaultInputsValues);
		serializedObject0.Update ();
		SerializedProperty m_gamepadPlayer1 = serializedObject0.FindProperty ("ListOfInputs");
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0).stringValue = "Joystick1Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1).stringValue = "Joystick1Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2).stringValue = "Joystick1Axis10";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3).stringValue = "Joystick1Axis9";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5).stringValue = "Joystick1Button3";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6).stringValue = "Joystick1Button0";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7).stringValue = "Joystick1Button6";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8).stringValue = "Joystick1Button7";
		serializedObject0.ApplyModifiedProperties ();
	}
	void F_XBOX360_PC_P2(){
		SetupDefaultInputs myScript = (SetupDefaultInputs)target; 
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.defaultInputsValues);
		serializedObject0.Update ();
		SerializedProperty m_gamepadPlayer1 = serializedObject0.FindProperty ("ListOfInputs");
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0).stringValue = "Joystick2Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1).stringValue = "Joystick2Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2).stringValue = "Joystick2Axis10";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3).stringValue = "Joystick2Axis9";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5).stringValue = "Joystick2Button3";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6).stringValue = "Joystick2Button0";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7).stringValue = "Joystick2Button6";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8).stringValue = "Joystick2Button7";
		serializedObject0.ApplyModifiedProperties ();
	}
	void F_XBOX360_MAC_P1(){
		SetupDefaultInputs myScript = (SetupDefaultInputs)target; 
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.defaultInputsValues);
		serializedObject0.Update ();
		SerializedProperty m_gamepadPlayer1 = serializedObject0.FindProperty ("ListOfInputs");
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0).stringValue = "Joystick1Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1).stringValue = "Joystick1Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2).stringValue = "Joystick1Axis6";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3).stringValue = "Joystick1Axis5";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5).stringValue = "Joystick1Button19";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6).stringValue = "Joystick1Button16";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7).stringValue = "Joystick1Button17";
		m_gamepadPlayer1.GetArrayElementAtIndex (0).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8).stringValue = "Joystick1Button9";
		serializedObject0.ApplyModifiedProperties ();
	}
	void F_XBOX360_MAC_P2(){
		SetupDefaultInputs myScript = (SetupDefaultInputs)target; 
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.defaultInputsValues);
		serializedObject0.Update ();
		SerializedProperty m_gamepadPlayer1 = serializedObject0.FindProperty ("ListOfInputs");
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (0).stringValue = "Joystick2Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (1).stringValue = "Joystick2Axis1";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (2).stringValue = "Joystick2Axis6";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (3).stringValue = "Joystick2Axis5";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (5).stringValue = "Joystick2Button19";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (6).stringValue = "Joystick2Button16";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (7).stringValue = "Joystick2Button17";
		m_gamepadPlayer1.GetArrayElementAtIndex (1).FindPropertyRelative ("Gamepad").GetArrayElementAtIndex (8).stringValue = "Joystick2Button9";
		serializedObject0.ApplyModifiedProperties ();
	}



	void OnSceneGUI( )
	{
	}
}
#endif