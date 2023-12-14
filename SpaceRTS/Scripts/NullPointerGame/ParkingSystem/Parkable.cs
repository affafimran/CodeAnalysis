using GameBase;
using NullPointerCore.Backend.JobScheduling;
using NullPointerGame.NavigationSystem;
using System;
using UnityEngine;

namespace NullPointerGame.ParkingSystem
{
	/// <summary>
	/// Component that controls the navigation during a parking sequence agains another
	/// entity. Requires a Parking system in the destination target.
	/// <p>When a paking slot is requested this parkable will move the entity close to 
	/// the target (in the wait area of the parking) and through its Navigation component.</p>
	/// <p>Once reached, internally will request to the parking system a parking slot and will
	/// wait until the the assignment. Then moves to the slot position and informs through the
	/// ParkingSlotEnter action that the position was reached.</p>
	/// <p>A call to CancelParkingRequest() will be needed to release the slot and exit the 
	/// position.</p>
	/// </summary>
	[RequireComponent(typeof(Navigation))]
	public class Parkable : GameEntityComponent, SimpleJob.ISlotChanged
	{
		/// <summary>
		/// Interface to deliver the main events during the parking sequence.
		/// </summary>
		public interface IParkingEvents
		{
			/// <summary>
			/// Called once the parkable reachs the exact slot position.
			/// </summary>
			void OnParkingEnter();
			/// <summary>
			/// Called once the parking is canceled and the parkable is actually at the slot position.
			/// </summary>
			void OnParkingExit();
			/// <summary>
			/// Called once the parkable leaves the parking slot and reachs the exit position.
			/// </summary>
			void OnParkingEnded();
		}
		/// <summary>
		/// class that handles the parking job once a slot is assigned to it.
		/// </summary>
		public class ParkingJob : SimpleJob
		{
			private Parkable parkable;
			public bool forcedSlot=false;
			public Parkable Parkable { get { return parkable; } set { this.parkable = value; } }
			internal ParkingJob() {}
		}

		/// <summary>
		/// Action that is called once the parkable reachs the exact slot position.
		/// </summary>
		public Action ParkingSlotEnter;
		/// <summary>
		/// Action that is called once the parking is canceled and the parkable is
		/// at the slot position.
		/// </summary>
		public Action ParkingSlotExit;
		/// <summary>
		/// Action that is called once the parkable leaves the parking slot and reachs the exit position.
		/// </summary>
		public Action ParkingEnded;

		private Navigation nav = null;
		private ParkingJob parkingJob = null;
		private Parking parking = null;
		private bool isParked = false;
		private IParkingEvents currentParkingControl;

		/// <summary>
		/// Indicates if the parkable is actually parking at the assigned slot.
		/// </summary>
		public bool IsParked { get { return isParked; } }
		/// <summary>
		/// Shortcut and cache to the Navigation component used by this component.
		/// </summary>
		public Navigation Nav 
		{
			get
			{
				if( nav == null )
					nav = GetComponent<Navigation>();
				return nav;
			}
		}
		/// <summary>
		/// Shortcut to a valid cache of the parking job associated with this parkable.
		/// </summary>
		protected ParkingJob ThisParkingJob 
		{
			get
			{
				if ( parkingJob == null )
				{
					parkingJob = new ParkingJob();
					parkingJob.Parkable = this;
					parkingJob.onSlotChanged = this;
				}
				return parkingJob;
			}
		}

		public void Start()
		{
			Nav.DestinationChanged += OnNavDestinationChanged;
		}


		/// <summary>
		/// Request a parking slot at the given parking system.
		/// </summary>
		/// <param name="parking">The parking system where to request a slot.</param>
		/// <param name="parkingControl">Interface that acts as a callback to inform 
		/// the parking events sequence during the parking.</param>
		public void RequestParkingSlot(Parking parking, IParkingEvents parkingControl=null)
		{
			if(parking==null)
				return;
			if(this.parking==parking)
				return;
			if(this.parking!=null)
				CancelParkingRequest();

			this.parking = parking;
			this.ThisParkingJob.forcedSlot = false;
			this.currentParkingControl = parkingControl;

			// We are going to take control of the navigation
			if( parking.CanRequestSlot(Nav.BasePosition) )
				ThisParkingJob.RequestRunSlot(parking.ParkingScheduler);
			else
			{
				Parking.WaitPoint point = parking.GetWaitingPoint(Nav.BasePosition);
				//Debug.Log(" ** Moving to parking wait pos... -> OnWaitPosReach");
				Nav.PrepareToMove(point.Position, point.LookAtDir);
				Nav.EngageMovement(OnWaitPosReach);
			}
		}

		/// <summary>
		/// Request a parking slot no matter the actual position of the entity.
		/// <p>This method is similar to the RequestParkingSlot method but in that case the
		/// Parkable will try to move the entity close to the Parking entity and then the 
		/// slot will be requested. Here the slot will be requested regardless of the actual
		/// position.</p>
		/// </summary>
		/// <param name="parking">The parking system where to request a slot.</param>
		/// <param name="parkingControl">Interface that acts as a callback to inform 
		/// the parking events sequence during the parking.</param>
		public void ForceParkingSlot(Parking parking, IParkingEvents parkingControl=null)
		{
			if(parking==null)
				return;
			if(this.parking==parking)
				return;
			if(this.parking!=null)
				CancelParkingRequest();

			this.parking = parking;
			this.currentParkingControl = parkingControl;

			isParked=true;
			ThisParkingJob.forcedSlot = true;
			ThisParkingJob.ForceSlot(parking.ParkingScheduler);
		}

		/// <summary>
		/// Cancels the current parking request, sends the entity to the exit position if
		/// the actual parkig job was already started.
		/// </summary>
		public void CancelParkingRequest()
		{
			if(this.parking==null)
				return;
			if(ThisParkingJob.HasJobSlotAssigned)
			{
				//Debug.Log(" ** isParked? " + isParked);
				if (isParked)
				{
					isParked = false;
					//Debug.Log(" ** Parking Exit Event." + isParked);
					if (ParkingSlotExit != null)
						ParkingSlotExit.Invoke();
					if (currentParkingControl != null)
						currentParkingControl.OnParkingExit();
					if (parking)
						parking.OnParkableUndocked(this);
					GoToExitPos();
				}
				else
					Nav.StopMovement();
			}
			else if(ThisParkingJob.IsWaitingJobSlot)
			{
				ThisParkingJob.ReleaseRunRequest();
				OnParkingEnded();
			}
			else
				Nav.StopMovement();
		}

		private void GoToExitPos()
		{
			Parking.Slot parkingSlot = GetParkingSlot<Parking.Slot>();
			//Debug.Log(" ** Moving to parking exit pos... -> OnExitPosReach");
			Nav.PrepareToMove(parkingSlot.ExitPos, parkingSlot.ExitDir);
			Nav.EngageMovement(OnExitPosReach);
			//Debug.Log(" ** ReleaseRunRequest()");
			ThisParkingJob.ReleaseRunRequest();
		}

		public void ForceCancelParkingRequest()
		{
			ThisParkingJob.ReleaseRunRequest();
			OnParkingEnded();
		}

		/// <summary>
		/// Called once the total parking job was ended. Can be overrided to extend its behaviour.
		/// </summary>
		protected virtual void OnParkingEnded()
		{
			// cache for the callbacks in case the user change its content during the call.
			IParkingEvents lastCall = this.currentParkingControl;
			Action lastEvent = ParkingEnded;
			// The actual parking clean up.
			this.currentParkingControl = null;
			this.parking=null;

			if(lastCall!=null)
				lastCall.OnParkingEnded();
			if( lastEvent!=null )
				lastEvent.Invoke();
		}

		/// <summary>
		/// Called after a parking slot is requested to the parking system, and that system
		/// assigns a slot to it.
		/// <p>Remember that the slot request goes to a ordered wait queue, so a slot will 
		/// be assigned only if it's the correct turn of the entity.</p>
		/// </summary>
		/// <param name="job">The job provided by this Parkable where to assign the slot.</param>
		/// <param name="slot">The Parking slot assigned by the system.</param>
		public void OnJobSlotAssigned(SimpleJob job, SimpleScheduler.JobSlot slot)
		{
			//Debug.LogWarning("OnJobSlotAssigned()");
			ParkingJob parkingJob = job as ParkingJob;
			Parking.Slot parkingSlot = slot as Parking.Slot;
			if(parking!=null)
				parking.OnParkingDockAssigned(this);
			if(parkingSlot!=null && !parkingJob.forcedSlot)
			{
				//Debug.Log(" ** Moving to parking enter pos... -> OnEnterPosReach");
				Nav.PrepareToMove(parkingSlot.EnterPos, parkingSlot.EnterDir);
				Nav.EngageMovement(OnEnterPosReach);
			}
			parkingJob.forcedSlot = false;
		}

		/// <summary>
		/// Called once the slot is removed from the parking job.
		/// </summary>
		/// <param name="job">The job provided by this Parkable and where the slot was removed.</param>
		public void OnJobSlotRemoved(SimpleJob job)
		{

		}


		private void OnNavDestinationChanged()
		{
			if(isParked)
			{
				//Debug.LogWarning("OnNavDestinationChanged. Canceling Parking Request.");
				CancelParkingRequest();
			}
		}

		private void OnWaitPosReach(bool successful)
		{
			//Debug.LogWarning("OnWaitPosReach() Successful? " + successful);
			if(successful && Parking.IsValid(parking) && ThisParkingJob != null )
				ThisParkingJob.RequestRunSlot(parking.ParkingScheduler);
			else
				OnParkingEnded();
		}

		private void OnEnterPosReach(bool successful)
		{
			//Debug.LogWarning("OnEnterPosReach() Successful? " + successful);
			if(successful)
			{
				Parking.Slot parkingSlot = GetParkingSlot<Parking.Slot>();
				if(parkingSlot!=null)
				{
					//Debug.Log(" ** Moving to parking slot... -> OnSlotPosReach");
					Nav.PrepareToMove(parkingSlot.SlotPos, parkingSlot.EnterDir);
					Nav.EngageMovement(OnSlotPosReach);
				}
			}
			else
			{
				ThisParkingJob.ReleaseRunRequest();
				OnParkingEnded();
			}
		}

		private void OnSlotPosReach(bool successful)
		{
			//Debug.LogWarning("OnSlotPosReach() Successful? " + successful);
			if(successful)
			{
				isParked=true;
				// Slot position reached
				if(ParkingSlotEnter!=null)
					ParkingSlotEnter.Invoke();
				if(currentParkingControl!=null)
					currentParkingControl.OnParkingEnter();
				if(parking)
					parking.OnParkableDocked(this);
			}
			else
			{
				ThisParkingJob.ReleaseRunRequest();
				OnParkingEnded();
			}
		}

		private void OnExitPosReach(bool successful)
		{
			//Debug.LogWarning("OnExitPosReach() Successful? " + successful);
			//parkingJob.ReleaseRunRequest();
			OnParkingEnded();
		}

		/// <summary>
		/// Converts the current assigned slot to the type passed as T in the method.
		/// </summary>
		/// <typeparam name="T">Type where to be casted the parking slot.</typeparam>
		/// <returns>The casted current slot.</returns>
		public T GetParkingSlot<T>() where T : SimpleScheduler.JobSlot
		{
			return ThisParkingJob.Slot as T;
		}
	}
}