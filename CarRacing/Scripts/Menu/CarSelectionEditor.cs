// Description : CarSelectionEditor.cs : Use in association with CarSelection.cs . Manage car selection for each player on Menu Page Car Selection. 
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

	[CustomEditor(typeof(CarSelection))]
	public class CarSelectionEditor : Editor {

	//public InventoryCar 	inventoryItemCar;

	SerializedProperty		SeeInspector;											// use to draw default Inspector
	SerializedProperty		ListOfCars;

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
		SeeInspector 	= serializedObject.FindProperty ("SeeInspector");

		ListOfCars 		= serializedObject.FindProperty ("ListOfCars");

		Tex_01 = MakeTex(2, 2, new Color(1,.92f,0.016F,.7f)); 
		Tex_02 = MakeTex(2, 2, new Color(1,.8f,0.2F,1f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.5f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.4f)); 

		/*if (EditorPrefs.HasKey ("inventoryItemCar_Path")) {							// Load the Data from inventoryItemCar_Path
			string objectPath = EditorPrefs.GetString ("inventoryItemCar_Path");
			inventoryItemCar = AssetDatabase.LoadAssetAtPath (objectPath, typeof(Object)) as InventoryCar;
		} else {
			string objectPath = "Assets/MCR Creator/Assets/Data/Data_CarList.asset";
			inventoryItemCar = AssetDatabase.LoadAssetAtPath (objectPath, typeof(Object)) as InventoryCar;
			if (inventoryItemCar) {
				string relPath = AssetDatabase.GetAssetPath(inventoryItemCar);
				EditorPrefs.SetString("inventoryItemCar_Path", relPath);
			}
		}*/
		CarSelection myScript = (CarSelection)target; 

	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)													// If true Default Inspector is drawn on screen
			DrawDefaultInspector();

		GUILayout.Label("");
		serializedObject.Update ();
		CarSelection myScript = (CarSelection)target; 
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Inspector :",GUILayout.Width(90));
		EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();


		if (myScript.inventoryItemCar == null) {
			Debug.Log ("MCR Creator  : You need to connect the Data_CarList on  gameObject ''CheckCarSelection'' on the Hierarchy (Hierarchy: Canvas_MainMenu -> CheckCarSelection)"); 
			EditorGUILayout.HelpBox ("MCR Creator  : You need to connect the Data_CarList on  gameObject ''CheckCarSelection'' on the Hierarchy (Hierarchy: Canvas_MainMenu -> CheckCarSelection)", MessageType.Warning);

			myScript.inventoryItemCar = EditorGUILayout.ObjectField (myScript.inventoryItemCar, typeof(Object), true) as InventoryCar;
		} else {

			EditorGUILayout.HelpBox ("This section allow to create cars lists for each player." +
			"\n" +
			"\n-Press button ''^'' to add a car BEFORE in the list." +
			"\n-Press button ''v'' to add a car AFTER in the list." +
			"\n-Press button ''-'' to delete a car in the list." +
			"\n" +
			"\nIMPORTANT : Empty slot is not allowed.", MessageType.Info);
		



			GUIStyle style_Yellow_01 = new GUIStyle (GUI.skin.box);
			style_Yellow_01.normal.background = Tex_01; 
			GUIStyle style_Blue = new GUIStyle (GUI.skin.box);
			style_Blue.normal.background = Tex_03;
			GUIStyle style_Purple = new GUIStyle (GUI.skin.box);
			style_Purple.normal.background = Tex_04;
			GUIStyle style_Orange = new GUIStyle (GUI.skin.box);
			style_Orange.normal.background = Tex_05; 
			GUIStyle style_Yellow_Strong = new GUIStyle (GUI.skin.box);
			style_Yellow_Strong.normal.background = Tex_02;


			myScript.inventoryItemCar = EditorGUILayout.ObjectField (myScript.inventoryItemCar, typeof(Object), true) as InventoryCar;

			for (var i = 0; i < myScript.inventoryItemCar.inventoryItem.Count; i++) {
			
				EditorGUILayout.BeginVertical (style_Yellow_01);

				if (i == 0)
					EditorGUILayout.LabelField ("Player 1 : Cars List", EditorStyles.boldLabel);
				if (i == 1)
					EditorGUILayout.LabelField ("Player/CPU 2  : Cars List", EditorStyles.boldLabel);
				if (i == 2)
					EditorGUILayout.LabelField ("CPU 3  : Cars List", EditorStyles.boldLabel);
				if (i == 3)
					EditorGUILayout.LabelField ("CPU 4 : Cars List", EditorStyles.boldLabel);
				//EditorGUILayout.BeginVertical ();
				SerializedObject serializedObject0 = new UnityEditor.SerializedObject (myScript.inventoryItemCar);

				for (var j = 0; j < serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (i).FindPropertyRelative ("Cars").arraySize; j++) {
					serializedObject0.Update ();

					EditorGUILayout.BeginHorizontal ();
					SerializedProperty m_car = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (i).FindPropertyRelative ("Cars").GetArrayElementAtIndex (j);
					EditorGUILayout.PropertyField (m_car, new GUIContent (""));
					if (GUILayout.Button ("^", GUILayout.Width (30))) {
						//AddCarBefore (i,j);
						Undo.RegisterFullObjectHierarchyUndo (myScript.inventoryItemCar, "AddCarBefore" + myScript.inventoryItemCar.name);
						myScript.inventoryItemCar.inventoryItem [i].Cars.Insert (j, null);
						break;
					}
					if (GUILayout.Button ("v", GUILayout.Width (30))) {
						//AddCarAfter (i,j);
						Undo.RegisterFullObjectHierarchyUndo (myScript.inventoryItemCar, "AddCarAfter" + myScript.inventoryItemCar.name);
						myScript.inventoryItemCar.inventoryItem [i].Cars.Insert (j + 1, null);
						break;
					}
					if (serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (i).FindPropertyRelative ("Cars").arraySize > 1) {
						if (GUILayout.Button ("-", GUILayout.Width (30))) {
							//DeleteCar (i, j);
							Undo.RegisterFullObjectHierarchyUndo (myScript.inventoryItemCar, "DeleteCar" + myScript.inventoryItemCar.name);
							myScript.inventoryItemCar.inventoryItem [i].Cars.RemoveAt (j);
							break;
						}
					}
					EditorGUILayout.EndHorizontal ();


					serializedObject0.ApplyModifiedProperties ();
				}
				GUILayout.Label ("");
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Cars spawn using this position :", GUILayout.Width (180));
				EditorGUILayout.PropertyField (ListOfCars.GetArrayElementAtIndex (i).FindPropertyRelative ("pivotToSpawnCar"), new GUIContent (""));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
			}
			
		}
		serializedObject.ApplyModifiedProperties ();
	}

	/*void AddCarBefore (int PlayerNumber,int carPosition){
		Undo.RegisterFullObjectHierarchyUndo (inventoryItemCar, "AddCarBefore" + inventoryItemCar.name);
		inventoryItemCar.inventoryItem [PlayerNumber].Cars.Insert (carPosition, null);
	}
	void AddCarAfter (int PlayerNumber,int carPosition){
		Undo.RegisterFullObjectHierarchyUndo (inventoryItemCar, "AddCarAfter" + inventoryItemCar.name);
		inventoryItemCar.inventoryItem [PlayerNumber].Cars.Insert (carPosition+1, null);
	}
	void DeleteCar (int PlayerNumber,int carPosition){
		Undo.RegisterFullObjectHierarchyUndo (inventoryItemCar, "DeleteCar" + inventoryItemCar.name);
		inventoryItemCar.inventoryItem [PlayerNumber].Cars.RemoveAt (carPosition);
	}*/


	void OnSceneGUI( )
	{
	}
}
#endif