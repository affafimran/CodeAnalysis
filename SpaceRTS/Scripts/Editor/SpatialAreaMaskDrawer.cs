using GameBase;
using NullPointerGame.Spatial;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor.Spatial
{
	[CustomPropertyDrawer (typeof (SpatialAreaMaskAttribute))]
	public class SpatialAreaMaskDrawer : PropertyDrawer
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

				int areaMask = property.intValue;
				EditorGUI.BeginProperty(position, GUIContent.none, property);

				EditorGUI.BeginChangeCheck();
				areaMask = EditorGUI.MaskField(position, label, areaMask, areaNames);
				//areaIndex = EditorGUI.Popup(rect, labelName, areaIndex, areaNames);

				if (EditorGUI.EndChangeCheck() && areaMask >= 0 && areaMask < areaNames.Length)
					property.intValue = areaMask;
			}
			else
				EditorGUI.PropertyField(position, property, true);
		}
	}
}