using GameBase;
using GameBase.AttributeExtension;
using GameBase.RTSKit;
using NullPointerCore.CoreSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NullPointerGame.BuildSystem;
using System;

namespace SpaceRTS.Core
{
	/// <summary>
	/// Is the Main Controller. The RTS implementation of the GameScene. Handles the input 
	/// according to the current state and send it to the game systems.
	/// </summary>
	public class HumanPlayerHandler : PlayerSystem, ISelectableCriteria
	{
		static Player localPlayer = null;

		/// <summary>
		/// The Gameplay GameEntity Prefab that will used when a structure is build in the map until it's complete.
		/// </summary>
		[System.Obsolete("Now the construct prefab is defined in each ship builder component.")]
		[HideInInspector]
		public RTSEntity constructPrefab;
		/// <summary>
		/// Reference to the SelectionInput
		/// </summary>
		public SelectionInput selectionInput;
		/// <summary>
		/// Transform of the GameObject that will act as a container to all created game entities.
		/// </summary>
		public Transform entitiesContainer;

		[SerializeField][ReadOnly]
		private bool isPointerOverGame = true;
		private bool isDoubleClick = false;

		/// <summary>
		/// Reference to the PlaceMarker that can be used to mark a destination point in the map.
		/// </summary>
		private PlaceMarker placeMarker;
		/// <summary>
		/// Reference to the SceneBounds
		/// </summary>
		private SceneBounds sceneBounds;
		private GroupController groupController;
		private BuildHandler builderGhost;
		private SelectionSystem selectionSystem;

		public static Player GetLocalPlayer() { return localPlayer; }
		/// <summary>
		/// Remarks if the cursor is over the game client area
		/// </summary>
		public bool IsPointerOverGame {	get	{ return isPointerOverGame; } set {	isPointerOverGame = value; } }

		public void OnPointerClickEvent(BaseEventData data)
		{
			PointerEventData pointerData = (PointerEventData) data;
			isDoubleClick = pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount >= 2;
		}

		void Start()
		{
			if( localPlayer == null )
				localPlayer = ThisPlayer;

			GameScene gameScene = ThisPlayer.GameScene;
			placeMarker = gameScene.Get<PlaceMarker>();
			sceneBounds = gameScene.Get<SceneBounds>();
			groupController = gameScene.Get<GroupController>();
			if(groupController)
				groupController.humanPlayer = ThisPlayer;
			builderGhost = gameScene.Get<BuildHandler>();
			selectionSystem = gameScene.Get<SelectionSystem>();
			if(selectionInput)
				selectionInput.selectionCriteria = this;
			if(groupController && selectionSystem)
			{
				selectionSystem.selectionAddedEntity += groupController.AddShip;
				selectionSystem.selectionRemovedEntity += groupController.RemoveShip;
				selectionSystem.selectionRemovedAll += groupController.RemoveAll;
			}
		}

		void OnDestroy()
		{
			if(groupController && selectionSystem)
			{
				selectionSystem.selectionAddedEntity -= groupController.AddShip;
				selectionSystem.selectionRemovedEntity -= groupController.RemoveShip;
				selectionSystem.selectionRemovedAll -= groupController.RemoveAll;
			}
		}

		void Update()
		{
			if(isPointerOverGame)
			{
				// Left mouse button up (click performed)
				if (Input.GetMouseButtonUp(0) && GUIUtility.hotControl==0)
				{
					// Is trying to stablish a build position?
					if (builderGhost && builderGhost.IsGhostRequested())
						builderGhost.Confirm();
					// There's a double click over a selectable unit?
					if (selectionSystem.HasHoveredEntities && isDoubleClick )
						SelectAllVisibleFromSameType(selectionSystem.OrderedHoveringEntities[0]);
					// If not the click will be delivered to the selection input system.
					else if(selectionInput)
						selectionInput.ProcessLeftClickEvent();
				}
				// Right mouse button down (starting to drag)
				if (Input.GetMouseButtonDown(1) && GUIUtility.hotControl==0)
				{
					// Is trying to stablish a build position?
					if (builderGhost && builderGhost.IsGhostRequested())
						builderGhost.Cancel();
					// IS over an game entity? Try to perform an action over that entity... 
					// in this case try to continue the build if it's possible.
					else if (groupController && selectionSystem.HasHoveredEntities)
					{
						groupController.DoActionOver(selectionSystem.Hoverings);
					}
					else if(groupController) // Is trying to move the current selected units, starts the move handler.
					{
						groupController.MoveHandlerBegin();
						if( !groupController.IsMoveHandlerReadyToMove() )
						{
							// None of the selected units can move. if structures are selected
							// then the order must be a change of the rally point position.
							if( groupController.ChangeBuildersRallyPoint(sceneBounds.CursorLookPoint) )
								placeMarker.ShowAt(sceneBounds.CursorLookPoint);
						}
					}
				}
			}
			// If it's the move handler enabled because we are trying to meve the selected ships...
			if (groupController != null)
			{
				if( groupController.TryingToMove )
				{
					// Drags the move handler every frame
					groupController.MoveHandlerDrag();
					// Is the right mouse up in this frame?
					if (Input.GetMouseButtonUp(1) && GUIUtility.hotControl==0)
						groupController.MoveHandlerEnd();
				}
				else if( selectionSystem.HasSelectedEntities && selectionSystem.HasHoveredEntities )
					groupController.TryShowActionFeedback(selectionSystem.Hoverings);
			}
		}

		private void SelectAllVisibleFromSameType(Selectable selectable)
		{
			RTSEntity rtsEntity = selectable.GetComponent<RTSEntity>();
			if(rtsEntity == null)
				return;
			Player owner = rtsEntity.GetComponent<PlayerControlled>().Owner;
			PlayerUnitsInView inView = owner.GetSystem<PlayerUnitsInView>();
			List<Selectable> sameTypeUnits = new List<Selectable>();
			foreach( GameEntity inViewEntity in inView.InViewEntities )
			{
				Selectable sel = inViewEntity.GetComponent<Selectable>();
				RTSEntity ent = inViewEntity.GetComponent<RTSEntity>();
				if( sel && ent && ent.unitConfiguration == rtsEntity.unitConfiguration )
					sameTypeUnits.Add(sel);
			}
			selectionSystem.RemoveAllHovering();
			selectionSystem.SetAsHovering(sameTypeUnits);
			selectionSystem.SetAsHighlighted(sameTypeUnits);
			selectionSystem.ConfirmAllHighlightsAsSelected();
			selectionSystem.RemoveAllHovering();
		}

		/// <summary>
		/// Must be called when the cursor enters the game client area of screen of it that
		/// area is no longer covered by another panel.
		/// </summary>
		public void OnPointerEnterGameArea()
		{
			isPointerOverGame = true;

			if (builderGhost && builderGhost.IsGhostRequested())
				builderGhost.Show(true);
		}

		/// <summary>
		/// Must be called when the cursor exits the game client area of screen or if that 
		/// area is covered by another panel.
		/// </summary>
		public void OnPointerExitGameArea()
		{
			isPointerOverGame = false;

			if (builderGhost && builderGhost.IsGhostRequested())
				builderGhost.Show(false);
		}

		public IEnumerable<Selectable> FilteringSelectablesByBox(IEnumerable<Selectable> source)
		{
			if( IsHoveringPlayerUnits(source) )
				source = SelectOnlyPlayerUnits(source);
			if( IsHoveringAShip(source) )
				return SelectOnlyShips(source);
			else
				return source;
		}

		private bool IsHoveringPlayerUnits(IEnumerable<Selectable> source)
		{
			foreach(Selectable sel in source)
			{
				PlayerControlled ownerInfo = sel.GetComponent<PlayerControlled>();
				if(ownerInfo==null)
					continue;
				if( ownerInfo.Owner == ThisPlayer)
					return true;

			}
			return false;
		}

		private bool IsHoveringAShip(IEnumerable<Selectable> source)
		{
			foreach(Selectable sel in source)
			{
				RTSEntity rtsEnt = sel.GetComponent<RTSEntity>();
				if(rtsEnt==null)
					continue;
				if( rtsEnt.Config is RTSShipConfig)
					return true;

			}
			return false;
		}

		private IEnumerable<Selectable> SelectOnlyPlayerUnits(IEnumerable<Selectable> source)
		{
			foreach(Selectable sel in source)
			{
				PlayerControlled ownerInfo = sel.GetComponent<PlayerControlled>();
				if(ownerInfo==null)
					continue;
				if( ownerInfo.Owner == ThisPlayer)
					yield return sel;
			}
			yield break;
		}

		private IEnumerable<Selectable> SelectOnlyShips(IEnumerable<Selectable> source)
		{
			foreach(Selectable sel in source)
			{
				RTSEntity rtsEnt = sel.GetComponent<RTSEntity>();
				if(rtsEnt==null)
					continue;
				if( rtsEnt.Config is RTSShipConfig)
					yield return sel;
			}
			yield break;
		}


		public IEnumerable<Selectable> FilteringSelectablesByRay(IEnumerable<Selectable> source)
		{
			if( IsHoveringAShip(source) )
				return SelectOnlyShips(source);
			else
				return source;
		}
	}
}
