using NullPointerCore.Backend.ResourceGathering;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Container for a collection of resource storages.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class StorageContainer : MonoBehaviour
	{
		/// <summary>
		/// Contains the storages initial perameters.
		/// </summary>
		[System.Serializable]
		public class InitParams
		{
			/// <summary>
			/// The resource id of the storage to be created
			/// </summary>
			public int resourceID;
			/// <summary>
			/// Indicates if the storage has allowed to overflow its capacity.
			/// </summary>
			public bool overflow=true;
			/// <summary>
			/// Initial stored quantity.
			/// </summary>
			public float initialyStored=0;
			/// <summary>
			/// The minimum capacity for the storage to be created. zero means infinit capacity.
			/// </summary>
			public float minCapacity=0;
		}
		/// <summary>
		/// The initial configuration for each storage container.
		/// </summary>
		public List<InitParams> initParams = new List<InitParams>();
		 
		/// <summary>
		/// The current list of storages created once in play mode.
		/// </summary>
		private List<Storage> storages = new List<Storage>();

		/// <summary>
		/// The current list of storage buckets created once in play mode.
		/// </summary>
		private List<StorageBucket> buckets = new List<StorageBucket>();

		/// <summary>
		/// Returns an enumeration of each contained storage.
		/// </summary>
		public IEnumerable<Storage> Storages { get { return storages; } }

		/// <summary>
		/// Gets the number of storages in the container.
		/// </summary>
		public int Count { get { return storages.Count; } }

		/// <summary>
		/// Initialize the storages according to the initParams list.
		/// </summary>
		public void Awake()
		{
			Initialize();
		}

		/// <summary>
		/// Creates each storage specified in the initParams list.
		/// </summary>
		private void Initialize()
		{
			foreach (InitParams config in initParams)
			{
				Storage storage = Add(config.resourceID, config.overflow, config.minCapacity);
				if(storage!=null)
				{
					storage.SetEmpty();
					storage.Store(config.initialyStored);
				}
			}
		}

		/// <summary>
		/// Adds a storage with the specified resource id, capacity and overflow params to the container.
		/// The method can fail if an invalid resource id is specified or there is a storage with the same resource id.
		/// </summary>
		/// <param name="id">The resource id of the storage to add.</param>
		/// <param name="allowOverflow">Indicates if overflow is allowed in the storage to add.</param>
		/// <param name="capacity">The capacity of the storage to add (Default Value: 0). </param>
		/// <returns>The created storage if successful; otherwise null.</returns>
		public Storage Add(ResourceID id, bool allowOverflow=true, float capacity=0)
		{
			Storage result = null;
			if(!ResourceID.IsInvalid(id) && !Contains(id))
			{
				result = new Storage(id);
				result.AllowOverflow = allowOverflow;
				if(capacity>0)
				{
					StorageBucket bucket = new StorageBucket(capacity);
					result.AddBucket(bucket);
					buckets.Add(bucket);
				}
				storages.Add(result);
			}
			return result;
		}

		/// <summary>
		/// Removes the specified storage from the container.
		/// </summary>
		/// <param name="storage">The storage to remove from the container.</param>
		public void Remove(Storage storage)
		{
			if(storage==null || ResourceID.IsInvalid(storage.ResourceID) )
				return;
			Storage toRemove = Get(storage.ResourceID);
			if(toRemove!=null)
				storages.Remove(toRemove);
		}

		/// <summary>
		/// Removes the storage with the specified resource id from the container.
		/// </summary>
		/// <param name="id">The resource id of the storage to remove.</param>
		public void Remove(ResourceID id)
		{
			if(ResourceID.IsInvalid(id))
				return;
			Storage toRemove = Get(id);
			if(toRemove!=null)
				storages.Remove(toRemove);
		}

		/// <summary>
		/// Removes all storages from the container.
		/// </summary>
		public void Clear()
		{
			storages.Clear();
		}

		/// <summary>
		/// Gets the storage associated with the specified resource id.
		/// </summary>
		/// <param name="id">The resource id of the storage to get.</param>
		/// <returns>The storage associated with the resource id if its found; otherwise, null.</returns>
		public Storage Get(ResourceID id)
		{
			if(ResourceID.IsInvalid(id))
				return null;
			foreach(Storage storage in storages)
			{
				if(storage.ResourceID == id)
					return storage;
			}
			return null;
		}

		/// <summary>
		/// Determines whether a resource id is in the current storage list.
		/// </summary>
		/// <param name="id">The resource id to locate in the internal list of storages.</param>
		/// <returns>true if <paramref name="id"/> is found in the storage list; otherwise, false.</returns>
		public bool Contains(ResourceID id)
		{
			if(ResourceID.IsInvalid(id))
				return false;
			foreach(Storage storage in storages)
			{
				if(storage.ResourceID == id)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the Storage associated with the specified resource id.
		/// </summary>
		/// <param name="id">The resource id of the storage to get.</param>
		/// <param name="result">When this method returns, contains the Storage
		/// associated with the specified resource id, if the resource id is found;
		/// otherwise, null. This parameter is passed uninitialized.</param>
		/// <returns>true if the storage with the specified resource id is found; otherwisem false.</returns>
		public bool TryGet(ResourceID id, out Storage result)
		{
			result = null;
			if(ResourceID.IsInvalid(id))
				return false;
			foreach(Storage storage in storages)
			{
				if(storage.ResourceID == id)
				{
					result = storage;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Check if the list of resource requirements is met.
		/// </summary>
		/// <param name="resourceRequirement">The list of resource requirements to check off.</param>
		/// <returns>True if all the requirements are met. false in otherwise.</returns>
		public bool HasEnough(List<PlayerStorageSetter.Entry> resourceRequirement)
		{
			foreach(PlayerStorageSetter.Entry entry in resourceRequirement)
			{
				if( entry.action == PlayerStorageSetter.Action.Consume && !HasEnoughStored(entry.resourceID, entry.amount) )
					return false;
				if( entry.action == PlayerStorageSetter.Action.Store && !HasEnoughEmptySpace(entry.resourceID, entry.amount) )
					return false;			}
			return true;
		}

		/// <summary>
		/// Checks if there is enough resource stored of the specific type.
		/// </summary>
		/// <param name="id">id of the resource to check</param>
		/// <param name="amount">amount of resource required.</param>
		/// <returns>true is it has enough amount of stored resources of the given type. False in otherwise.</returns>
		[Obsolete("Use HasEnoughStored() instead.")]
		public bool CanBeConsumed(ResourceID id, float amount)
		{
			return HasEnoughStored(id, amount);
		}

		/// <summary>
		/// Checks if there is enough resource stored of the specific type.
		/// </summary>
		/// <param name="id">id of the resource to check</param>
		/// <param name="amount">amount of resource required.</param>
		/// <returns>true is it has enough amount of stored resources of the given type. False in otherwise.</returns>
		public bool HasEnoughStored(ResourceID id, float amount)
		{
			Storage storage = null;
			if( TryGet(id, out storage) )
				return storage.HasEnoughStored(amount);
			return false;
		}

		/// <summary>
		/// Checks if there is enough empty space to store the given amount of resource.
		/// </summary>
		/// <param name="id">The id of the resource to check off.</param>
		/// <param name="amount">The amount of empty space required.</param>
		/// <returns>True if there is enough empty space of the given resource.</returns>
		[Obsolete("Use HasEnoughEmptySpace() instead.")]
		public bool CanBeStored(ResourceID id, float amount)
		{
			return HasEnoughEmptySpace(id, amount);
		}

		/// <summary>
		/// Checks if there is enough empty space to store the given amount of resource.
		/// </summary>
		/// <param name="id">The id of the resource to check off.</param>
		/// <param name="amount">The amount of empty space required.</param>
		/// <returns>True if there is enough empty space of the given resource.</returns>
		public bool HasEnoughEmptySpace(ResourceID id, float amount)
		{
			Storage storage = null;
			if( TryGet(id, out storage) )
				return storage.HasEnoughEmptySpace(amount);
			return false;
		}

		#region InitParams Methods
		/// <summary>
		/// Returns an enumeration of the resource ids for each configured storage.
		/// </summary>
		public IEnumerable<int> ConfiguredStorages 
		{ 
			get
			{
				foreach(InitParams storage in initParams)
					yield return storage.resourceID;
				yield break;
			}
		}
	
		/// <summary>
		/// Determines if the resource id was initially configured.
		/// </summary>
		/// <param name="resourceID">The resource id</param>
		/// <returns>true if the resource id its present; otherwise, false.</returns>
		public bool HasStorageConfigured(int resourceID)
		{
			foreach(InitParams storage in initParams)
			{
				if(storage.resourceID==resourceID)
					return true;
			}
			return false;
		}
		#endregion InitParams
	}
}
