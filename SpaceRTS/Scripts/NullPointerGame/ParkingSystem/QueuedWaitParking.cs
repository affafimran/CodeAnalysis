using NullPointerCore.Backend.JobScheduling;
using NullPointerCore.Extras;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace NullPointerGame.ParkingSystem
{
	/// <summary>
	/// Special case of Parking traffic controller. There is only one slot to assign but there 
	/// is a ordered wait queue where to wait the slot assignment.
	/// </summary>
	public class QueuedWaitParking : Parking, SimpleScheduler.ISlotSelectionCriteria
	{
		public Vector3 slotPos;
		public Vector3 enterOffset;
		public Vector3 waitDir;
		public Vector3 exitOffset;
		public float unitsSeparation=1.0f;
		public float waitDistance = 4.0f;

		public Vector3 SlotPos { get { return transform.position + transform.rotation * slotPos; } }
		public Vector3 EnterPos { get { return transform.position + transform.rotation * (slotPos + enterOffset); } }
		public Vector3 ExitPos { get { return transform.position + transform.rotation * (slotPos + exitOffset); } }
		public Vector3 EnterDir { get { return transform.rotation * enterOffset.normalized; } }
		public Vector3 ExitDir { get { return transform.rotation * exitOffset.normalized; } }

		public void Start()
		{
			GenerateParkingSlots();
			jobScheduler.slotSelectionCriteria = this;
			jobScheduler.OnWaitingJobsChanged += WaitingJobsChanged;
		}

		public void OnDestroy()
		{
			jobScheduler.slotSelectionCriteria = null;
			jobScheduler.OnWaitingJobsChanged -= WaitingJobsChanged;
		}

		protected void GenerateParkingSlots()
		{
			List<SimpleScheduler.JobSlot> slots = new List<SimpleScheduler.JobSlot>();
			slots.Add( new Slot(this.transform, slotPos, enterOffset, exitOffset));
			jobScheduler.SetupCustomJobSlots(slots);
		}

		public bool CanJobBeStarted(SimpleJob job)
		{
			return jobScheduler.AssignedSlotsCount < 1;
		}

		public override bool CanRequestSlot(Vector3 basePosition)
		{
			if(jobScheduler.AssignedSlotsCount >= 1)
				return false;
			float sqrDistToCenter = (basePosition - EnterPos).sqrMagnitude;
			float sqrDistWaiting = unitsSeparation * 0.5f;
			return sqrDistToCenter < sqrDistWaiting;
		}

		public override void OnParkingDockAssigned(Parkable parkable)
		{
			base.OnParkingDockAssigned(parkable);
			//ReorderWaitQueue();
		}

		private void WaitingJobsChanged()
		{
			ReorderWaitQueue();
		}

		private void ReorderWaitQueue()
		{
			int index = 0;
			foreach (Parkable.ParkingJob parkingJob in jobScheduler.WaitingForSlots)
			{
				parkingJob.Parkable.Nav.PrepareToMove(GetWaitPos(index), -waitDir);
				parkingJob.Parkable.Nav.EngageMovement();
				index++;
			}
		}

		public override SimpleScheduler.JobSlot GetBestJobSlotFor(IEnumerable<SimpleScheduler.JobSlot> slots, SimpleJob parkable)
		{
			return slots.FirstOrDefault();
		}

		public override WaitPoint GetWaitingPoint(Vector3 fromPosition)
		{
			Vector3 lastPoint = GetWaitPos(WaitingCount+2);
			Vector3 entranceDir = (fromPosition - lastPoint).normalized;
			Vector3 entrancePos = lastPoint + transform.rotation * entranceDir * waitDistance;
			return new WaitPoint(entrancePos, -entranceDir);
		}

		public Vector3 GetWaitPos(int index)
		{
			return EnterPos + transform.rotation *(waitDir*unitsSeparation*index);
		}

		public void OnDrawGizmosSelected()
		{
			float rad = unitsSeparation * 0.4f;
			// Draw in yellow the slot position
			Gizmos.color = Color.yellow;
			GizmosExt.DrawWireCircle(SlotPos, rad);
			// Draw in green the enter position
			Gizmos.color = Color.green;
			GizmosExt.DrawWireCircle(EnterPos, rad);
			// Draw in red the exit position
			Gizmos.color = Color.red;
			GizmosExt.DrawWireCircle(ExitPos, rad);
			Gizmos.color = Color.white;
			// Draws the wait pos center and it radius
			GizmosExt.DrawWireCircle(GetWaitPos(2), rad);
			GizmosExt.DrawWireCircle(GetWaitPos(2), waitDistance);
		}
	}
}
