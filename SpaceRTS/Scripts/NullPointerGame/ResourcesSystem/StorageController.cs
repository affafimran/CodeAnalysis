using GameBase;
using NullPointerCore.CoreSystem;
using UnityEngine;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Base class for ResourceCollector and ResourceWarehouse
	/// </summary>
	public abstract class StorageController : GameEntityComponent
	{
		/// <summary>
		/// Information about where the working StorageContainer must be taken.
		/// </summary>
		public enum DestStorageType
		{
			/// <summary>
			/// The StorageContainer must be found attached next to the Player owner of this GameEntity.
			/// </summary>
			PlayerStorage,
			/// <summary>
			/// The StorageContainer must be found attached next to this Component.
			/// </summary>
			LocalStorage,
		}
		/// <summary>
		/// Used by this Component to know where to find the StorageContainer to work with.
		/// </summary>
		public DestStorageType storageSource = DestStorageType.LocalStorage;

		/// <summary>
		/// Cache for the PlayerControlled component.
		/// </summary>
		private PlayerControlled playerControlled;
		/// <summary>
		/// Cache for the current StorageContainer that this component is working with.
		/// </summary>
		private StorageContainer storages;

		/// <summary>
		/// The current cached StorageContainer that this component is working with.
		/// </summary>
		public StorageContainer Storages { get { return storages; } }
		/// <summary>
		/// Returns the cached PlayerControlled of this GameEntity. If not cached already then does it during this call.
		/// </summary>
		protected PlayerControlled PlayerControlled 
		{
			get
			{
				if( playerControlled == null )
					playerControlled = GetComponent<PlayerControlled>();
				return playerControlled;
			}
		}

		/// <summary>
		/// Initializes the StorageContainer cache according with the storageSource configuration.
		/// Also start to listen to any player owner change if the PlayerStorage setting is chosen.
		/// </summary>
		public virtual void Start ()
		{
			if (storageSource == DestStorageType.LocalStorage)
			{
				ChangeStorageSource(GetComponent<StorageContainer>());
				if (storages == null)
					Debug.LogError("A ResourceStorage component attached in this GameObject its required when LocalStorage is setted as source.", this);
			}
			else if (storageSource == DestStorageType.PlayerStorage)
			{
				if (PlayerControlled != null)
				{
					OnPlayerOwnerChanged();
					PlayerControlled.OwnerChanged += OnPlayerOwnerChanged;
				}
			}
		}
	
		/// <summary>
		/// Uninitializes the registered callbacks during Start
		/// </summary>
		public virtual void OnDestroy()
		{
			if(playerControlled!=null)
				playerControlled.OwnerChanged -= OnPlayerOwnerChanged;
		}

		/// <summary>
		/// Called whenever the player owner is changed for this GameEntity.
		/// </summary>
		private void OnPlayerOwnerChanged()
		{
			Player newOwner = PlayerControlled.Owner;
			if(newOwner!=null)
			{
				StorageContainer newContainer = newOwner.GetComponent<StorageContainer>();
				if(newContainer==null)
					Debug.LogWarning("Using PlayerStorage setting but the current player has no StorageContainer."+this);
				ChangeStorageSource(newContainer);
			}
			else
				ChangeStorageSource(null);
		}

		/// <summary>
		/// Assign the new StorageContainer source to work with it.
		/// If overridden, remember to call this base method to continue with the proper cache assignment.
		/// </summary>
		/// <param name="source">The new StorageContainer that we want to work with.</param>
		protected virtual void ChangeStorageSource(StorageContainer source)
		{
			this.storages = source;
		}
	}
}