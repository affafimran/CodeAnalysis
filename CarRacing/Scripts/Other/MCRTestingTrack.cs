// Description : MCRTestingTrack.cs : This script is used to create a menu that allow to setup the global preferences of MCR Creator
#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class MCRTestingTrack : EditorWindow
{
	public InventoryGlobalPrefs 		inventoryItemList;
	public List<GameObject> 			list_Cars	= new List<GameObject> ();
	public LapCounter 					lapCounter;
	public Game_Manager					game_Manager;
	public DifficultyManager		 	game_ManagerDifficulty;
	public GameObject 					objTextTestMode;

	private Vector2 					scrollPosition = Vector2.zero;


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

	// Add menu item named "Test Mode Panel" to the Window menu
	[MenuItem("Tools/MCR/Test Mode Panel")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(MCRTestingTrack));
	}

	void OnEnable () {
		for (var i = 0; i < 4; i++) {
			list_Cars.Add (null);
		}

// --> Load Data
		if (EditorPrefs.HasKey ("MCRGlobalPref_Path_")) {																
			string objectPath = EditorPrefs.GetString ("MCRGlobalPref_Path_");
			inventoryItemList = AssetDatabase.LoadAssetAtPath (objectPath, typeof(Object)) as InventoryGlobalPrefs;
		} else {
			string objectPath = "Assets/MCR Creator/Assets/Datas/Data_MCRTestingTrack.asset";
			inventoryItemList = AssetDatabase.LoadAssetAtPath (objectPath, typeof(Object)) as InventoryGlobalPrefs;
			if (inventoryItemList) {
				string relPath = AssetDatabase.GetAssetPath(inventoryItemList);
				EditorPrefs.SetString("MCRGlobalPref_Path_", relPath);
			}
		}


		/*if (!PlayerPrefs.HasKey ("TestMode")) {															// First Time the window is opened. Activate Test Mode
			PlayerPrefs.SetInt ("TestMode", 1); 														// Test Mode is activated
			SerializedObject serializedObject0 = new UnityEditor.SerializedObject (inventoryItemList);
			serializedObject0.Update ();
			SerializedProperty m_b_TestMode = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("b_TestMode");
			if (inventoryItemList) {
				m_b_TestMode.boolValue = true;
			}
			serializedObject0.ApplyModifiedProperties ();
		}*/
			
	}

	void OnGUI()
	{
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, true, true, GUILayout.Width (position.width), GUILayout.Height (position.height));  

		if (Tex_01 == null) {
			Tex_01 = MakeTex (2, 2, new Color (1, .8f, 0.2F, .4f)); 
			Tex_02 = MakeTex (2, 2, new Color (1, .8f, 0.2F, 1f)); 
			Tex_03 = MakeTex (2, 2, new Color (.3F, .9f, 1, .5f));
			Tex_04 = MakeTex (2, 2, new Color (1, .3f, 1, .3f)); 
			Tex_05 = MakeTex (2, 2, new Color (1, .5f, 0.3F, .4f)); 
		}


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

		EditorGUI.BeginChangeCheck ();


		EditorGUILayout.LabelField ("");
		if (inventoryItemList == null) {
			EditorGUILayout.HelpBox ("Drag and drop the file ''Data_MCRTestingTrack'' on the next field" +
				"\n(MCR Creator -> Assets -> Datas -> Data_MCRTestingTrack)", MessageType.Warning);
		}
// --> Display Data
		inventoryItemList = EditorGUILayout.ObjectField (inventoryItemList, typeof(Object), true) as InventoryGlobalPrefs;


		if (EditorGUI.EndChangeCheck ()) {
			if (inventoryItemList) {
				string relPath = AssetDatabase.GetAssetPath (inventoryItemList);
				EditorPrefs.SetString ("MCRGlobalPref_Path_", relPath);
			}
		}


		EditorGUILayout.HelpBox ("IMPORTANT : If ''Test Mode'' is activated all the Unity project is affected by the Test Mode", MessageType.Info);

		if (inventoryItemList) {
			EditorGUILayout.BeginVertical (style_Yellow_Strong);

			SerializedObject serializedObject0 = new UnityEditor.SerializedObject (inventoryItemList);
			serializedObject0.Update ();
			SerializedProperty m_b_TestMode = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("b_TestMode");

			EditorGUILayout.LabelField ("");

// --> Test mode is activated or deactivated
			if (!m_b_TestMode.boolValue) {										
				if (GUILayout.Button ("Test Mode is deactivated")) {
					//PlayerPrefs.SetInt ("TestMode", 1); 																	// --> activated

					if (inventoryItemList) {
						m_b_TestMode.boolValue = true;
					}
				}
			} else {
				if (GUILayout.Button ("Test Mode is Activated")) {
					//PlayerPrefs.SetInt ("TestMode", 0); 																	// --> deactivated

					if (inventoryItemList) {
						m_b_TestMode.boolValue = false;
					}

					if (lapCounter && game_Manager) {																				// Activate Lap Counter
						SerializedObject serializedObject1 = new UnityEditor.SerializedObject (game_Manager.inventoryItemCar);
						serializedObject1.Update ();
						SerializedProperty m_b_LapCounter = serializedObject1.FindProperty("b_LapCounter");
						m_b_LapCounter.boolValue = true;
						serializedObject1.ApplyModifiedProperties ();
					}


					if (game_Manager) {																								// Activate Countdown
						SerializedObject serializedObject2 = new UnityEditor.SerializedObject (game_Manager.inventoryItemCar);
						serializedObject2.Update ();
						SerializedProperty m_b_Countdown = serializedObject2.FindProperty("b_Countdown");
						m_b_Countdown.boolValue = true;
						serializedObject2.ApplyModifiedProperties ();
					}

					if (game_Manager == null) {																				// --> Case : We are not on a track scene														
						string objectPath = "Assets/MCR Creator/Assets/Datas/Data_CarList.asset";
						InventoryCar inventoryItemCar = AssetDatabase.LoadAssetAtPath (objectPath, typeof(Object)) as InventoryCar;
						if (inventoryItemCar) {
							SerializedObject serializedObject1 = new UnityEditor.SerializedObject (inventoryItemCar);
							serializedObject1.Update ();
							SerializedProperty m_b_LapCounter = serializedObject1.FindProperty("b_LapCounter");
							m_b_LapCounter.boolValue = true;
							serializedObject1.ApplyModifiedProperties ();

							SerializedObject serializedObject2 = new UnityEditor.SerializedObject (inventoryItemCar);
							serializedObject2.Update ();
							SerializedProperty m_b_Countdown = serializedObject2.FindProperty("b_Countdown");
							m_b_Countdown.boolValue = true;
							serializedObject2.ApplyModifiedProperties ();
						}
					}



					// --> Find and Pause Cars
				/*	GameObject[] arrCars = GameObject.FindGameObjectsWithTag ("Car");												// Delete cars on scene						


					List<GameObject> tmpList	= new List<GameObject> ();
					foreach (GameObject car in arrCars) {
						if (car.GetComponent<CarController> ()) {
							tmpList.Add (car);
						}
					}
					for (var i = tmpList.Count - 1; i >= 0; i--) {
						Undo.DestroyObjectImmediate (tmpList [i]);
					}
					tmpList.Clear ();*/
				}
			}
			serializedObject0.ApplyModifiedProperties ();
			EditorGUILayout.LabelField ("");

			EditorGUILayout.EndVertical ();
	

			if (!game_Manager) {
				GameObject tmpObj = GameObject.Find ("Game_Manager");
				if (tmpObj)
					game_Manager = tmpObj.GetComponent<Game_Manager> ();
			}

			if (m_b_TestMode.boolValue && game_Manager) {																							// if Test Mode activated
				EditorGUILayout.LabelField ("");
				EditorGUILayout.BeginVertical (style_Yellow_01);
				EditorGUILayout.LabelField ("Add cars to test the scene"
			, EditorStyles.boldLabel);


				// Block of code with controls
				// that may set GUI.changed to true.
				EditorGUILayout.HelpBox ("This section allow to add cars in the scene when you want to test a scene" +
				"\n1-Press button ''Apply to create'' for each car you want to create on scene." +
				"\n2-For Car 1 and 2 you could press button ''Player/CPU'' to choose if the car is manage by a Player or a CPU. Don't forget to press ''Apply'' after your modification" +
				"\n3-You could choose the car you want to add in the scene. Drag and drop a car prefab in the first field" +
				"\n" +
				"\nINFO : If you want to delete a car. You could delete the car on the Hierarchy. Or you could Hide the car on the Hierarchy", MessageType.Info);



				if (inventoryItemList && game_Manager) {
					EditorGUILayout.BeginVertical ();
// --> Options to add cars on screen
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("", GUILayout.Width (50));
					EditorGUILayout.LabelField ("Drag and drop your car", GUILayout.Width (150));
					EditorGUILayout.LabelField ("Who control car", GUILayout.Width (100));
					EditorGUILayout.LabelField ("Add car to the scene", GUILayout.Width (120));
					EditorGUILayout.EndHorizontal ();

					for (var i = 0; i < 4; i++) {
						SerializedObject serializedObject01 = new UnityEditor.SerializedObject (inventoryItemList);
						serializedObject01.Update ();
						SerializedProperty m_carName = serializedObject01.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("list_Cars");
						SerializedProperty m_PlayerType = serializedObject01.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("list_playerType");
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Car " + (i + 1) + " :", GUILayout.Width (50));


						if (inventoryItemList) {
							EditorGUILayout.PropertyField (m_carName.GetArrayElementAtIndex (i), new GUIContent (""), GUILayout.Width (150));

							if (i == 0 || i == 1) {
								if (m_PlayerType.GetArrayElementAtIndex (i).boolValue) {
									if (GUILayout.Button ("CPU", GUILayout.Width (50))) {													// Clear All the playerPrefs
										m_PlayerType.GetArrayElementAtIndex (i).boolValue = false;
									}
								} else {
									if (GUILayout.Button ("Player", GUILayout.Width (50))) {													// Clear All the playerPrefs
										m_PlayerType.GetArrayElementAtIndex (i).boolValue = true;
									}
								}

							} else {
								EditorGUILayout.LabelField ("CPU", GUILayout.Width (50));
							}
						}
						EditorGUILayout.LabelField ("", GUILayout.Width (50));
						if (GUILayout.Button ("Apply", GUILayout.Width (60))) {													// Clear All the playerPrefs
							ReplaceACar (i);
						}

						EditorGUILayout.EndHorizontal ();
						serializedObject01.ApplyModifiedProperties ();
					}

					EditorGUILayout.LabelField ("");


					EditorGUILayout.EndVertical ();
				}
				EditorGUILayout.EndVertical ();

				EditorGUILayout.LabelField ("");

				EditorGUILayout.BeginVertical (style_Blue);
				EditorGUILayout.LabelField ("Lap Counter Options"
			, EditorStyles.boldLabel);

				EditorGUILayout.HelpBox ("The next two buttons are the same as buttons you find on gameObject ''StartLine_LapCounter'' on the Hierarchy" +
				"\n(Grp_Manager -> Grp_StartLine -> StartLine_LapCounter", MessageType.Info);


				if (GUILayout.Button ("Initilize car positions on the start line")) {
					InitCarPositionOnTheStartLine ();
				}

				if (!lapCounter) {
					GameObject tmpObj = GameObject.FindGameObjectWithTag ("TriggerStart");
					if (tmpObj)
						lapCounter = tmpObj.GetComponent<LapCounter> ();
				}



				if (lapCounter && game_Manager) {
					SerializedObject serializedObject1 = new UnityEditor.SerializedObject (game_Manager.inventoryItemCar);
					serializedObject1.Update ();
					//SerializedProperty m_b_ActivateLapCounter = serializedObject1.FindProperty ("b_ActivateLapCounter");
					SerializedProperty m_b_LapCounter = serializedObject1.FindProperty("b_LapCounter");


					if (m_b_LapCounter.boolValue) {
						if (GUILayout.Button ("Lap Counter is activated")) {
							m_b_LapCounter.boolValue = false;
						}
					} else {
						if (GUILayout.Button ("Lap Counter is deactivated")) {
							m_b_LapCounter.boolValue = true;
						}
					}
					serializedObject1.ApplyModifiedProperties ();
				}
				EditorGUILayout.EndVertical ();


				EditorGUILayout.LabelField ("");

				EditorGUILayout.BeginVertical (style_Orange);
				EditorGUILayout.LabelField ("Countdown"
			, EditorStyles.boldLabel);

				EditorGUILayout.HelpBox ("The next button is the same as button you find on gameObject ''Game_Manager'' on the Hierarchy" +
				"\n(Grp_Manager -> Game_Manager", MessageType.Info);




				if (game_Manager) {
					SerializedObject serializedObject2 = new UnityEditor.SerializedObject (game_Manager.inventoryItemCar);
					serializedObject2.Update ();
					//SerializedProperty m_b_UseCountdown = serializedObject2.FindProperty ("b_UseCountdown");
					SerializedProperty m_b_Countdown = serializedObject2.FindProperty("b_Countdown");


					if (m_b_Countdown.boolValue) {
						if (GUILayout.Button ("Countdown is activated")) {
							m_b_Countdown.boolValue = false;
						}
					} else {
						if (GUILayout.Button ("Countdown is deactivated")) {
							m_b_Countdown.boolValue = true;
						}
					}
					serializedObject2.ApplyModifiedProperties ();
				}
				EditorGUILayout.EndVertical ();
				EditorGUILayout.LabelField ("");

				EditorGUILayout.BeginVertical (style_Purple);
				if (!game_ManagerDifficulty) {
					GameObject tmpObj = GameObject.Find ("Game_Manager");

					if (tmpObj)
						game_ManagerDifficulty = tmpObj.GetComponent<DifficultyManager> ();
				}

				if (game_ManagerDifficulty) {
					EditorGUILayout.LabelField ("Choose Difficulty for Test Mode", EditorStyles.boldLabel);
					SerializedObject serializedObject3 = new UnityEditor.SerializedObject (game_ManagerDifficulty);
					serializedObject3.Update ();
					SerializedProperty m_SelectedDifficulties = serializedObject3.FindProperty ("selectedDifficulties");

					string tmpDifficulty = "";
					if (m_SelectedDifficulties.intValue == 0) {
						tmpDifficulty = "Easy";
						PlayerPrefs.SetInt ("DifficultyChoise",0); 
					}
					if (m_SelectedDifficulties.intValue == 1) {
						tmpDifficulty = "Medium";
						PlayerPrefs.SetInt ("DifficultyChoise",1); 
					}
					if (m_SelectedDifficulties.intValue == 2) {
							tmpDifficulty = "Expert";
						PlayerPrefs.SetInt ("DifficultyChoise",2); 
					}

					if (GUILayout.Button (tmpDifficulty)) {
						m_SelectedDifficulties.intValue = (m_SelectedDifficulties.intValue + 1) % 3;
					} 
					serializedObject3.ApplyModifiedProperties ();
				}
				EditorGUILayout.EndVertical ();
				EditorGUILayout.LabelField ("");
			}
		}
		GUILayout.EndScrollView();
	}

	void LoadCar(){
		// --> Find and Pause Cars
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							


		List<GameObject> 			tmpList	= new List<GameObject> ();
		foreach (GameObject car in arrCars) {
			if (car.GetComponent<CarController> ()) {
				tmpList.Add(car);
			}
		}
		for (var i = tmpList.Count-1; i >= 0; i--) {
			//Debug.Log(arrCars.Length + " : " +  arrCars[i].name);
			Undo.DestroyObjectImmediate (tmpList[i]);
		}
		tmpList.Clear();


		for (var i = 0; i < 4; i++) {
			SerializedObject serializedObject0 = new UnityEditor.SerializedObject (inventoryItemList);
			serializedObject0.Update ();
			SerializedProperty m_carName = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex(0).FindPropertyRelative("list_Cars");
			SerializedProperty m_PlayerType = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex(0).FindPropertyRelative("list_playerType");

			if (inventoryItemList) {
				if (m_carName.GetArrayElementAtIndex(i).objectReferenceValue != null) {
					GameObject instance = (GameObject)Instantiate (m_carName.GetArrayElementAtIndex(i).objectReferenceValue);

					Undo.RegisterCreatedObjectUndo (instance, instance.name);

					instance.name = instance.name.Replace ("(Clone)", "");

					GameObject tmpPosition = GameObject.Find ("Start_Position_0" + (i+1));

					if (tmpPosition) {
						Undo.RegisterFullObjectHierarchyUndo (instance, "Pos_" + instance.name);

						instance.GetComponent<CarController> ().playerNumber = i + 1;						// Select the player number

						instance.GetComponent<CarAI>().enabled = m_PlayerType.GetArrayElementAtIndex (i).boolValue;

						instance.transform.position = new Vector3(tmpPosition.transform.position.x,tmpPosition.transform.position.y+.15f,tmpPosition.transform.position.z);
						instance.transform.eulerAngles = tmpPosition.transform.eulerAngles;
					}
				}
			}
			serializedObject0.ApplyModifiedProperties ();
		}

	}

	void ReplaceACar(int value){
		// --> Find and Pause Cars
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag ("Car");							

		List<GameObject> tmpList	= new List<GameObject> ();
		foreach (GameObject car in arrCars) {
			if (car.GetComponent<CarController> () && car.GetComponent<CarController> ().playerNumber == (value + 1)) {
				tmpList.Add (car);
			}
		}
			
		for (var i = tmpList.Count - 1; i >= 0; i--) {
			Undo.DestroyObjectImmediate (tmpList [i]);
		}
		tmpList.Clear ();

			
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (inventoryItemList);
		serializedObject0.Update ();
		SerializedProperty m_carName = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("list_Cars");
		SerializedProperty m_PlayerType = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex (0).FindPropertyRelative ("list_playerType");


		if (inventoryItemList) {

			if (m_carName.GetArrayElementAtIndex (value).objectReferenceValue != null) {
				GameObject instance = (GameObject)Instantiate (m_carName.GetArrayElementAtIndex (value).objectReferenceValue);



				Undo.RegisterCreatedObjectUndo (instance, instance.name);

				instance.name = instance.name.Replace ("(Clone)", "");
				GameObject tmpPosition = GameObject.Find ("Start_Position_0" + (value + 1));

				if (tmpPosition) {
					Undo.RegisterFullObjectHierarchyUndo (instance, "Pos_" + instance.name);

					instance.GetComponent<CarController> ().playerNumber = value + 1;
					instance.GetComponent<CarAI> ().enabled = m_PlayerType.GetArrayElementAtIndex (value).boolValue;
					instance.transform.position = new Vector3 (tmpPosition.transform.position.x, tmpPosition.transform.position.y + .15f, tmpPosition.transform.position.z);
					instance.transform.eulerAngles = tmpPosition.transform.eulerAngles;
				}
			}
		}
		serializedObject0.ApplyModifiedProperties ();
	}

	void RemoveACar(int value){
		// --> Find and Pause Cars
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							

		List<GameObject> 			tmpList	= new List<GameObject> ();
		foreach (GameObject car in arrCars) {
			if (car.GetComponent<CarController> () && car.GetComponent<CarController> ().playerNumber == (value +1)) {
				tmpList.Add(car);
			}
		}

		for (var i = tmpList.Count-1; i >= 0; i--) {
			Undo.DestroyObjectImmediate (tmpList[i]);
		}
		tmpList.Clear();
	
		SerializedObject serializedObject0 = new UnityEditor.SerializedObject (inventoryItemList);
		serializedObject0.Update ();
		SerializedProperty m_carName = serializedObject0.FindProperty ("inventoryItem").GetArrayElementAtIndex(0).FindPropertyRelative("list_Cars");


		if (inventoryItemList) {
			m_carName.GetArrayElementAtIndex (value).objectReferenceValue = null;
		}
		serializedObject0.ApplyModifiedProperties ();

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


					car.transform.position = new Vector3(tmpPosition.transform.position.x,tmpPosition.transform.position.y+.15f,tmpPosition.transform.position.z);
					car.transform.eulerAngles = tmpPosition.transform.eulerAngles;
				}

			}
		}

	}
		

	void OnInspectorUpdate()
	{
		Repaint();
	}

}
#endif