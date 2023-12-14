using GameBase;
using NullPointerGame.Spatial;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CustomPropertyDrawer (typeof (SpatialAreaAttribute))]
	public class SpatialAreaDrawer : PropertyDrawer
	{
		SpatialSystem spatial = null;

		// Draw the property inside the given rect
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if(spatial==null)
			{
				GameScene gameScene = GameObject.FindObjectOfType<GameScene>();
				spatial = gameScene.Get<SpatialSystem>();
			}
			if(spatial!=null)
			{
				string [] areaNames = spatial.GetAreaNames();
				if(areaNames==null)
					areaNames = GameObjectUtility.GetNavMeshAreaNames();

				int area = property.intValue;
				EditorGUI.BeginProperty(position, GUIContent.none, property);

				EditorGUI.BeginChangeCheck();
				area = EditorGUI.Popup(position, label.text, area, areaNames);
				//areaIndex = EditorGUI.Popup(rect, labelName, areaIndex, areaNames);

				if (EditorGUI.EndChangeCheck() && area >= 0 && area < areaNames.Length)
					property.intValue = area;
			}
			else
				EditorGUI.PropertyField(position, property, true);
		}
	}
}