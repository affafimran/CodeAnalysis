using GameBase;
using GameBase.RTSKit;

using NullPointerGame.NavigationSystem;
using UnityEngine;
using UnityEngine.Events;

namespace NullPointerGame.Extras
{
	/// <summary>
	/// Helper class to handle simple units movement for the test scenes.
	/// </summary>
	public class SimpleMoveSystem : GameSceneSystem
	{
		/// <summary>
		/// Set here the reference to the SceneBounds
		/// </summary>
		private SceneBounds sceneBounds;

		[System.Serializable]
		public class SelectableEvent : UnityEvent<Selectable> { }
		public SelectableEvent onActionOverSelectable;

		private SelectionSystem selectionSystem  = null;

		public void Start()
		{
			selectionSystem = gameScene.Get<SelectionSystem>();
			sceneBounds = gameScene.Get<SceneBounds>();
		}

		public void ProcessTargetSelectEvent()
		{
			if( !selectionSystem.HasSelectedEntities )
				return;

			if( selectionSystem.HasHoveredEntities )
			{
				foreach( Selectable sel in selectionSystem.Hoverings )
				{
					if(onActionOverSelectable!=null)
						onActionOverSelectable.Invoke(sel);
				}
			}
			else
			{
				foreach( Selectable sel in selectionSystem.Selecteds )
				{
					Navigation nav = sel.GetComponent<Navigation>();
					if(nav && sceneBounds)
					{
						Vector3 dir = (sceneBounds.CursorLookPoint-nav.BasePosition).normalized;
						nav.PrepareToMove(sceneBounds.CursorLookPoint, dir);
						nav.EngageMovement();
					}
				}
			}
		}
	}
}