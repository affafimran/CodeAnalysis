using NullPointerCore.Backend.JobScheduling;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NullPointerGame.ParkingSystem
{
	/// <summary>
	/// The system that controls the traffic around a group of parking slots.
	/// </summary>
	public abstract class Parking : MonoBehaviour
	{
		/// <summary>
		/// The base class for the parking slots.
		/// </summary>
		public class Slot : SimpleScheduler.JobSlot
		{
			private Transform trCenter;
			private Vector3 slotPos;
			private Vector3 enterOffset;
			private Vector3 exitOffset;

			public Slot(Transform center, Vector3 slotPos, Vector3 enterOffset, Vector3 exitOffset)
			{
				this.trCenter = center;
				this.slotPos = slotPos;
				this.enterOffset = enterOffset;
				this.exitOffset = exitOffset;
			}
			public Transform CentralTransform { get { return trCenter; } }
			public Vector3 SlotPos { get { return trCenter.position + trCenter.rotation * slotPos; } }
			public Vector3 EnterPos { get { return trCenter.position + trCenter.rotation * (slotPos+enterOffset); } }
			public Vector3 ExitPos { get { return trCenter.position + trCenter.rotation * (slotPos+exitOffset); } }
			public Vector3 EnterDir { get { return (SlotPos-EnterPos).normalized; } }
			public Vector3 ExitDir { get { return (ExitPos-SlotPos).normalized; } }

			protected override void OnJobAssigned()
			{
			}
			protected override void OnJobRemoved()
			{
			}
		}

		public struct WaitPoint
		{
			private Vector3 position;
			private Vector3 lookAtDir;

			public Vector3 Position { get {return position; } }
			public Vector3 LookAtDir { get { return lookAtDir; } }

			internal WaitPoint(Vector3 pos, Vector3 dir)
			{
				this.position = pos;
				this.lookAtDir = dir;
			}
		}

		public Action<Parkable> ParkingDockAssigned;
		public Action<Parkable> ParkableDocked;
		public Action<Parkable> ParkableUndocked;

		protected bool valid = false;
		protected SimpleScheduler jobScheduler = new SimpleScheduler();

		internal SimpleScheduler ParkingScheduler { get { return jobScheduler; } }
		public abstract WaitPoint GetWaitingPoint(Vector3 fromPosition);

		public virtual SimpleScheduler.JobSlot GetBestJobSlotFor(IEnumerable<SimpleScheduler.JobSlot> slots, SimpleJob parkable)
		{
			return slots.GetEnumerator().Current as SimpleScheduler.JobSlot;
		}

		public IEnumerable<Parkable> WaitingParkables 
		{
			get
			{
				foreach( SimpleJob job in jobScheduler.WaitingForSlots )
					yield return (job as Parkable.ParkingJob).Parkable;
				yield break;
			}
		}

		public IEnumerable<Parkable> AssignedParkables 
		{
			get
			{
				if(jobScheduler==null)
					yield break;
				foreach( KeyValuePair<SimpleJob, SimpleScheduler.JobSlot> assignments in jobScheduler.Assigned )
					yield return (assignments.Key as Parkable.ParkingJob).Parkable;
				yield break;
			}
		}

		protected virtual void OnEnable()
		{
			valid = true;
		}

		protected virtual void OnDisable()
		{
			valid = false;
			CancelAllParkingRequests();
		}

		static public bool IsValid(Parking value)
		{
			return value!=null && value.gameObject != null && value.enabled && value.valid;
		}

		public void CancelAllParkingRequests()
		{
			List<Parkable> toCancelParking = new List<Parkable>();
			toCancelParking.AddRange(WaitingParkables);
			toCancelParking.AddRange(AssignedParkables);
			foreach( Parkable parkable in toCancelParking )
			{
				parkable.CancelParkingRequest();
				parkable.Nav.StopMovement();
			}
		}

		public int WaitingCount { get { return jobScheduler.WaitingJobsCount; } }
		public int FreeSlotsCount { get { return jobScheduler.FreeSlotsCount; } }

		public virtual bool CanRequestSlot(Vector3 basePosition)
		{
			return true;
		}

		public virtual void OnParkingDockAssigned(Parkable parkable)
		{
			if(ParkingDockAssigned!=null)
				ParkingDockAssigned.Invoke(parkable);
		}

		public virtual void OnParkableDocked(Parkable parkable)
		{
			if(ParkableDocked!=null)
				ParkableDocked.Invoke(parkable);
		}

		public virtual void OnParkableUndocked(Parkable parkable)
		{
			if(ParkableUndocked!=null)
				ParkableUndocked.Invoke(parkable);
		}
	}
}