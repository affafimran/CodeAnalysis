using GameBase;
using NullPointerGame.ResourceSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CustomPropertyDrawer(typeof(ResourcesMask))]
	public class ResourcesMaskDrawer : PropertyDrawer
	{
		ResourceSystem resourceSystem=null;

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if(resourceSystem==null || resourceSystem.gameObject == null)
				resourceSystem = FindResourceSystem(property);

			List<string> resourceTypes = new List<string>();
			if (resourceSystem != null)
			{
				resourceTypes.AddRange(resourceSystem.DefinedResources);

				EditorGUI.BeginProperty(position, label, property);
				EditorGUI.BeginChangeCheck();

				SerializedProperty maskProp = property.FindPropertyRelative("mask");
				int currentMask = maskProp.intValue;
				int newMask = EditorGUI.MaskField(position, label, currentMask, resourceTypes.ToArray());

				if (currentMask != newMask && (EditorGUI.EndChangeCheck() || !property.hasMultipleDifferentValues))
					maskProp.intValue = newMask;
				EditorGUI.EndProperty();
			}
			else
			{
				EditorGUI.LabelField(position, label, new GUIContent("No ResourceSystem in Scene."), EditorStyles.helpBox);
			}
		}

		public static ResourceSystem FindResourceSystem(SerializedProperty property)
		{
			if( property.serializedObject.targetObject is MonoBehaviour )
			{
				MonoBehaviour mb = property.serializedObject.targetObject as MonoBehaviour;
				GameScene gs = GameScene.FindInHierarchy(mb.transform);
				if(gs==null)
					gs = GameObject.FindObjectOfType<GameScene>();
				if(gs!=null)
					return gs.Get<ResourceSystem>();
			}
			return null;
		}
	}
}