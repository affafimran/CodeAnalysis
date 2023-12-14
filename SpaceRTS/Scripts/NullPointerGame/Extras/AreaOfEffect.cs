using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	[RequireComponent(typeof(Collider))]
	public class AreaOfEffect : MonoBehaviour
	{
		public EffectData effect;
		private List<GameEntity> affectedEntities = new List<GameEntity>();
		private List<GameEntity> runningOnEntities = new List<GameEntity>();


		private void OnValidate()
		{
			if( !GetComponent<Collider>().isTrigger )
			{
				Debug.LogWarning("AreaOfEffect requires that the associated collider have is isTrigger as true. Fixing!", this.gameObject);
				GetComponent<Collider>().isTrigger = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other != null && effect != null)
			{
				GameEntity gameEntity = other.gameObject.GetComponentInParent<GameEntity>();
				if (gameEntity != null)
					OnEffectStart(gameEntity);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other != null && effect != null)
			{
				GameEntity gameEntity = other.gameObject.GetComponentInParent<GameEntity>();
				if (gameEntity != null)
					OnEffectEnd(gameEntity);
			}
		}

		protected virtual void OnEffectStart(GameEntity entity)
		{
			affectedEntities.Add(entity); 
			if( !runningOnEntities.Contains(entity) )
				StartCoroutine(RunEffect(entity));
		}

		protected virtual  void OnEffectEnd(GameEntity entity)
		{
			affectedEntities.Remove(entity);
		}

		public bool Contains(GameEntity entity)
		{
			return affectedEntities.Contains(entity);
		}

		IEnumerator RunEffect(GameEntity entity)
		{
			runningOnEntities.Add(entity);
			yield return ApplyEffectUpdate(entity);
			runningOnEntities.Remove(entity);
		}

		protected virtual IEnumerator ApplyEffectUpdate(GameEntity entity)
		{
			yield return effect.ApplyEffect(this, entity);
		}
	}
}