using NullPointerCore.Backend.JobScheduling;
using NullPointerCore.Extras;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.ParkingSystem
{
	/// <summary>
	/// Creates a group of Parking slot around a central point
	/// </summary>
	public class OrbitalParking : Parking, SimpleScheduler.ISlotSelectionCriteria
	{
		/// <summary>
		/// Distance between the slots and its central position.
		/// </summary>
		public float parkDistance = 3f;
		/// <summary>
		/// Distance between the enter and exit slot positions and the parking central position.
		/// </summary>
		public float entranceDistance = 5.4f;
		/// <summary>
		/// Distance between the waiting position for the parkables and the parking central position.
		/// </summary>
		public float waitingDistance = 8f;
		// <summary>
		// tickness of the waiting area
		// </summary>
		//public float waitingRadius = 1f;
		/// <summary>
		/// Separation between each enter slot position and its corresponding exit slot position.
		/// </summary>
		public float inoutSeparation = 1.0f;
		/// <summary>
		/// Maximum assignable slots quantity. must be equal or less than MaxSlots.
		/// </summary>
		public int assignableSlots = 1;
		/// <summary>
		/// Maximum slots quantity to be created around the central point.
		/// </summary>
		public int maxSlots = 5;

		public int MaxValidSlots { get { return Math.Min(assignableSlots, maxSlots); } }
		public float ParkingDistance { get { return parkDistance; } }
		public float EntranceDistance { get { return ParkingDistance+entranceDistance; } }
		public float WaitDistance { get { return EntranceDistance+waitingDistance; } }

		public void Start()
		{
			GenerateParkingSlots();
			jobScheduler.slotSelectionCriteria = this;
		}

		protected void GenerateParkingSlots()
		{
			List<SimpleScheduler.JobSlot> slots = new List<SimpleScheduler.JobSlot>();
			if(maxSlots > 0)
			{
				int step = 360 / maxSlots;
				for( int i=0; i<maxSlots; i++)
				{
					Vector3 slotPos =  GetOrbitalPos(step*i, ParkingDistance);
					Vector3 enterPos = GetOrbitalPos(step*i-inoutSeparation, EntranceDistance);
					Vector3 exitPos = GetOrbitalPos(step*i+inoutSeparation, EntranceDistance);
					slots.Add( new Slot(this.transform, slotPos, enterPos-slotPos, exitPos-slotPos));
				}
			}
			jobScheduler.SetupCustomJobSlots(slots);
		}

		/// <summary>
		/// Gets the best waiting position according to the fromPosition.
		/// </summary>
		/// <param name="fromPosition">actual entity position</param>
		/// <returns>The WaitPoint containing the best position to wait for a slot.</returns>
		public override WaitPoint GetWaitingPoint(Vector3 fromPosition)
		{
			Vector3 entranceDir = (fromPosition - transform.position).normalized;
			Vector3 entrancePos = transform.position + entranceDir * WaitDistance;
			return new WaitPoint(entrancePos, -entranceDir);
		}

		public bool CanJobBeStarted(SimpleJob job)
		{
			return jobScheduler.AssignedSlotsCount < assignableSlots;
		}

		public override bool CanRequestSlot(Vector3 basePosition)
		{
			if(jobScheduler.AssignedSlotsCount >= assignableSlots)
				return false;
			float sqrDistToCenter = (basePosition - transform.position).sqrMagnitude;
			float sqrDistWaiting = WaitDistance * WaitDistance;
			return sqrDistToCenter < sqrDistWaiting;
		}

		public override SimpleScheduler.JobSlot GetBestJobSlotFor(IEnumerable<SimpleScheduler.JobSlot> slots, SimpleJob job)
		{
			Parkable.ParkingJob parkingJob = job as Parkable.ParkingJob;
			Vector3 parkablePosition = parkingJob.Parkable.Nav.BasePosition;

			Slot bestSlot = null;
			float bestSqrDist = 0.0f;
			foreach(Slot slot in slots)
			{
				float sqrDist = (slot.EnterPos - parkablePosition).sqrMagnitude;
				if(bestSlot==null || sqrDist < bestSqrDist )
				{
					bestSlot = slot;
					bestSqrDist = sqrDist;
				}
			}
			return bestSlot;
		}

		static Vector3 GetOrbitalPos(float angle, float distance)
		{
			return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * distance;
		}

		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.white;
			GizmosExt.DrawWireCircle(transform.position, WaitDistance);
			GizmosExt.DrawWireCircle(transform.position, WaitDistance);
			if(maxSlots > 0)
			{
				int step = 360 / maxSlots;
				for( int i=0; i<maxSlots; i++)
				{
					Vector3 slotPos = transform.rotation *  GetOrbitalPos(step*i, ParkingDistance);
					Vector3 enterPos = transform.rotation * GetOrbitalPos(step*i-inoutSeparation, EntranceDistance);
					Vector3 exitPos = transform.rotation * GetOrbitalPos(step*i+inoutSeparation, EntranceDistance);

					Gizmos.color = Color.yellow;
					GizmosExt.DrawWireCircle(transform.position+slotPos, 0.5f);
					Gizmos.color = Color.green;
					Gizmos.DrawLine(transform.position+enterPos, transform.position+slotPos);
					GizmosExt.DrawWireCircle(transform.position+enterPos, 0.4f);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(transform.position+exitPos, transform.position+slotPos);
					GizmosExt.DrawWireCircle(transform.position+exitPos, 0.45f);
				}
			}
		}


	}
}