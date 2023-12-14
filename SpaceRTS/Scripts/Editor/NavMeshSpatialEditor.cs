using NullPointerGame.NavMeshIntegration;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using System;
using GameBase;
using NullPointerGame.Spatial;

namespace NullPointerEditor
{
	[CustomEditor(typeof(NavMeshSpatialSystem))]
	public class NavMeshSpatialEditor : GameSceneSystemEditor
	{
		public IEnumerable<NavMeshSurface> Surfaces { get {	return NavMeshSurface.activeSurfaces; } }
		public NavMeshSpatialSystem Target { get { return target as NavMeshSpatialSystem; } }

        void OnEnable()
        {
            NavMeshVisualizationSettings.showNavigation++;
			Target.OnRebuildCompleted += OnRebuildCompleted;
        }

		void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
			Target.OnRebuildCompleted -= OnRebuildCompleted;
        }

		public override void OnInspectorGUI()
		{
			
			NullPointerGUIUtility.DrawRequiredGameSystems(targets, Target.gameObject);
			if( NavMeshSurface.activeSurfaces.Count == 0 )
			{
				if( NullPointerGUIUtility.DrawWarnBox("Requires at least one NavMeshSurface.", "Fix") )
				{
					GameScene.EntitiesParent.gameObject.AddComponent<NavMeshSurface>();
					Selection.activeGameObject = GameScene.EntitiesParent.gameObject;
					EditorGUIUtility.PingObject(GameScene.EntitiesParent);
				}
			}
			if( SpatialModifierCollector.collectors.Count == 0 )
			{
				if( NullPointerGUIUtility.DrawWarnBox("Requires at least one SpatialModifierColector.", "Fix") )
				{
					SpatialModifierCollector collector = GameScene.EntitiesParent.gameObject.AddComponent<SpatialModifierCollector>();
					collector.Collect();
					if(collector.modifiers.Count == 0)
					{
						SpatialBoxModifier boxModifier = GameScene.EntitiesParent.gameObject.AddComponent<SpatialBoxModifier>();
						boxModifier.size.x = 200f;
						boxModifier.size.z = 200f;
						collector.Collect();
					}
					Selection.activeGameObject = GameScene.EntitiesParent.gameObject;
					EditorGUIUtility.PingObject(GameScene.EntitiesParent);
				}
			}

			this.DrawDefaultInspector();

			NavMeshSpatialSystem nmss = target as NavMeshSpatialSystem;

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if(nmss.IsRebuilding() )
			{
				if( GUILayout.Button("Stop Rebuild") )
					nmss.StopRebuild();
			}
			else
			{
				if( GUILayout.Button("Rebuild") )
					nmss.Rebuild();
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		private void OnRebuildCompleted()
		{
			SceneView.RepaintAll();
		}
	}
}