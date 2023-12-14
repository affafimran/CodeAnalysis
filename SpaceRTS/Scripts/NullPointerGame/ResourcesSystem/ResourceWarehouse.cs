using GameBase;
using NullPointerCore.Backend.ResourceGathering;
using NullPointerGame.ParkingSystem;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Acts as resource port or bay transportation ready controller. Has a direct connection
	/// to a ResourceContainer that can be defined locally or in the player game object.
	/// Must be defined the valid operations that can be done in this entity (which resources are
	/// available to be extracted and which ones are available to be inserted).
	/// A parking system must be defined to control the insertion/extraction traffic.
	/// </summary>
	public class ResourceWarehouse : StorageController
	{
		/// <summary>
		/// Parking system that will control the resources transportation traffic.
		/// </summary>
		public ProxyRef parking = new ProxyRef(typeof(Parking), "parking");
		/// <summary>
		/// Mask that indicates which resources are capable to be extracted from this warehouse.
		/// </summary>
		public ResourcesMask extractable = new ResourcesMask();
		/// <summary>
		/// Mask that indicates which resources are capable to be inserted into this warehouse.
		/// </summary>
		public ResourcesMask insertable = new ResourcesMask();

		/// <summary>
		/// Indicates if there is a valid parking traffic control defined for this warehouse.
		/// </summary>
		public bool IsParkingRequired  { get { return !ProxyRef.IsInvalid(parking); } }
		/// <summary>
		/// Returns the current parking system that will be used for the traffic control.
		/// </summary>
		public Parking Parking { get { return parking.Get<Parking>(); } }

		/// <summary>
		/// Initializes some proxy property references.
		/// </summary>
		public override void OnVisualModuleSetted()
		{
			parking.SafeAssign(ThisEntity);
			base.OnVisualModuleSetted();
		}
		/// <summary>
		/// Cleans up some proxy property references.
		/// </summary>
		public override void OnVisualModuleRemoved()
		{
			parking.SafeClear();
			base.OnVisualModuleRemoved();
		}

		public virtual void OnEnable()
		{
			if(Parking!=null)
				Parking.enabled = true;
		}

		public virtual void OnDisable()
		{
			if(Parking!=null)
				Parking.enabled = false;
		}

		static public bool IsValid(ResourceWarehouse value)
		{
			if(value == null || value.gameObject == null || !value.enabled )
				return false;
			if(!Parking.IsValid(value.Parking))
				return false;
			return true;
		}

		/// <summary>
		/// Indicates if there is a storage of the specified resource id and there is not empty.
		/// The storage is requested according with the current storage container source configuration.
		/// </summary>
		/// <param name="id">The resource id for the requested storage validation.</param>
		/// <returns>true in case there is stored some resources quantities of the specified type. false in otherwise.</returns>
		public bool HasStored(ResourceID id)
		{
			if(!IsValid(this))
				return false;
			if(Storages==null)
				return false;
			if( !Storages.Contains(id) )
				return false;
			Storage storage = Storages.Get(id);
			return !storage.IsEmpty();
		}

		/// <summary>
		/// Indicates if there is a storage defined of the specified type and if is available the 
		/// extraction of that resource.
		/// </summary>
		/// <param name="id">The resource id to check.</param>
		/// <returns>true if it's possible to extract the resource; false in otherwise.</returns>
		public bool CanExtract(ResourceID id)
		{
			if(!IsValid(this))
				return false;
			if(Storages==null)
				return false;
			if( !Storages.Contains(id) )
				return false;
			if( !extractable.Contains(id) )
				return false;
			return true;
		}

		/// <summary>
		/// Indicates if there is a storage defined of the specified type and if is available the 
		/// insertion of resource of that type.
		/// </summary>
		/// <param name="id">The resource id to check.</param>
		/// <returns>true if it's possible to insert the resource; false in otherwise.</returns>
		public bool CanInsert(ResourceID id)
		{
			if(!IsValid(this))
				return false;
			if(Storages==null)
				return false;
			if( !Storages.Contains(id) )
				return false;
			if( !insertable.Contains(id) )
				return false;
			return true;
		}

		/// <summary>
		/// Indicates if can be extracted the specified resource by a certain amount.
		/// </summary>
		/// <param name="id">The resource id to check.</param>
		/// <param name="amount">The amount to compare.</param>
		/// <returns>true if the resource is valid to be extracted and there is enough 
		/// amount of that resource stored; false in otherwise.</returns>
		public bool HasAvailableToExtract(ResourceID id, float amount)
		{
			if(!IsValid(this))
				return false;
			if( !CanExtract(id) )
				return false;
			if( Storages.HasEnoughStored(id,amount) )
				return false;
			return true;
		}

		/// <summary>
		/// Indicates if the specified resource amount can be inserted in the storage.
		/// </summary>
		/// <param name="id">The resource id to check.</param>
		/// <param name="amount">The amount required to insert in the storage.</param>
		/// <returns>true if the resource is valid to be extracted and there is enough 
		/// empty space to store that amount (only if overflow is not checked in the storage);
		/// false in otherwise.</returns>
		public bool HasAvailableToInsert(ResourceID id, float amount)
		{
			if(!IsValid(this))
				return false;
			if( !CanExtract(id) )
				return false;
			if( Storages.Get(id).CanStore(amount) )
				return false;
			return true;
		}
	}
}
