using NullPointerCore.Backend.ResourceGathering;
using UnityEngine;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Automatic resource collector. Requires a StorageContainer where to store the collected quantities.
	/// </summary>
	public class ResourceCollector : StorageController
	{
		/// <summary>
		/// id of the resource to collect. Must be defined a storage of the same type in 
		/// the StorageContainer.
		/// </summary>
		public ResourceID resourceID;
		/// <summary>
		/// Collect Speed. (collect amount per second)
		/// </summary>
		public float collectRate = 0.5f;
		/// <summary>
		/// Base transfer value. (minimum quantity to transfer)
		/// </summary>
		public float transferBase = 1.0f;
		/// <summary>
		/// indicates that the collection must start as soon as the component starts.
		/// </summary>
		public bool initOnStart = true;

		private bool extractionStarted=false;
		private Collector collector;

		/// <summary>
		/// Id of the resource to collect and store.
		/// </summary>
		public ResourceID ResourceID { get { return resourceID; } }
		/// <summary>
		/// Current stored quantity.
		/// </summary>
		public float Stored { get { return collector!=null?collector.Storage.Stored: 0; } }

		public override void Start()
		{
			base.Start();
			if (initOnStart)
				StartExtraction();
		}

		public void Update()
		{
			if (extractionStarted)
				TickExtraction(Time.deltaTime);
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if( Application.isEditor && !Application.isPlaying )
			{
				if(Storages!=null && Storages.HasStorageConfigured(resourceID))
					Debug.LogWarning("The configured Resource ID doesn't match with the ones configured in the StorageContainer.");
			} 
		}

		protected override void ChangeStorageSource(StorageContainer newSource)
		{
			base.ChangeStorageSource(newSource);

			if(Storages != null)
			{
				Storage storageSource = Storages.Get(resourceID);
				if(storageSource!=null)
				{
					collector = new Collector(storageSource, collectRate);
					collector.TransferBase = transferBase;
				}
				else
					collector=null;
			}
		}

		/// <summary>
		/// Starts the configured resource extraction.
		/// </summary>
		public void StartExtraction()
		{
			extractionStarted = true;
		}

		/// <summary>
		/// Ends the configured resource extraction.
		/// </summary>
		public void EndExtraction()
		{
			extractionStarted = false;
		}

		/// <summary>
		/// Updates the resource extraction acording to the elapsed time.
		/// </summary>
		/// <param name="deltaTime">elapsed time since the last call.</param>
		public void TickExtraction(float deltaTime)
		{
			if(collector==null)
				return;
			collector.Collect(deltaTime);
		}
	}
}
