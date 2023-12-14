// Description : Game_ManagerEditor.cs : Works in association with Game_Manager.cs .  Manage game Rules
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Game_Manager))]
public class Game_ManagerEditor : Editor {
	SerializedProperty		SeeInspector;		// use to draw default Inspector


	//SerializedProperty		b_UseCountdown;
	public LapCounter 		lapCounter;

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
		SeeInspector 			= serializedObject.FindProperty ("SeeInspector");
		//b_UseCountdown 		= serializedObject.FindProperty ("b_UseCountdown");

		Tex_01 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		Tex_02 = MakeTex(2, 2, new Color(1,.92f,0.016F,.8f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.5f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.4f)); 
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();

		serializedObject.Update ();
		Game_Manager myScript = (Game_Manager)target; 

		GUIStyle style_Yellow_01 		= new GUIStyle(GUI.skin.box);	style_Yellow_01.normal.background 		= Tex_01; 
		GUIStyle style_Blue 			= new GUIStyle(GUI.skin.box);	style_Blue.normal.background 			= Tex_03;
		GUIStyle style_Purple 			= new GUIStyle(GUI.skin.box);	style_Purple.normal.background 			= Tex_04;
		GUIStyle style_Orange 			= new GUIStyle(GUI.skin.box);	style_Orange.normal.background 			= Tex_05; 
		GUIStyle style_Yellow_Strong 	= new GUIStyle(GUI.skin.box);	style_Yellow_Strong.normal.background 	= Tex_02;

		GUILayout.Label("");

		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Inspector :",GUILayout.Width(90));
			EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();



		GUILayout.Label("");
		EditorGUILayout.HelpBox("This script Manager Game Rules.",MessageType.Info);
		GUILayout.Label("");


		SerializedObject serializedObject2 = new UnityEditor.SerializedObject (myScript.inventoryItemCar);
		serializedObject2.Update ();
		SerializedProperty m_b_mobile = serializedObject2.FindProperty ("b_mobile");
		SerializedProperty m_mobileMaxSpeedOffset = serializedObject2.FindProperty ("mobileMaxSpeedOffset");
		SerializedProperty mobileWheelStearingOffsetReactivity = serializedObject2.FindProperty ("mobileWheelStearingOffsetReactivity");
		//SerializedProperty m_b_Countdown = serializedObject2.FindProperty ("b_Countdown");
		//SerializedProperty m_b_LapCounter = serializedObject2.FindProperty ("b_LapCounter");


// -->Mobile Options
		EditorGUILayout.BeginVertical (style_Yellow_Strong);




		EditorGUILayout.HelpBox("IMPORTANT : Modification is applied to the entire project in this section." +
			"\n" +
			"\n-You could modify the Max speed for all car on Mobile." +
			"\n" +
			"-You could modify the wheel stearing reactivity for all cars on mobile." +
			"\n",MessageType.Info);

		if (m_b_mobile.boolValue) {
			if (GUILayout.Button ("Cars are setup for Mobile Inputs")) {
				m_b_mobile.boolValue = false;
			}
		} else {
			if (GUILayout.Button ("Cars are setup for Desktop Inputs")) {
				m_b_mobile.boolValue = true;
			}
		}


		if (m_b_mobile.boolValue) {
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Mobile Max Speed Offset :", GUILayout.Width (220));
			EditorGUILayout.PropertyField (m_mobileMaxSpeedOffset, new GUIContent (""));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Mobile Wheel Stearing Offset Reactivity :", GUILayout.Width (220));
			EditorGUILayout.PropertyField (mobileWheelStearingOffsetReactivity, new GUIContent (""));
			EditorGUILayout.EndHorizontal ();
		}



		EditorGUILayout.EndVertical ();


// --> Countdown
/*		EditorGUILayout.BeginVertical (style_Orange);
		EditorGUILayout.HelpBox("IMPORTANT : Modification is applied to the entire project in this section.",MessageType.Info);
		
		if (m_b_Countdown.boolValue) {
			if (GUILayout.Button ("Countdown is activated when game Start")) {
				//b_UseCountdown.boolValue = false;
				m_b_Countdown.boolValue = false;
			}
		} else {
			if (GUILayout.Button ("Countdown is deactivated when game Start")) {
				//b_UseCountdown.boolValue = true;
				m_b_Countdown.boolValue = true;
			}
		}
		EditorGUILayout.EndVertical ();*/

// --> Lap Counter
	/*	EditorGUILayout.BeginVertical (style_Blue);
		EditorGUILayout.HelpBox("IMPORTANT : Modification is applied to the entire project in this section.",MessageType.Info);
		if (!lapCounter) {
			GameObject tmpObj = GameObject.FindGameObjectWithTag ("TriggerStart");
			if (tmpObj)
				lapCounter = tmpObj.GetComponent<LapCounter> ();
		}

		if (lapCounter) {
			//SerializedObject serializedObject1 = new UnityEditor.SerializedObject (lapCounter);
			//serializedObject1.Update ();
			//SerializedProperty m_b_ActivateLapCounter = serializedObject1.FindProperty ("b_ActivateLapCounter");
			//SerializedProperty m_lapNumber = serializedObject1.FindProperty ("lapNumber");


			if (m_b_LapCounter.boolValue) {
				if (GUILayout.Button ("Lap Counter is activated")) {
					m_b_LapCounter.boolValue = false;
				}
			} else {
				if (GUILayout.Button ("Lap Counter is deactivated")) {
					m_b_LapCounter.boolValue = true;
				}
			}

			//serializedObject1.ApplyModifiedProperties ();
		}
		EditorGUILayout.EndVertical ();*/

		serializedObject2.ApplyModifiedProperties ();

		GUILayout.Label("");
		GUILayout.Label("");
		// --> Lap Counter
		EditorGUILayout.BeginVertical (style_Yellow_01);
		EditorGUILayout.HelpBox("The next sections only affect this scene.",MessageType.Info);
		if (!lapCounter) {
			GameObject tmpObj = GameObject.FindGameObjectWithTag ("TriggerStart");
			if (tmpObj)
				lapCounter = tmpObj.GetComponent<LapCounter> ();
		}

		if (lapCounter) {
			SerializedObject serializedObject1 = new UnityEditor.SerializedObject (lapCounter);
			serializedObject1.Update ();
			//SerializedProperty m_b_ActivateLapCounter = serializedObject1.FindProperty ("b_ActivateLapCounter");
			SerializedProperty m_lapNumber = serializedObject1.FindProperty ("lapNumber");

			// --> Lap Numbers
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label( "Lap number:",GUILayout.Width(100));
			EditorGUILayout.PropertyField(m_lapNumber, new GUIContent (""));
			EditorGUILayout.EndHorizontal ();


			serializedObject1.ApplyModifiedProperties ();
		}
		EditorGUILayout.EndVertical ();

		GUILayout.Label("");

		serializedObject.ApplyModifiedProperties ();
	}

	void AddBefore(int value){

	}


	void OnSceneGUI( )
	{
	}
}
#endif