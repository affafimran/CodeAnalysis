using System;
using NullPointerGame.BuildSystem;
using UnityEditor;
using UnityEngine;
using GameBase;
using GameBase.RTSKit;

namespace NullPointerEditor
{
	[CustomEditor(typeof(UnitConfig), true)]
	class UnitConfigEditor : Editor
	{
		UnitConfig Target { get { return target as UnitConfig; } }
		public override bool UseDefaultMargins()
		{
			return false;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			if( GUILayout.Button("Instantiate In Scene", EditorStyles.toolbarButton) )
				InstantiateInScene();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			base.OnInspectorGUI();
			EditorGUILayout.EndVertical();
		}

		private void InstantiateInScene()
		{
			Vector3 spawnPosition = Vector3.zero;
			Quaternion spawnRotation = Quaternion.identity;
			
			if( SceneView.lastActiveSceneView != null )
			{
				Ray cameraLookAtRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(Vector3.one/2);
				if( GameScene.ValidateExists(this) )
				{
					SceneBounds sceneBounds = GameSceneSystem.GetValid<SceneBounds>(this);
					if( sceneBounds.Raycast(cameraLookAtRay, ref spawnPosition) )
					{
						Vector3 cornerPos = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(Vector3.zero);
						float radius = Vector3.Distance(cornerPos,spawnPosition);
						spawnPosition = spawnPosition + (Vector3)UnityEngine.Random.insideUnitCircle * radius * 0.5f;
					}
				}
			}
			else
			{
				Debug.LogWarning("It's required that the user have focus over the SceneView in order to use this feature.");
			}
			GameEntity spawned = Target.SpawnFinal(null, spawnPosition, spawnRotation);
			EditorGUIUtility.PingObject(spawned);
		}
	}
}
