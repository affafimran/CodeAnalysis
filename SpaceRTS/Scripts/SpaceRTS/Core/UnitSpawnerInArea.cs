using GameBase;
using NullPointerCore.Extras;
using NullPointerGame.BuildSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.Core
{
	public class UnitSpawnerInArea : GameEntityComponent
	{
		[System.Serializable]
		public struct SpawnInfo
		{
			public UnitConfig spawnType;
			public int count;
			public float delay;
		}
		public List<SpawnInfo> spawnQueue = new List<SpawnInfo>();
		public float spawnRadius = 5.0f;

		// Use this for initialization
		void Start ()
		{
			StartCoroutine(SpawnQueued());
		}

		public IEnumerator SpawnQueued()
		{
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
				
			foreach(SpawnInfo spawnConfig in spawnQueue)
			{
				if(spawnConfig.spawnType==null)
					continue;

				if(spawnConfig.delay > 0.0f)
					yield return new WaitForSeconds(spawnConfig.delay);

				for(int i=0; i<spawnConfig.count; i++)
				{
					Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
					Vector3 finalPos = transform.position;
					finalPos.x += randomPoint.x;
					finalPos.z += randomPoint.y;
					spawnConfig.spawnType.SpawnFinal(ThisEntity, finalPos, rotation);
				}
			}
		}

		public void OnDrawGizmosSelected()
		{
			GizmosExt.DrawWireCircle(transform.position, spawnRadius);
		}
	}
}
