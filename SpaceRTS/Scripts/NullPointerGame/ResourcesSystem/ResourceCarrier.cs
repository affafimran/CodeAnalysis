using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NullPointerCore.Backend.ResourceGathering;
using NullPointerGame.ParkingSystem;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Treats the game entity as a mobile resource storage capable of loading and unloading 
	/// resources from different warehouses. A Parkable component will be required attached 
	/// along with this component to handle the parking traffic control and a StorageContainer 
	/// where to store the loaded or unloaded resource quantities.
	/// </summary>
	[RequireComponent(typeof(Parkable))]
	[RequireComponent(typeof(StorageContainer))]
	public class ResourceCarrier : StorageController, Parkable.IParkingEvents
	{
		/// <summary>
		/// loading cargo rate for this carrier.
		/// </summary>
		public float loadRate = 1.0f;
		/// <summary>
		/// unloading cargo rate for this carrier.
		/// </summary>
		public float unloadRate = 1.0f;
		/// <summary>
		/// Minimal transfering unit
		/// </summary>
		public float transferUnit = 1.0f;
		/// <summary>
		/// Action triggered once this cargo is parked and the trasfer was started (loading or unloading).
		/// </summary>
		public Action onCargoTransferStarted;
		/// <summary>
		/// Action triggered once this cargo is parked and the trasfer was ended (loading or unloading).
		/// </summary>
		public Action onCargoTransferEnded;

		private Parkable parkable = null;
		//private StorageContainer storages = null;
		private Collector collector = null;
		private ResourceWarehouse warehouse=null;
		private bool isLoading = false;
		private bool isUnloading = false;
		private ResourceID transferedResourceID = null;

		private IEnumerator cargoTransferCycle;
		private IEnumerator currentCargoTransfer;
		private bool quitting=false;

		/// <summary>
		/// The current used parkable component.
		/// </summary>
		public Parkable Parkable { get { return parkable; } }
		/// <summary>
		/// The current used storage container.
		/// </summary>
		//public StorageContainer Storages { get { return storages; } }
		/// <summary>
		/// The current resource collector used for the cargo transfer.
		/// </summary>
		public Collector CurrentCollector { get { return collector; } }
		/// <summary>
		/// The current assigned warehouse
		/// </summary>
		public ResourceWarehouse AssignedWarehouse { get { return warehouse; } }
		/// <summary>
		/// Indicates if a warehouse was assigned as working point for cargo loading or unloading.
		/// </summary>
		public bool HasAssignedWarehouse { get { return warehouse!=null; } }

		public bool IsTransferLoading { get { return isLoading; } }
		public bool IsTransferUnloading { get { return isUnloading; } }
		public ResourceID CurrentResourceTransfer { get { return transferedResourceID; } }

		// Use this for initialization
		public override void Start ()
		{
			//storages = GetComponent<StorageContainer>();
			parkable = GetComponent<Parkable>();
			base.Start();
		}

		public override void OnDestroy()
		{
			if( !quitting && this.warehouse != null && this.warehouse.Parking != null)
				parkable.CancelParkingRequest();
			base.OnDestroy();
		}

		void OnApplicationQuit()
		{
			quitting = true;
		}

		/// <summary>
		/// Assigns the warehouse as a target for the cargo transfer.
		/// </summary>
		/// <param name="target">the target warehouse to be used for the cargo transfer.</param>
		public virtual void AssignWarehouse(ResourceWarehouse target)
		{
			if( target != null && target.Parking == null)
			{
				Debug.LogWarning("Unable to assign warehouse. Doesn't have a proper ParkingSystem assigned.", target);
				return;
			}
			if( this.warehouse != null && this.warehouse.Parking != null)
				parkable.CancelParkingRequest();
			this.warehouse = target;
			if(this.warehouse!=null)
				parkable.RequestParkingSlot(target.Parking, this);
		}
		#region Carrier Info Getters

		/// <summary>
		/// Indicates if at least one of the storages aren't empty
		/// </summary>
		/// <returns>true if there is at least one storage not empty in the StorageContainer.</returns>
		public bool HasAnyCargo()
		{
			foreach (Storage ownStorage in Storages.Storages)
			{
				if( !ownStorage.IsEmpty() )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns an IEnumerable containing the list of resource ids that the carrier is currently
		/// transporting. none will be returned if all the storages are empty.
		/// </summary>
		/// <returns>IEnumerable interface with all the resources ids stored in the carrier associated StorageContainer.</returns>
		public IEnumerable<ResourceID> GetLoadedCargoIDs()
		{
			foreach (Storage ownStorage in Storages.Storages)
			{
				if( !ownStorage.IsEmpty() )
					yield return ownStorage.ResourceID;
			}
		}

		/// <summary>
		/// Indicates if the specified warehouse is capable to receive any resource defined in the StorageContainer.
		/// </summary>
		/// <param name="targetToCheck">The target warehouse to check.</param>
		/// <returns>true if the specified warehouse is capable to receive any of the defined resources.</returns>
		public bool IsAbleToUnloadCargoTo(ResourceWarehouse targetToCheck)
		{
			if(targetToCheck==null)
				return false;
			if( targetToCheck.ThisEntity == ThisEntity )
				return false;
			foreach (Storage ownStorage in Storages.Storages)
			{
				if( targetToCheck.CanInsert(ownStorage.ResourceID) )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Indicates if the specified warehouse is capable to give any resource defined in the StorageContainer.
		/// also check for emptyness if it's indicated.
		/// </summary>
		/// <param name="targetToCheck">The target warehouse to check.</param>
		/// <param name="checkEmptyness">if true means that doesn't take into account the empty storages.</param>
		/// <returns>true if the specified warehouse is capable to give any of the defined resources.</returns>
		public bool IsAbleToLoadCargoFrom(ResourceWarehouse targetToCheck, bool checkEmptyness=false)
		{
			if(targetToCheck==null)
				return false;
			if( targetToCheck.ThisEntity == ThisEntity )
				return false;
			foreach (Storage ownStorage in Storages.Storages)
			{
				if( targetToCheck.CanExtract(ownStorage.ResourceID) && 
					(!checkEmptyness || targetToCheck.HasStored(ownStorage.ResourceID)))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Indicates if the specified warehouse is capable to give or receive any resource defined in the StorageContainer.
		/// </summary>
		/// <param name="targetToCheck">The target warehouse to check.</param>
		/// <returns>true if the specified warehouse is capable to give or receive any of the defined resources.</returns>
		public bool IsAbleToTransferAnyCargo(ResourceWarehouse targetToCheck)
		{
			if(targetToCheck==null)
				return false;
			if( targetToCheck.ThisEntity == ThisEntity )
				return false;
			foreach (Storage ownStorage in Storages.Storages)
			{
				if( targetToCheck.CanInsert(ownStorage.ResourceID) )
					return true;
				else if( targetToCheck.CanExtract(ownStorage.ResourceID) )
					return true;
			}
			return false;
		}
		#endregion Carrier Info Getters

		#region Private Helper Methods

		private void FinishCurrentCargoTransfer()
		{
			if(currentCargoTransfer==null)
				return;
			StopCoroutine(currentCargoTransfer);
			if (onCargoTransferEnded != null)
				onCargoTransferEnded.Invoke();
		}

		private IEnumerator TransferingCargo()
		{
			if(warehouse==null)
				yield break;

			if ( warehouse.Storages == null )
			{
				parkable.CancelParkingRequest();
				Debug.LogWarning("The warehouse isn't properly configured. Has no Storages!");
				yield break;
			}

			foreach (Storage ownStorage in Storages.Storages)
			{
				// The target warehouse has a storage of the same resource type?
				Storage targetStorage = warehouse.Storages.Get(ownStorage.ResourceID);
				if(targetStorage==null)
					continue;

				if( warehouse.CanInsert(ownStorage.ResourceID) )
				{
					if( Collector.CanTransfer(ownStorage, targetStorage, transferUnit) )
					{
						collector = new Collector(targetStorage, unloadRate);
						isUnloading = true;
						transferedResourceID = new ResourceID(ownStorage.ResourceID);
						currentCargoTransfer = CollectCargoFrom(ownStorage, collector);
					}
				}
				else if( warehouse.CanExtract(ownStorage.ResourceID) )
				{
					if( Collector.CanTransfer(targetStorage, ownStorage, transferUnit) )
					{
						collector = new Collector(ownStorage, loadRate);
						isLoading = true;
						transferedResourceID = new ResourceID(ownStorage.ResourceID);
						currentCargoTransfer = CollectCargoFrom(targetStorage, collector);
					}
				}
				if( collector != null && currentCargoTransfer != null )
				{
					collector.TransferBase = transferUnit;
					yield return StartCoroutine(currentCargoTransfer);
					currentCargoTransfer = null;
					collector = null;
					transferedResourceID = null;
					isLoading = false;
					isUnloading = false;
				}
			}
			parkable.CancelParkingRequest();
		}

		IEnumerator CollectCargoFrom(Storage targetStorage, Collector collector)
		{
			if(onCargoTransferStarted!=null)
				onCargoTransferStarted.Invoke();
			while(collector.CanCollectFrom(targetStorage))
			{
				collector.CollectFrom(targetStorage, Time.deltaTime);
				yield return null;
			}
			if(onCargoTransferEnded!=null)
				onCargoTransferEnded.Invoke();
		}
		/// <summary>
		/// Event called when the parking step is entering on the designed slot.
		/// </summary>
		public virtual void OnParkingEnter()
		{
			if(this.warehouse == null)
				return;
			cargoTransferCycle = TransferingCargo();
			StartCoroutine(cargoTransferCycle);
		}

		/// <summary>
		/// Event called when the parking step is leaving the designed slot.
		/// </summary>
		public virtual void OnParkingExit()
		{
			if( this.warehouse == null )
				return;
			if (cargoTransferCycle!=null)
				StopCoroutine(cargoTransferCycle);
			FinishCurrentCargoTransfer();
			this.warehouse = null;
		}

		/// <summary>
		/// Event called when the parking job is ended.
		/// </summary>
		public virtual void OnParkingEnded()
		{
			
		}

		#endregion Private Helper Methods
	}
}