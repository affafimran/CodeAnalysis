using GameBase;
using NullPointerGame.ResourceSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CustomPropertyDrawer(typeof(ResourceID))]
	public class ResourceIDDrawer : PropertyDrawer
	{
		//GUIContent errorContent = new GUIContent("ResourceSystem couldn't be found.");
		[SerializeField]
		ResourceSystem resourceSystem=null;
		
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if(resourceSystem==null || resourceSystem.gameObject == null)
				resourceSystem = FindResourceSystem(property);
			SerializedProperty idProp = property.FindPropertyRelative("id");

			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);
		
			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			Rect rectContent = EditorGUI.PrefixLabel(position, label);
			EditorGUI.indentLevel = 0;

			if (resourceSystem != null && resourceSystem.ResourcesCount > 0)
			{
				List<string> availableResources = new List<string>(resourceSystem.DefinedResources);
				//int currIndex = availableResources.IndexOf(idProp.stringValue);
				int currIndex = idProp.intValue;
				if (currIndex < 0 || currIndex > availableResources.Count)
					currIndex = 0;
				currIndex = EditorGUI.Popup(rectContent, currIndex, availableResources.ToArray());
				idProp.intValue = currIndex;
				//idProp.stringValue = availableResources[currIndex];
			}
			else
				idProp.intValue = EditorGUI.IntField(rectContent, idProp.intValue);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}

		public static ResourceSystem FindResourceSystem(SerializedProperty property)
		{
			GameScene gs = GameObject.FindObjectOfType<GameScene>();
			if(gs!=null)
				return gs.Get<ResourceSystem>();
			return null;
		}
	}
}