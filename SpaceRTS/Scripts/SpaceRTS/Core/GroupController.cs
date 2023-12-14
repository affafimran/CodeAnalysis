using System.Collections.Generic;
using UnityEngine;
using System;
using GameBase;
using GameBase.RTSKit;
using NullPointerGame.ResourceSystem;
using NullPointerGame.NavigationSystem;
using NullPointerCore.Backend.Commands;
using SpaceRTS.Commands;
using NullPointerGame.BuildSystem;
using NullPointerCore.CoreSystem;
using NullPointerGame.DamageSystem;

namespace SpaceRTS.Core
{
	/// <summary>
	/// Game system that controls the movement handler (which obtains a destination and final look 
	/// direction for the moved entities.
	/// </summary>
	public class GroupController : GameSceneSystem
	{
		/// <summary>
		/// Set here the reference to the SceneBounds
		/// </summary>
		public SceneBounds sceneBounds;
		/// <summary>
		/// List of entities to move
		/// </summary>
		public List<GameEntity> entities = new List<GameEntity>();
		/// <summary>
		/// How far the cursor has to be from the destination point to change it's direction.
		/// </summary>
		public float directionPrecision = 20.0f;
		/// <summary>
		/// Default separation between ships when are in formation.
		/// </summary>
		public float defaultSeparation = 1.2f;
		/// <summary>
		/// Event triggered when list of entities change.
		/// </summary>
		public event Action OnShipsChanged;
		/// <summary>
		/// Max units where the correct formation will be a single row line.
		/// </summary>
		public int maxUnitsPerRow = 6;
		/// <summary>
		/// The current human player. It will be setted by the HumanPlayerHandler.
		/// </summary>
		[HideInInspector]
		public Player humanPlayer;

        private List<Navigation> navigations = new List<Navigation>();
        private List<Vector3> formationOffsets = new List<Vector3>();
		private bool tryingToMove = false;
		private Vector3 destination;
		private Vector3 direction;
		private Vector3 groupCenterBasePos;

		/// <summary>
		/// Current controlled entities collection
		/// </summary>
        public IEnumerable<GameEntity> Entities { get { return entities; } }
		/// <summary>
		/// Quantity of the current controlled entities
		/// </summary>
        public int EntitiesCount { get { return entities.Count; } }
		/// <summary>
		/// Returns true when the movement handler is active.
		/// </summary>
		public bool TryingToMove { get { return tryingToMove; } }

		override protected void OnValidate()
		{
			base.OnValidate();
			if( this.gameObject.scene.IsValid() && GameScene.ValidateExists(this) )
				sceneBounds = GameSceneSystem.GetValid<SceneBounds>(this);
		}

		#region Movement Related

		/// <summary>
		/// Returns true if the MoveHandlerBegin method was successful.
		/// </summary>
		/// <returns>True if at least one of the entities are ready and capable to move. False otherwise.</returns>
		public bool IsMoveHandlerReadyToMove()
		{
			return navigations.Count > 0 && tryingToMove;
		}

		public IEnumerable<T> Collect<T>() where T : GameEntityComponent
		{
			if(humanPlayer!=null)
				return humanPlayer.CollectOwned<T>(Entities);
			return GameEntity.Collect<T>(Entities);
		}

		/// <summary>
		/// Collects the navigations from the grouped entities and starts the movement 
		/// handler obtaining a destination and a group center.
		/// </summary>
		public void MoveHandlerBegin()
		{
			navigations = new List<Navigation>(Collect<Navigation>());
			if(navigations.Count == 0)
				return;
			Vector3 cursorWorldPos = sceneBounds.CursorLookPoint;
			tryingToMove = true;
			destination = cursorWorldPos;
			groupCenterBasePos = CalcMedianPosition(navigations);
		}

		/// <summary>
		/// After the destination is established by the MoveHanlerBegin method, we need to obtain a final direction.
		/// This is achieved during the execution of this method that needs to be called each frame the cursor is 
		/// dragged through the screen.
		/// </summary>
		public void MoveHandlerDrag()
		{
			Vector3 cursorWorldPos = sceneBounds.CursorLookPoint;
			direction = cursorWorldPos - destination;

			float sqrDist = (cursorWorldPos - destination).sqrMagnitude;
			direction = Vector3.Lerp(direction, destination - groupCenterBasePos, 1.0f - (sqrDist / directionPrecision));

			formationOffsets = CalcFormationOffsets(direction.normalized);
			for (int i = 0; i < formationOffsets.Count; i++)
				navigations[i].ShowMoveMarkerAt(destination + formationOffsets[i]);
				//navigations[i].PrepareToMove(destination + formationOffsets[i], direction);
		}

		/// <summary>
		/// Ends the move handler and assign the movement target to all the Navigations that are part of the GameEntities
		/// in this group controller.
		/// After the drag is finished we need to call this method to confirm that the current direction established by 
		/// the MoveHandlerDrag method is correct and we need to assign it to their navigations components.
		/// </summary>
		public void MoveHandlerEnd()
		{
			if(tryingToMove)
			{
				MoveHandlerDrag();
				tryingToMove = false;
				AssignMovementTargets();
			}
		}

		public void DirectMove(Vector3 position, Vector3 direction)
		{
			this.destination = position;
			this.direction = direction;
			navigations = new List<Navigation>(Collect<Navigation>());
			if(navigations.Count == 0)
				return;
			formationOffsets = CalcFormationOffsets(direction.normalized);
			for (int i = 0; i < formationOffsets.Count; i++)
				navigations[i].ShowMoveMarkerAt(position + formationOffsets[i]);
				//navigations[i].PrepareToMove(position + formationOffsets[i], direction);
			AssignMovementTargets();
		}

		/// <summary>
		/// After the formationOffsets are stablished this method will assign the final positions
		/// for each navigations component that belongs to the current GameEntities in this group.
		/// </summary>
		private void AssignMovementTargets()
        {
			Vector3 medianMassCenter = CalcMedianPosition(navigations);
			float slowestSpeed = CalcSlowestSpeed(navigations);
			foreach (Navigation nav in navigations)
			{
				Vector3 massCenterOffset = nav.BasePosition - medianMassCenter;
				int bestIndex=0;
				float bestSqrDist = Vector3.SqrMagnitude(massCenterOffset - formationOffsets[bestIndex]);
				for( int i=1;i < formationOffsets.Count; i++)
				{
					float sqrDist = Vector3.SqrMagnitude(massCenterOffset - formationOffsets[i]);
					if( sqrDist < bestSqrDist)
					{
						bestSqrDist = sqrDist;
						bestIndex = i;
					}
				}
				Vector3 selectedOffset = formationOffsets[bestIndex];
				formationOffsets.RemoveAt(bestIndex);
				CommandController commands = nav.GetComponent<CommandController>();

				if (commands!=null)
					commands.Set(new CmdAttackMove(destination + selectedOffset, direction));
				else
				{
					nav.speed = slowestSpeed;
					nav.PrepareToMove(destination + selectedOffset, direction);
					nav.EngageMovement();
				}
			}
		}

		/// <summary>
		/// With the given direction calculates the formation offset
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
        private List<Vector3> CalcFormationOffsets(Vector3 direction)
		{
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, direction);
			List<Vector3> result = new List<Vector3>(navigations.Count);

			if(navigations.Count < maxUnitsPerRow)
			{
				// Linear formation offsets
				AddLineOffsets(ref result, Vector3.zero, navigations.Count);
			}
			else if(navigations.Count < (maxUnitsPerRow * 2))
			{
				int columns = navigations.Count / 2;
				// Double Line formation offsets
				AddLineOffsets(ref result, Vector3.forward * defaultSeparation, columns);
				AddLineOffsets(ref result, -Vector3.forward * defaultSeparation, navigations.Count-columns);
			}
			else
			{
				int rows = Mathf.FloorToInt((float)Math.Sqrt(navigations.Count));
				int cols = navigations.Count / rows;
				int rest = navigations.Count;
				Vector3 centerOffset = Vector3.forward * defaultSeparation * ((rows-1) / 2.0f);
				for(int i=0; i<rows; i++)
				{
					AddLineOffsets(ref result, - centerOffset + Vector3.forward * defaultSeparation * i, cols);
					rest -= cols;
				}
				if(rest > 0)
					AddLineOffsets(ref result, - centerOffset + Vector3.forward * defaultSeparation * rows, rest);
			}

			// Applying the current direction look at.
			for (int i = 0; i < result.Count; i++)
				result[i] = rot * result[i];

			return result;
		}

		private void AddLineOffsets( ref List<Vector3> offsets, Vector3 startingOffset, int count)
		{
				Vector3 centerOffset = Vector3.right * defaultSeparation * ((count-1) / 2.0f);
				// Linear formation offsets
				for (int i = 0; i < count; i++)
					offsets.Add(startingOffset - centerOffset + Vector3.right * defaultSeparation * i);
		}

		/// <summary>
		/// Calcs the center of mass of all the GameEntities that are part of the controlled group.
		/// </summary>
		/// <returns>The median position of center of mass between all the the current controlled group.</returns>
		private Vector3 CalcMedianPosition(IEnumerable<Navigation> navigations)
		{
			Vector3 result = Vector3.zero;
			foreach(Navigation nav in navigations)
			{
				if(nav==null || nav.gameObject == null)
					continue;
				result += nav.BasePosition;
			}
			result /= entities.Count;
			return result;
		}

		private float CalcSlowestSpeed(List<Navigation> navigations)
		{
			if(navigations.Count == 0)
				return 0.0f;
			float result = navigations[0].moveConfig.maxSpeed;
			foreach(Navigation nav in navigations)
			{
				if(nav==null || nav.gameObject == null)
					continue;
				if(result > nav.moveConfig.maxSpeed)
					result = nav.moveConfig.maxSpeed;
			}
			return result;
		}

		#endregion Movement Related

		#region Builders Helpers

		/// <summary>
		/// Changes the rally point position for all selected builders.
		/// </summary>
		/// <param name="newRallyPointPosition">The world coords position for the rally point.</param>
		/// <returns>True if at least one of the entities is a builder.</returns>		
		public bool ChangeBuildersRallyPoint(Vector3 newRallyPointPosition)
		{
			bool result = false;
			foreach( StaticBuilder builder in Collect<StaticBuilder>())
			{
				builder.ChangeRallyPointPosition(newRallyPointPosition);
				result = true;
			}
			return result;
		}

		/// <summary>
		/// Performs the best action available for a group of selectables
		/// </summary>
		/// <param name="targets">The hovered selectables where check if they have available actions.</param>
		public void DoActionOver(IEnumerable<Selectable> targets)
		{
			ResourceWarehouse warehouse;
			Damageable damageable;
			Buildable buildable = GetHoveringComponent<Buildable>(targets);
			if (buildable)
			{
				MobileBuilder builder = GetBestBuilder<MobileBuilder>(buildable);
				if (builder!=null)
				{
					CommandController commands = builder.GetComponent<CommandController>();
					if(commands!=null)
						commands.Set(new CmdBuildStructure(buildable));
					else
						builder.SetupBuild(buildable);
				}
			}
			else if (warehouse = GetHoveringComponent<ResourceWarehouse>(targets))
			{
				ResourceCarrier extractor = GetBestExtractor(warehouse);
				if (extractor != null)
				{
					CommandController commands = extractor.GetComponent<CommandController>();
					if(commands!=null)
						commands.Set(new CmdExtractResource(warehouse));
					else
						extractor.AssignWarehouse(warehouse);
				}
			}
			else if (damageable = GetHoveringComponent<Damageable>(targets))
			{
				foreach( AttackCapable attacker in Collect<AttackCapable>())
				{
					if(damageable.ThisEntity == attacker.ThisEntity)
						continue;
					if( !attacker.IsValidTarget(damageable, 0) )
						continue;

					damageable.ShowTargetedConfirmMarker();
					CommandController commands = attacker.GetComponent<CommandController>();
					if(commands!=null)
						commands.Set(new CmdAttack(damageable, 0));
					else if( attacker.CanUseAttack(0) && attacker.IsInAttackRange(damageable, 0))
						attacker.Attack(damageable, 0);
				}
			}
		}

		public void TryShowActionFeedback(IEnumerable<Selectable> targets)
		{
			Damageable damageable;
			if (damageable = GetHoveringComponent<Damageable>(targets))
			{
				foreach( AttackCapable attacker in Collect<AttackCapable>())
				{
					if(damageable.ThisEntity == attacker.ThisEntity)
						continue;
					if( !attacker.IsValidTarget(damageable, 0) )
						continue;
					damageable.ShowValidTargetMarker();
				}
			}
		}

		/// <summary>
		/// From the list of GameEntities in this group takes the Builder that has no working target
		/// and is able to build the given unit (toBuild)
		/// </summary>
		/// <param name="buildable">The buildable unit to build.</param>
		/// <returns>Returns the best builder that can build the buildable unit.</returns>
		public T GetBestBuilder<T>(Buildable buildable=null) where T : Builder
		{
			T result = null;
			foreach( T builder in Collect<T>())
			{
				if( buildable!=null && !builder.CanBuild(buildable) )
					continue;
				if(result == null)
					result = builder;
				else if( !builder.HasTarget )
					result = builder;
			}
			return result;
		}

		/// <summary>
		/// From the list of GameEntities in this group takes the ResourceCarrier that is able to 
		/// work into the resource warehouse (toWorkIn)
		/// </summary>
		/// <param name="toWorkIn">The ResourceWarehouse unit where to extract or store the resource.</param>
		/// <returns>Returns the best builder that can work on the resource warehouse.</returns>
		public ResourceCarrier GetBestExtractor(ResourceWarehouse toWorkIn)
		{
			ResourceCarrier result = null;
			foreach( ResourceCarrier extractor in Collect<ResourceCarrier>())
			{
				if(extractor.ThisEntity == toWorkIn.ThisEntity)
					continue;
				if( extractor.loadRate == 0.0f )
					continue;
				if (result == null)
					result = extractor;
				else if (!extractor.HasAssignedWarehouse)
					result = extractor;
			}
			return result;
		}

		/// <summary>
		/// Returns the first buildable inside the hoverings Selectable collection.
		/// </summary>
		/// <param name="hoverings">The collection where to find the buildable.</param>
		/// <returns>The first buildable found or null if none is found.</returns>
		private T GetHoveringComponent<T>(IEnumerable<Selectable> hoverings)
		{
			foreach(Selectable selectable in hoverings)
			{
				T result = selectable.GetComponent<T>();
				if(result != null)
					return result;
			}
			return default(T);
		}

		#endregion Builders Helpers

		#region GameEntities CRUD

		/// <summary>
		/// Adds a GameEntity to the controlled group.
		/// </summary>
		/// <param name="gameEntity">GameEntity to be added to the controlled group.</param>
		public void AddShip(GameEntity gameEntity)
		{
			if (!entities.Contains(gameEntity))
				entities.Add(gameEntity);

			this.enabled = entities.Count > 0;
            if(OnShipsChanged!=null)
                OnShipsChanged();
		}

		/// <summary>
		/// Removes the ship from the controlled group. Does nothing if the gameEntity is not part of the group.
		/// </summary>
		/// <param name="gameEntity">GameEntity to be removed from the controlled group.</param>
		public void RemoveShip(GameEntity gameEntity)
		{
			if (entities.Contains(gameEntity))
				entities.Remove(gameEntity);
			
			this.enabled = entities.Count > 0;
            if(OnShipsChanged!=null)
                OnShipsChanged();
		}

		/// <summary>
		/// Removes all current GameEntities from the current controlled group.
		/// </summary>
		public void RemoveAll()
		{
			entities.Clear();

			this.enabled = entities.Count > 0;
            if(OnShipsChanged!=null)
                OnShipsChanged();
		}

		#endregion GameEntites CRUD
	}

}

