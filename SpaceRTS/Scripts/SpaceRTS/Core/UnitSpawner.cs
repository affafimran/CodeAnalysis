using GameBase;
using NullPointerGame.BuildSystem;
using NullPointerGame.ParkingSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.Core
{
	public class UnitSpawner : GameEntityComponent
	{
		public ProxyRef spawnParking = new ProxyRef(typeof(Parking), "spawn_parking");
		public List<UnitConfig> spawnQueue = new List<UnitConfig>();
		[Tooltip("The time at which this spawner will start (in minutes)")]
		public float firstBurstTime = 0.0f;
		[Tooltip("Indicates how many burst will be required after the first one. (zero means infinite)")]
		public int burstRepeatCount = 1;
		[Tooltip("Spawn Interval (in minutes)")]
		public float spawnInterval = 10.0f;
		public delegate void SpawnedEvent(GameEntity spawnedEntity);
		public SpawnedEvent Spawned;

		private float timeForNextSpawn = 0;
		private int burstCount = 0;

		public Parking SpawnParking { get { return spawnParking.Get<Parking>(); } }
		// Use this for initialization
		void Start ()
		{
			if(firstBurstTime==0.0f)
				StartSpawnBurst();
			else
				timeForNextSpawn = firstBurstTime*60;
		}
	
		public override void OnVisualModuleSetted()
		{
			base.OnVisualModuleSetted();
			spawnParking.SafeAssign(ThisEntity);
		}

		public override void OnVisualModuleRemoved()
		{
			spawnParking.SafeClear();
			base.OnVisualModuleRemoved();
		}

		public virtual void Update()
		{
			if(burstRepeatCount<=0 || burstCount<burstRepeatCount )
			{
				timeForNextSpawn -= Time.deltaTime;
				if( timeForNextSpawn <= 0 )
					StartSpawnBurst();
			}
		}

		public void StartSpawnBurst()
		{
			StartCoroutine(SpawnQueued());
		}

		public IEnumerator SpawnQueued()
		{
			burstCount++;
			timeForNextSpawn = spawnInterval * 60;
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			if( SpawnParking == null )
			{
				Debug.LogError("Unable to spawn entity. Spawn Parking not properly configured.", this.gameObject);
				yield break;
			}
				
			foreach(UnitConfig spawnConfig in spawnQueue)
			{
				if(spawnConfig==null)
					continue;
				yield return new WaitUntil( () => (SpawnParking.FreeSlotsCount > 0) );

				GameEntity spawned = spawnConfig.SpawnFinal(ThisEntity, position, rotation);
				Parkable parkable = spawned.GetComponent<Parkable>();
				parkable.ForceParkingSlot(SpawnParking);
				yield return new WaitForSeconds(0.1f);
				parkable.CancelParkingRequest();
				OnSpawned(spawned);
				if( Spawned != null )
					Spawned.Invoke(spawned);
			}
		}

		protected virtual void OnSpawned(GameEntity spawned)
		{
			
		}
	}
}