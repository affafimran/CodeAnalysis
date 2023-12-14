using GameBase;
using UnityEditor;
using UnityEngine;


namespace NullPointerGame
{
	public class SpaceRTSEditorHelper
	{
		// Add a menu item to create custom GameObjects.
		// Priority 1 ensures it is grouped with the other menu items of the same kind
		// and propagated to the hierarchy dropdown and hierarch context menus.
		[MenuItem("GameObject/SpaceRTS/GameScene", false, 10)]
		static void CreateCustomGameObject(MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("GameController");
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			/*GameScene gameScene = */

			GameObject playersContainer = new GameObject(GameScene.playersContainerName);
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(playersContainer, go);

			GameObject systemsContainer = new GameObject(GameScene.systemsContainerName);
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(systemsContainer, go);

			GameObject entitiesContainer = new GameObject(GameScene.entitiesContainerName);
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(entitiesContainer, go);

			go.AddComponent<GameScene>();
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}
	}
}