// Description : LapCounterEditor.cs : Works in association with LapCounter.cs . Access parameters on Inspector
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LapCounter))]
public class LapCounterEditor : Editor {
	SerializedProperty		SeeInspector;								// use to draw default Inspector

	//SerializedProperty		b_ActivateLapCounter;
	SerializedProperty		lapNumber;
	SerializedProperty		inventoryItemCar;


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

		//b_ActivateLapCounter 	= serializedObject.FindProperty ("b_ActivateLapCounter");
		lapNumber				= serializedObject.FindProperty ("lapNumber");
		inventoryItemCar		= serializedObject.FindProperty ("inventoryItemCar");

		Tex_01 = MakeTex(2, 2, new Color(1,.8f,0.2F,.4f)); 
		Tex_02 = MakeTex(2, 2, new Color(1,.8f,0.2F,1f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.5f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.4f)); 
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)																// If true Default Inspector is drawn on screen
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

		LapCounter myScript = (LapCounter)target; 

		GUILayout.Label("");

		if(inventoryItemCar.objectReferenceValue == null)
			EditorGUILayout.HelpBox("You need to connect ''Data_CarList'' on the next SLot" +
				"\nMCR Creator -> Assets -> Data -> Data_CarList.",MessageType.Warning);
		EditorGUILayout.PropertyField(inventoryItemCar, new GUIContent (""));

		GUILayout.Label("");
// --> Number of Lap
		EditorGUILayout.BeginVertical (style_Yellow_Strong);
			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Number of Lap :",GUILayout.Width(90));
				EditorGUILayout.PropertyField(lapNumber, new GUIContent (""),GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical ();

		GUILayout.Label("");



		serializedObject.ApplyModifiedProperties ();
	}


	void InitCarPositionOnTheStartLine(){
// --> Find and Pause Cars
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							

		foreach (GameObject car in arrCars) {
			if (car.GetComponent<CarController> ()) {

				int tmpCarNumber = car.GetComponent<CarController> ().playerNumber;

				GameObject tmpPosition = GameObject.Find ("Start_Position_0" + tmpCarNumber);

				if (tmpPosition) {
					Undo.RegisterFullObjectHierarchyUndo (car.gameObject, car.gameObject.name);

					car.transform.position = new Vector3(tmpPosition.transform.position.x,tmpPosition.transform.position.y+.15f,tmpPosition.transform.position.z);	// Init poisiton
					car.transform.eulerAngles = tmpPosition.transform.eulerAngles;																					// Init rotation
				}

			}
		}
	}


	void OnSceneGUI( )
	{
	}
}
#endif