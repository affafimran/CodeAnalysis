using UnityEngine;
using GameBase;
using System;
using NullPointerCore.Extras;

namespace NullPointerGame.NavigationSystem
{
	/// <summary>
	/// GameEntityComponent in control of the unit movement.
	/// </summary>
	public class Navigation : GameEntityComponent
	{
		public const float AngularSleepThreshold = 2.0f;

		private enum MoveOrder
		{
			None,
			PreparingToMove,
			GoToTargetDestination,
			LookAtFinalDirection,
			StoppingAllMovement,
		}

		/// <summary>
		/// Movement configuration for the Navigation component
		/// </summary>
		[System.Serializable]
		public class Data
		{
			/// <summary>
			/// Top speed of the unit.
			/// </summary>
			[Header("Steering")]
			public float maxSpeed = 6f;
			/// <summary>
			/// Rotation speed of the unit.
			/// </summary>
			public float rotSpeed = 80f;
			/// <summary>
			/// Acceleration of the unit
			/// </summary>
		    public float accel = 6f;
			/// <summary>
			/// How great must be the rotation arc when the unit needs to turn the direction.
			/// </summary>
			[Obsolete("Deprecated. Now the Navigation does an internal calculation to fix the best maneuver factor according with the speed and distance to the target.")]
			public float maneuverFactor = 0.1f;
			/// <summary>
			/// pseudo friction applied to the unit when is not accelerated to achieve a slow down effect.
			/// </summary>
			[Range(0.0f,1.0f)]
			public float fullStopFactor = 0.95f;
			/// <summary>
			/// Roll effect factor to be applied when the unit turns (hovercraft turn effect).
			/// Requires a Transform called "roll_body" in the ComponentProxy of the VisualModule.
			/// </summary>
			[Range(0.0f,1.0f)]
			public float rollFactor = 0.8f;
			/// <summary>
			/// Angular speed for roll axis rotation.
			/// </summary>
			[Tooltip("Angular speed for roll axis rotation.")]
			public float rollSpeed = 70.0f;
		}
		[SerializeField]
		public ProxyRef bodyToRoll = new ProxyRef(typeof(Transform), "roll_body");
		/// <summary>
		/// The relative vertical displacement of the owning GameObject.
		/// </summary>
		[Tooltip("The relative vertical displacement of the owning GameObject.")]
		public float baseOffset = 0.0f;
		/// <summary>
		/// Reference to the child GameObject that'll be used as destination feedback marker.
		/// Must contain a MeshRenderer in order to make it visible or hide it when needed.
		/// </summary>
		public GameObject moveTargetMarker;
		/// <summary>
		/// Square radius of the destination area where the destination target can be considered as reached.
		/// Deprecated. Use stoppingDistance instead.
		/// </summary>
		[System.Obsolete("Use stoppingDistance instead.")]
		[Tooltip("Deprecated. Use StoppingDistance instead.")]
		[HideInInspector]
		public float sqrTargetingPrecision = 1.0f;

		/// <summary>
		/// The movement speed that will be used by this navigation. by default, speed
		/// will be the max speed (defined in moveConfig.maxSpeed). but you can set with this a 
		/// slower velocity or a boost for a short time, etc.
		/// </summary>
		public float speed = 10.0f;
		/// <summary>
		/// Movement configuration to be used by this component.
		/// </summary>
		public Data moveConfig;
		/// <summary>
		/// Stop within this distance from the target position.
		/// </summary>
		[Tooltip("Stop within this distance from the target position.")]
		public float stoppingDistance = 1.0f;

		private Transform moveMarkerTR;
		private MeshRenderer moveMarkerMR;
		private Vector3 targetPosition;
		private Vector3 preparePos;
		private Vector3 markerPos;
		private Quaternion targetLookAt;
		private MoveOrder currentMoveOrder = MoveOrder.None;
		private Transform pursuitUnit = null;
		private Vector3 pursuitOffset = Vector3.zero;

		protected Vector3 moveSpeed = Vector3.zero;
		private float lastAccel = 0.0f;
		private bool ignoreFinalLookAt=false;

		public Vector3 BasePosition { get { return transform.position - Vector3.up * baseOffset; } }
		public Vector3 BaseOffset { get { return Vector3.up * baseOffset; } }
		public Vector3 CurrentSpeed { get { return moveSpeed; } }
		public float LastAcceleration { get { return lastAccel; } }
		public Vector3 Destination { get { return targetPosition; } }
		public float RemainingDistance { get { return Vector3.Distance(targetPosition,BasePosition); } }
		/// <summary>
		/// Returns the current used transform body where to apply the roll effect.
		/// </summary>
		protected Transform BodyToRoll { get { return bodyToRoll.Get<Transform>(); } }

		public Action DestinationChanged;
		public Action DestinationReached;
		private Action<bool> onEnded;

		// Use this for initialization
		protected virtual void Start ()
		{
			if(moveTargetMarker)
			{
				moveMarkerTR = moveTargetMarker.GetComponent<Transform>();
				moveMarkerMR = moveTargetMarker.GetComponent<MeshRenderer>();
			}
			speed = moveConfig.maxSpeed;
		}

		/// <summary>
		/// Called when the Visual Module is setted. Here we need to initialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleSetted()
		{
			base.OnVisualModuleSetted();
			bodyToRoll.SafeAssign(ThisEntity);
		}

		/// <summary>
		/// Called when the Visual Module is removed. Here we need to uninitialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleRemoved()
		{
			bodyToRoll.SafeClear();
			base.OnVisualModuleRemoved();
		}

		private void ChangeDestination(Vector3 position)
		{
			targetPosition = position;
			if(DestinationChanged!=null)
				DestinationChanged.Invoke();
			CallOnEndedCallback(false);
		}

		private void CallOnEndedCallback(bool successful)
		{
			if(onEnded!=null)
			{
				Action<bool> temp = onEnded;
				onEnded = delegate { };
				temp.Invoke(successful);
			}
		}

		/// <summary>
		/// Instantly teleports the GameEntity to the given position maintaining the looking at direction.
		/// </summary>
		/// <param name="position">The new position where must be located thee game entity.</param>
		public virtual void Teleport(Vector3 position)
		{
			preparePos = position;
			transform.position = preparePos;
			ChangeDestination(position);
			moveSpeed = Vector3.zero;
			speed = 0.0f;
			OnMoveDestinationReached();
		}

		/// <summary>
		/// Instantly teleports the GameEntity to the given position and looking at the given direction
		/// </summary>
		/// <param name="position">The new position where must be located thee game entity.</param>
		/// <param name="direction">The new direction that must be looking at the game entity.</param>
		public virtual void Teleport(Vector3 position, Vector3 direction)
		{
			preparePos = position;
			if(direction != Vector3.zero)
				targetLookAt = Quaternion.LookRotation(direction.normalized);
			else
				targetLookAt = transform.rotation;
			transform.position = preparePos;
			transform.rotation = targetLookAt;
			ChangeDestination(position);
			moveSpeed = Vector3.zero;
			speed = 0.0f;
			OnMoveDestinationReached();
		}

		/// <summary>
		/// Initialize the movement configuration and show the destination marker at its final position.
		/// </summary>
		/// <param name="position">Final destination of the movement.</param>
		/// <param name="direction">Final look at direction for the unit once the destination is reached.</param>
		public virtual void PrepareToMove(Vector3 position, Vector3 direction)
		{
			preparePos = position;
			pursuitUnit = null;
			ignoreFinalLookAt = direction == Vector3.zero;
			if(!ignoreFinalLookAt)
				targetLookAt = Quaternion.LookRotation(direction.normalized);
			else
				targetLookAt = transform.rotation;

			ShowMoveMarkerAt(preparePos);
		}

		public virtual void PrepareToPursuit(Transform target, Vector3 offset, Vector3 direction)
		{
			if(target == null)
				return;
			pursuitUnit = target;
			pursuitOffset = offset;

			preparePos = target.position+pursuitOffset;
			ignoreFinalLookAt = direction == Vector3.zero;
			if(!ignoreFinalLookAt)
				targetLookAt = Quaternion.LookRotation(direction.normalized);
			else
				targetLookAt = transform.rotation;
		}

		public void ShowMoveMarkerAt(Vector3 pos)
		{
			markerPos = pos;
			if(moveMarkerTR)
				moveMarkerTR.position = markerPos;
			if(moveMarkerMR)
				moveMarkerMR.enabled = true;
		}

		/// <summary>
		/// Confirms the destination setted at PrepareToMove and starts the movement.
		/// </summary>
		/// <param name="onEnded">Callback to be called when the destination is reached or changed.</param>
		public virtual void EngageMovement(Action<bool> onEnded=null)
		{
			if(moveMarkerMR)
				moveMarkerMR.enabled = false;
			currentMoveOrder = MoveOrder.GoToTargetDestination;
			OnMovementOrderEngage(preparePos);
			this.onEnded = onEnded;
		}

		/// <summary>
		/// Stops the current movement at the current position.
		/// </summary>
		public virtual void StopMovement()
		{
			if(moveMarkerMR)
				moveMarkerMR.enabled = false;
			if(currentMoveOrder != MoveOrder.None)
				currentMoveOrder = MoveOrder.StoppingAllMovement;
			pursuitUnit = null;
			pursuitOffset = Vector3.zero;
			OnMovementOrderStop();	
		}

		/// <summary>
		/// Called after a EngageMovement order.
		/// Override to add special behaviours.
		/// </summary>
		/// <param name="destination">The target point in world coordinates where to move the unit.</param>
		protected virtual void OnMovementOrderEngage(Vector3 destination)
		{
			ChangeDestination(destination);
		}

		/// <summary>
		/// Called when a full stop order was given to this navigation.
		/// Override to add special behaviours.
		/// </summary>
		protected virtual void OnMovementOrderStop()
		{
			CallOnEndedCallback(false);
		}

		/// <summary>
		/// Determines whether the movement order is completed because the unit has reached the target position.
		/// </summary>
		/// <returns>True if the target position was reached by this unit.</returns>
		public virtual bool IsFinalDestinationReached()
		{
			return (targetPosition - BasePosition).sqrMagnitude < (stoppingDistance * stoppingDistance);
		}

		// Update is called once per frame
		void Update ()
		{
			lastAccel = 0.0f;
			if (currentMoveOrder == MoveOrder.GoToTargetDestination)
			{
				if(pursuitUnit!=null )
				{
					float offsetSqrDist = Vector3.SqrMagnitude( pursuitUnit.position+pursuitOffset - targetPosition );
					// Has the pursued target change it's position since the last time?
					if( offsetSqrDist > 0.1f )
						targetPosition = pursuitUnit.position+pursuitOffset;
				}
				// I'm already at the location ?
				if (IsFinalDestinationReached())
				{
					// Yes. Target reached! Change move order to finaly look at the desired direction.
					if(ignoreFinalLookAt)
						currentMoveOrder = MoveOrder.StoppingAllMovement;
					else
						currentMoveOrder = MoveOrder.LookAtFinalDirection;
					OnMoveDestinationReached();
				}
				else
					OnMovingToDestination();
			}
			else if (currentMoveOrder == MoveOrder.LookAtFinalDirection)
			{
				Advance(targetLookAt, 0.0f);
				if( DoRoll(targetLookAt) )
				{
					if(moveSpeed.sqrMagnitude > 0)
						currentMoveOrder = MoveOrder.StoppingAllMovement;
					else
						currentMoveOrder = MoveOrder.None;
				}
			}
			else if (currentMoveOrder == MoveOrder.StoppingAllMovement)
			{
				Advance(transform.rotation, 0.0f);
				if( moveSpeed.sqrMagnitude == 0 && DoRoll(transform.rotation) )
					currentMoveOrder = MoveOrder.None;
			}
		}

		public void LateUpdate()
		{
			if(moveMarkerMR && moveMarkerMR.enabled)
				moveMarkerTR.position = markerPos;
		}

		protected virtual void OnMovingToDestination()
		{
			Vector3 totalMoveVector = targetPosition - BasePosition;
			Quaternion targetLookAt = Quaternion.LookRotation(totalMoveVector.normalized);
			Advance(targetLookAt, moveConfig.accel);
			DoRoll(targetLookAt);
		}

		protected virtual void OnMoveDestinationReached()
		{
			if(DestinationReached!=null)
				DestinationReached.Invoke();
			CallOnEndedCallback(true);
		}

		public Vector3 Advance(Quaternion targetLookAt, float accel, bool applyMovement=true)
		{
			if(transform.rotation != targetLookAt)
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetLookAt, moveConfig.rotSpeed * Time.deltaTime);

			float faceDeltaAngle = Quaternion.Angle(transform.rotation, targetLookAt);
			if( faceDeltaAngle < AngularSleepThreshold )
			{
				transform.rotation = targetLookAt;
				faceDeltaAngle = 0.0f;
			}

			float currSpeed = moveSpeed.magnitude;
			Vector3 initSpeed = transform.forward * currSpeed;
			Vector3 finalSpeed = initSpeed;

			if( accel != 0.0f )
			{
				float turnFactor = 1;
				if(faceDeltaAngle != 0.0f)
				{
					float radianFaceFactor = Mathf.Abs(faceDeltaAngle) / 180.0f;
					float targetSqrDist = (targetPosition - transform.position).sqrMagnitude;
					float distFactor = targetSqrDist > 0.0f ? 1 / targetSqrDist : 1.0f;
					turnFactor = turnFactor - radianFaceFactor * currSpeed * distFactor;
				}
				finalSpeed += transform.forward * accel * Time.deltaTime;
				finalSpeed *= turnFactor;
				finalSpeed = Vector3.ClampMagnitude(finalSpeed, speed);
			}
			else
			{
				finalSpeed *= moveConfig.fullStopFactor;
				if (finalSpeed.sqrMagnitude <= Physics.sleepThreshold)
					finalSpeed = Vector3.zero;
			}
			Vector3 moveDistance = (initSpeed + finalSpeed) * 0.5f * Time.deltaTime;
			if(applyMovement)
				transform.position += moveDistance;
			moveSpeed = finalSpeed;
			lastAccel = accel;
			return moveDistance;
		}

		public bool DoRoll(Quaternion lookAt)
		{
			float faceDeltaAngle = Quaternion.Angle(transform.rotation, lookAt);
			float rollDeltaAngle = 0.0f;

			if (BodyToRoll && moveConfig.rollFactor != 0.0f)
			{
				//if(faceDeltaAngle > 0)
				//	faceDeltaAngle = Mathf.MoveTowards(faceDeltaAngle, 90 * moveConfig.rollFactor, 6.0f );
				float faceRotSign = Vector3.Dot(transform.right, lookAt * Vector3.forward) >= 0 ? -1.0f : 1.0f;
				Quaternion rollAngle = Quaternion.AngleAxis(faceRotSign * moveConfig.rollFactor * faceDeltaAngle * 0.5f, Vector3.forward);
				BodyToRoll.localRotation = Quaternion.RotateTowards(BodyToRoll.localRotation, rollAngle, moveConfig.rollSpeed * Time.deltaTime);

				rollDeltaAngle = Quaternion.Angle(BodyToRoll.localRotation, Quaternion.identity);
			}
			if(faceDeltaAngle < 1 && rollDeltaAngle < 1.0f)
			{
				if(BodyToRoll)
					BodyToRoll.localRotation = Quaternion.identity;
				return true;
			}
			return false;
		}

		public void OnDrawGizmosSelected()
		{
			if( Application.isPlaying )
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(transform.position, targetPosition);
				GizmosExt.DrawWireCircle(targetPosition, stoppingDistance);
			}
		}
	}
}