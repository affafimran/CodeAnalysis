// Description : MC_CreateGroup.cs : Create a group of gameObjects with ctrl+g or command+g
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

public class MC_CreateGroup : MonoBehaviour {
	[MenuItem("Edit/Group Selected GameObjets %g", false, 0)]							// --> Right clic to create a group
	static void Create_A_Group_Of_GameObjects(){			
		if (Selection.objects.Length > 1)														// If there are more than one gameobject selected
		{	
			if (!Selection.activeTransform) return;

			var NewGameObject = new GameObject("Group");										// Create the group

			Undo.RegisterCreatedObjectUndo(NewGameObject, "Grp_Selection");						// create undo

			NewGameObject.transform.SetParent(Selection.activeTransform.parent, false);			// make gameObjects parent to gameObject ''Group''


			Vector3 center = new Vector3(0, 0, 0);												// Find the center of the selection
			float count  = 0;


			foreach (var zombieInrange in Selection.transforms){
				center += zombieInrange.transform.position;
				count++;
			}
			Vector3 theCenter = center / count;

			NewGameObject.transform.position = theCenter;										// move the group to center position


			foreach (var transform in Selection.transforms) 							
				Undo.SetTransformParent(transform, NewGameObject.transform, "Grp_Selection");	// create undo transform position and change the parent 

			Selection.activeGameObject = NewGameObject;											// select the group
		}
	}

// --> Init all PlayerPrefs
	[MenuItem("Tools/MCR/Other/Delete all PlayerPrefs")]
	static void DeletePLayerPrefs()
	{
		PlayerPrefs.DeleteAll();													
	}


	// --> Delete Editor Prefs Key MCRGlobalPref_Path
	/*[MenuItem("Tools/MCR/Other/Delete MCRGlobalPref_Path")]
	static void DeleteMCRGlobalPref_Path()
	{
		EditorPrefs.DeleteKey("MCRGlobalPref_Path");													
	}*/


// --> Ready For Mobile 
	[MenuItem("Tools/MCR/Switch Platform/Ready For Mobile")]
	static void ReadyForMobile()
	{
		GameObject gameManager = GameObject.Find ("Game_Manager");
		if (gameManager) {
			
			SerializedObject serializedObject2 = new UnityEditor.SerializedObject (gameManager.GetComponent<Game_Manager>().inventoryItemCar);
			serializedObject2.Update ();
			SerializedProperty m_b_mobile = serializedObject2.FindProperty ("b_mobile");
			m_b_mobile.boolValue = true;

			serializedObject2.ApplyModifiedProperties ();
			GameObject eventSystem = GameObject.Find ("EventSystem");
			if(eventSystem)Selection.activeGameObject = eventSystem;
		}

		GameObject canvas_MainMenu = GameObject.Find ("Canvas_MainMenu");
		if (canvas_MainMenu) {

			SerializedObject serializedObject2 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>());
			serializedObject2.Update ();
			SerializedProperty m_b_DesktopOrMobile = serializedObject2.FindProperty ("b_DesktopOrMobile");
			m_b_DesktopOrMobile.boolValue = true;

			serializedObject2.ApplyModifiedProperties ();


			for (int m = 0; m < canvas_MainMenu.GetComponent<Menu_Manager>().List_GroupCanvas.Count; m++) {
				for (int i = 0; i < canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject.Count; i++) {


					if (!canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].Desktop) {
						if (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList) {
							SerializedObject serializedObject3 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList);
							serializedObject3.Update ();
							SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_IsActive");
							tmpSer2.boolValue = true;
							serializedObject3.ApplyModifiedProperties ();
						}
					} 
					else {
						if (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList) {
							SerializedObject serializedObject3 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList);
							serializedObject3.Update ();
							SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_IsActive");
							tmpSer2.boolValue = false;
							serializedObject3.ApplyModifiedProperties ();
						}
					}
				}
			}



			GameObject eventSystem = GameObject.Find ("EventSystem");
			if(eventSystem)Selection.activeGameObject = eventSystem;

			Button[] allUIButtons = canvas_MainMenu.GetComponentsInChildren<Button>(true);

			foreach (Button _button in allUIButtons) {
				Undo.RegisterFullObjectHierarchyUndo (_button.gameObject, _button.name);

				SerializedObject serializedObject3 = new UnityEditor.SerializedObject (_button.gameObject.GetComponent<Button>());
				serializedObject3.Update ();
				SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_Transition");
				tmpSer2.enumValueIndex = 0;														//_button.transition =  Selectable.Transition.SpriteSwap;
				serializedObject3.ApplyModifiedProperties ();
			}

		}

		Debug.Log ("MRC Creator : IMPORTANT");
		Debug.Log ("-You need to de the Operation ''Ready For Mobile'' on each scene of your project");
		Debug.Log ("-Don't forget to use materials for mobile");
		Debug.Log("(More information on documentation)");
	}
// --> Ready For Desktop
	[MenuItem("Tools/MCR/Switch Platform/Ready For Desktop")]
	static void ReadyForDesktop()
	{
		GameObject gameManager = GameObject.Find ("Game_Manager");
		if (gameManager) {
			SerializedObject serializedObject2 = new UnityEditor.SerializedObject (gameManager.GetComponent<Game_Manager>().inventoryItemCar);
			serializedObject2.Update ();
			SerializedProperty m_b_mobile = serializedObject2.FindProperty ("b_mobile");
			m_b_mobile.boolValue = false;

			serializedObject2.ApplyModifiedProperties ();

			GameObject eventSystem = GameObject.Find ("EventSystem");
			if(eventSystem)Selection.activeGameObject = eventSystem;
		}

		GameObject canvas_MainMenu = GameObject.Find ("Canvas_MainMenu");
		if (canvas_MainMenu) {

			SerializedObject serializedObject2 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>());
			serializedObject2.Update ();
			SerializedProperty m_b_DesktopOrMobile = serializedObject2.FindProperty ("b_DesktopOrMobile");
			m_b_DesktopOrMobile.boolValue = false;

			serializedObject2.ApplyModifiedProperties ();


			for (int m = 0; m < canvas_MainMenu.GetComponent<Menu_Manager>().List_GroupCanvas.Count; m++) {
				for (int i = 0; i < canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject.Count; i++) {


					if (!canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].Desktop) {
						if (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList) {
							SerializedObject serializedObject3 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList);
							serializedObject3.Update ();
							SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_IsActive");
							tmpSer2.boolValue = false;
							serializedObject3.ApplyModifiedProperties ();
						}
					} 
					else {
						if (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList) {
							SerializedObject serializedObject3 = new UnityEditor.SerializedObject (canvas_MainMenu.GetComponent<Menu_Manager>().list_gameObjectByPage[m].listOfMenuGameobject[i].objList);
							serializedObject3.Update ();
							SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_IsActive");
							tmpSer2.boolValue = true;
							serializedObject3.ApplyModifiedProperties ();
						}
					}
				}
			}
			GameObject eventSystem = GameObject.Find ("EventSystem");
			if(eventSystem)Selection.activeGameObject = eventSystem;

			Button[] allUIButtons = canvas_MainMenu.GetComponentsInChildren<Button>(true);

			foreach (Button _button in allUIButtons) {
				Undo.RegisterFullObjectHierarchyUndo (_button.gameObject, _button.name);
			

				SerializedObject serializedObject3 = new UnityEditor.SerializedObject (_button.gameObject.GetComponent<Button>());
				serializedObject3.Update ();
				SerializedProperty tmpSer2 = serializedObject3.FindProperty ("m_Transition");
				tmpSer2.enumValueIndex = 2;															//_button.transition =  Selectable.Transition.SpriteSwap;
				serializedObject3.ApplyModifiedProperties ();
			}
		}
		Debug.Log ("MRC Creator : IMPORTANT");
		Debug.Log ("-You need to do the Operation ''Ready For Desktop'' on each scene of your project");
		Debug.Log ("-Don't forget to use materials for Desktop");
		Debug.Log("(More information on documentation)");
	}



}
#endif
