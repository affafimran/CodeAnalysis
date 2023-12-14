using GameBase;
using NullPointerGame.ParkingSystem;
using System.Collections.Generic;

namespace SpaceRTS.Core
{
	[System.Obsolete("Use CmdExtractResource instead of this component")]
	public class RTSShipCarrier : GameEntityComponent
	{
		public Parking parking;

		private List<Parkable> storedShips = new List<Parkable>();

		public IEnumerable<Parkable> StoredShips { get { return storedShips; } }

		// Use this for initialization
		void Start ()
		{
			if( parking!=null )
			{
				parking.ParkableDocked += OnParkableDocked;
				parking.ParkingDockAssigned += OnParkableDockAssigned;
				parking.ParkableUndocked += OnParkableUndocked;
			}
		}

		private void OnDestroy()
		{
			if( parking!=null )
			{
				parking.ParkableDocked -= OnParkableDocked;
				parking.ParkableUndocked -= OnParkableUndocked;
			}
		}

		public void Dock(Parkable parkable)
		{
			// Check for invalid parkable
			if(parkable==null)
				return;
			// I can park over me self!
			if(ThisEntity==parkable.ThisEntity)
				return;
			parkable.RequestParkingSlot(parking);

		}

		internal void Undock(Parkable parkable)
		{
			// Check for invalid parkable
			if(parkable==null)
				return;
			parkable.ForceParkingSlot(parking);
		}

		private void OnParkableDockAssigned(Parkable parkable)
		{
			if(parkable==null)
				return;
			if( storedShips.Contains(parkable) )
			{
				parkable.gameObject.SetActive(true);
				parkable.transform.parent = this.transform.parent;
				Parking.Slot parkingSlot = parkable.GetParkingSlot<Parking.Slot>();
				if(parkingSlot!=null)
					parkable.transform.LookAt(parkingSlot.ExitDir);
				parkable.CancelParkingRequest();
				storedShips.Remove(parkable);
			}
		}

		private void OnParkableDocked(Parkable parkable)
		{
			if(parkable==null)
				return;
			if( !storedShips.Contains(parkable) )
			{
				parkable.transform.parent = this.transform;
				parkable.gameObject.SetActive(false);
				parkable.ForceCancelParkingRequest();
				storedShips.Add(parkable);
			}
		}

		private void OnParkableUndocked(Parkable obj)
		{
			//throw new NotImplementedException();
		}
	}
}