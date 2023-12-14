using GameBase;
using NullPointerCore.Backend.ResourceGathering;
using NullPointerGame.ResourceSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(StorageContainer))]
	public class StorageContainerEditor : Editor
	{
		const float addBtnSize = 50.0f;

		GUIStyle styleTableTitle;
		GUIStyle styleTableStored;
		GUIStyle styleTableToggle;
		GUIStyle styleTableButton;

		// Draw the property inside the given rect
		public override void OnInspectorGUI()
		{
			styleTableTitle = new GUIStyle(EditorStyles.miniLabel);
			styleTableTitle.alignment = TextAnchor.MiddleCenter;
			styleTableStored = new GUIStyle(EditorStyles.miniTextField);
			styleTableStored.alignment = TextAnchor.MiddleCenter;
			styleTableStored.margin.top = 6;
			styleTableToggle = new GUIStyle(EditorStyles.toggle);
			styleTableToggle.imagePosition = ImagePosition.ImageOnly;
			styleTableToggle.margin.bottom = -5;
			styleTableButton = new GUIStyle(EditorStyles.miniButton);
			styleTableButton.margin.top = 5;

			serializedObject.Update();

			StorageContainer myTarget = (StorageContainer)target;

			List<string> availableResources = new List<string>();
			GameScene gs = FindGameScene(myTarget.transform);

			if(gs==null)
				EditorGUILayout.HelpBox("No GameScene can be found in the hierarchy.", MessageType.Error);
			else
			{
				ResourceSystem rs=gs.Get<ResourceSystem>();
				if (rs == null)
					EditorGUILayout.HelpBox("No ResourceSystem can be found in the GameScene.", MessageType.Error);
				else
				{
					if(!EditorApplication.isPlaying)
						FindAvailableResourcesInConfiguredStorages(availableResources, rs);
					else
						FindAvailableResourcesInFinalStorages(availableResources, rs);

					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Add Storage", GUILayout.Width(140)))
					{
						// create the menu and add items to it
						GenericMenu menu = new GenericMenu();
						// forward slashes nest menu items under submenus
						foreach (string rid in availableResources)
							menu.AddItem(new GUIContent(rid), false, OnStorageAddRequest, rid);
						// display the menu
						menu.ShowAsContext();
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}
			}

			if( myTarget.initParams.Count == 0 && targets.Length == 1)
				EditorGUILayout.HelpBox("There is no Storage defined for this container.", MessageType.Info);
			else
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("ResourceID", styleTableTitle, GUILayout.Width(EditorGUIUtility.labelWidth));
				GUILayout.Label("Stored", styleTableTitle);
				GUILayout.Label("Capacity", styleTableTitle);
				GUILayout.Label("Overflow", styleTableTitle, GUILayout.Width(60));
				GUILayout.Space(26);
				EditorGUILayout.EndHorizontal();

				int toRemove = -1;
				if(!EditorApplication.isPlaying)
					toRemove = DrawConfiguredStorages(this.serializedObject, availableResources);
				else
					toRemove = DrawStorages(myTarget, availableResources);
				EditorGUILayout.EndVertical();

				if (toRemove>=0)
				{
					foreach (StorageContainer target in targets)
					{
						if(!EditorApplication.isPlaying)
						{
							StorageContainer.InitParams st = target.initParams.Find(x => x.resourceID == toRemove);
							target.initParams.Remove(st);
						}
						else
						{
							target.Remove(toRemove);
						}
					}
				}
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void FindAvailableResourcesInFinalStorages(List<string> availableResources, ResourceSystem rs)
		{
			for (int ridx = 0; ridx < rs.ResourcesCount; ridx++)
			{
				foreach (StorageContainer target in targets)
				{
					if (!target.Contains(ridx))
					{
						availableResources.Add(ResourceSystem.GetResourceName(ridx));
						break;
					}
				}
			}
		}

		private void FindAvailableResourcesInConfiguredStorages(List<string> availableResources, ResourceSystem rs)
		{
			for (int ridx = 0; ridx < rs.ResourcesCount; ridx++)
			{
				foreach (StorageContainer target in targets)
				{
					if (!target.ConfiguredStorages.Contains(ridx))
					{
						availableResources.Add(ResourceSystem.GetResourceName(ridx));
						break;
					}
				}
			}
		}

		GameScene FindGameScene(Transform hierarchy)
		{
			GameScene gs = GameScene.FindInHierarchy(hierarchy);
			if(gs==null)
				gs = GameObject.FindObjectOfType<GameScene>();
			return gs;
		}

		private void OnStorageAddRequest(object ridobj)
		{
			string rid = ridobj as string;
			int rIdx = ResourceSystem.GetResourceIndex(rid);
			foreach(StorageContainer target in targets)
			{
				if(!EditorApplication.isPlaying)
				{
					// Add the storage configuration if it doesn't have it.
					if(target.ConfiguredStorages.Contains(rIdx))
						continue;
					StorageContainer.InitParams config = new StorageContainer.InitParams();
					config.resourceID = rIdx;
					target.initParams.Add(config);
				}
				else
				{
					// Add the final sotrage because we are in play mode
					if(target.Contains(rIdx))
						continue;
					target.Add(rIdx, false, 0.0f);
				}
			}
		}

		private int DrawConfiguredStorages(SerializedObject myTarget, List<string> availableResources)
		{
			int result = -1;

			SerializedProperty initParamsProp = myTarget.FindProperty("initParams");
			//if( initParamsProp.hasMultipleDifferentValues )
			//	Debug.Log("hasMultipleDifferentValues: "+initParamsProp.arraySize);
			
			for( int i=0; i<initParamsProp.arraySize; i++ )
			{
				//initParamsProp.
				SerializedProperty paramProp = initParamsProp.GetArrayElementAtIndex(i);
				try
				{
					SerializedProperty ridProp = paramProp.FindPropertyRelative("resourceID");

					if( ridProp.hasMultipleDifferentValues )
						EditorGUILayout.HelpBox("Multiple different resource types", MessageType.None);
					else
					{
						SerializedProperty storedProp = paramProp.FindPropertyRelative("initialyStored");
						SerializedProperty capProp = paramProp.FindPropertyRelative("minCapacity");
						SerializedProperty overflowProp = paramProp.FindPropertyRelative("overflow");

						EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
						EditorGUILayout.PrefixLabel(ResourceSystem.GetResourceName(ridProp.intValue), EditorStyles.miniLabel);
						//EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PropertyField(storedProp, GUIContent.none, GUILayout.ExpandWidth(true));
						EditorGUILayout.PropertyField(capProp, GUIContent.none, GUILayout.ExpandWidth(true));
				
						// The Table Column for the Overflow property
						GUILayout.Space(16);
						//GUILayout.FlexibleSpace();
						EditorGUILayout.PropertyField(overflowProp, GUIContent.none, GUILayout.Width(20));
						GUILayout.Space(16);
						//GUILayout.FlexibleSpace();
						//EditorGUILayout.PropertyField(overflowProp, GUIContent.none, GUILayout.Width(40), GUILayout.ExpandWidth(true));

						if (GUILayout.Button("X", styleTableButton, GUILayout.Width(18)))
							result = ridProp.intValue;
						//EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndHorizontal();
					}
				}
				catch(InvalidOperationException)
				{

				}
			}
			return result;
		}

		private int DrawStorages(StorageContainer myTarget, List<string> availableResources)
		{
			int toRemove = -1;
			foreach (Storage storage in myTarget.Storages)
			{
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
				EditorGUILayout.PrefixLabel(ResourceSystem.GetResourceName(storage.ResourceID), EditorStyles.miniLabel);

				if (availableResources.Contains(ResourceSystem.GetResourceName(storage.ResourceID)))
					EditorGUILayout.HelpBox("Not available in all the selected Containers", MessageType.None);
				else if (targets.Length > 1) // Maybe i can do the multi edit in some future
					EditorGUILayout.HelpBox("Internal data cannot be multi-edited for now...", MessageType.None);
				else
				{
					EditorGUILayout.DelayedFloatField(storage.Stored, styleTableStored, GUILayout.ExpandWidth(true));
					EditorGUILayout.DelayedFloatField(storage.Capacity, styleTableStored, GUILayout.ExpandWidth(true));

					// The Table Column for the Overflow property
					EditorGUILayout.BeginHorizontal(GUILayout.Width(60));
					GUILayout.Space(20);
					EditorGUILayout.Toggle(storage.AllowOverflow, styleTableToggle);
					EditorGUILayout.EndHorizontal();
				}
				//GUILayout.Space(18);
				if (GUILayout.Button("X", styleTableButton, GUILayout.Width(18)))
					toRemove = storage.ResourceID;
				EditorGUILayout.EndHorizontal();
			}

			return toRemove;
		}
	}
}