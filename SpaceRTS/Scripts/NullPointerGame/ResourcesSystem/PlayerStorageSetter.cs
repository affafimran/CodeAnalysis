using NullPointerCore.Backend.ResourceGathering;
using NullPointerCore.CoreSystem;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Manipulates the storage properties in the player according with a list of action entries.
	/// In each entry we can find an action to do to a specific resource type by a specific amount.
	/// Every action entry will be applied in the Start() and removed in the OnDestroy().
	/// The most common use will be store a certain resource amount on the entity creation, resource 
	/// amount that must be removed on the OnDestroy().
	/// </summary>
	public class PlayerStorageSetter : MonoBehaviour
	{
		private PlayerControlled playerControlled = null;
		private Player cachedPlayer = null;

		/// <summary>
		/// Action types to apply in the player storage.
		/// </summary>
		public enum Action
		{
			/// <summary>
			/// Consume a certain amount of resource in the Start(). Stores it again in the OnDestroy().
			/// </summary>
			Consume,
			/// <summary>
			/// Stores a certain amount of resource in the Start(). Consumes it in the OnDestroy().
			/// </summary>
			Store,
			/// <summary>
			/// Expands the resource capacity in Start(). Shrinks in the OnDestroy().
			/// </summary>
			Expand,
		}
		/// <summary>
		/// Action entry configuration.
		/// </summary>
		[System.Serializable]
		public class Entry
		{
			/// <summary>
			/// Action to be applied in the Start()
			/// </summary>
			public Action action;
			/// <summary>
			/// Id of the resource where to apply the action.
			/// </summary>
			public ResourceID resourceID;
			/// <summary>
			/// The Amount of resource where to apply the action.
			/// </summary>
			public float amount;
			/// <summary>
			/// Copy constructor.
			/// </summary>
			/// <param name="copied">source entry</param>
			public Entry(Entry copied)
			{
				action = copied.action;
				resourceID = copied.resourceID;
				amount = copied.amount;
			}		
		}
		
		public List<Entry> entries = new List<Entry>();
		private Dictionary<Entry, StorageBucket> buckets = new Dictionary<Entry, StorageBucket>();
		private bool alreadyAssigned = false;

		void Start ()
		{
			if(cachedPlayer==null)
				cachedPlayer = GetComponent<Player>();
			if(cachedPlayer==null)
			{
				playerControlled = GetComponent<PlayerControlled>();
				if(playerControlled==null)
				{
					Debug.LogError("This component Requires a Player or a PlayerControlled component attached with it.", this);
					return;
				}
				cachedPlayer = playerControlled.Owner;
				playerControlled.OwnerChanged += OnPlayerOwnerChanged;
			}
			AssignAllStorageSettings();
		}

		public void AddStorageInfo(PlayerStorageSetter.Entry newEntry)
		{
			Entry newSetInfo = new Entry(newEntry);
			entries.Add(newSetInfo);

			if(alreadyAssigned && cachedPlayer!=null)
			{
				StorageContainer playerResources = cachedPlayer.GetComponent<StorageContainer>();
				if(playerResources!=null)
					Apply(playerResources, newSetInfo);
			}
		}

		private void OnDestroy()
		{
			RemoveAllStorageSettings();
			if(playerControlled!=null)
				playerControlled.OwnerChanged -= OnPlayerOwnerChanged;
		}

		private void OnPlayerOwnerChanged()
		{
			RemoveAllStorageSettings();
			cachedPlayer = playerControlled.Owner;
			AssignAllStorageSettings();
		}

		private void AssignAllStorageSettings()
		{
			if(cachedPlayer==null)
				return;
			StorageContainer playerResources = cachedPlayer.GetComponent<StorageContainer>();
			if(playerResources==null)
				return;

			alreadyAssigned = true;
			foreach (Entry entry in entries)
				Apply(playerResources, entry);
		}

		private void RemoveAllStorageSettings()
		{
			if(cachedPlayer==null)
				return;
			StorageContainer playerResources = cachedPlayer.GetComponent<StorageContainer>();
			if(playerResources==null)
				return;
			alreadyAssigned = false;
			foreach (Entry info in entries)
				Clear(playerResources, info);
		}

		private void Apply(StorageContainer playerResources, Entry entry)
		{
			Storage storage = playerResources.Get(entry.resourceID);
			if (storage != null)
			{
				if (entry.action == Action.Consume)
					storage.Consume(entry.amount);
				else if (entry.action == Action.Store)
					storage.Store(entry.amount);
				else if (entry.action == Action.Expand)
					buckets.Add(entry, new StorageBucket(storage, entry.amount));
			}
		}

		private void Clear(StorageContainer playerResources, Entry entry)
		{
			Storage storage = playerResources.Get(entry.resourceID);
			if (storage != null)
			{
				if (entry.action == Action.Consume)
					storage.Store(entry.amount);
				else if (entry.action == Action.Store)
					storage.Consume(entry.amount);
				else if (entry.action == Action.Expand)
				{
					buckets[entry].Storage = null;
					buckets.Remove(entry);
				}
			}
		}
	}
}