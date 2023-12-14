using NullPointerGame.ResourceSystem;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CustomPropertyDrawer(typeof(PlayerStorageSetter.Entry))]
	public class StorageSetInfoDrawer : PropertyDrawer
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

			// Don't make child fields be indented
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			Rect resourceRect = new Rect(position.x, position.y, position.width-170, position.height);
			Rect actionRect = new Rect(position.x+position.width-165, position.y, 80, position.height);
			Rect amountRect = new Rect(position.x+position.width-80, position.y, 80, position.height);

			// Draw fields - passs GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(actionRect, property.FindPropertyRelative("action"), GUIContent.none);
			EditorGUI.PropertyField(resourceRect, property.FindPropertyRelative("resourceID"), GUIContent.none);
			EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"), GUIContent.none);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}