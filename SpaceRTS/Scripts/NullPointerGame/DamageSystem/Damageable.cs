using GameBase;
using System.Collections;
using UnityEngine;
using System;

namespace NullPointerGame.DamageSystem
{
	public class Damageable : GameEntityComponent
	{
		public ProxyRef volume = new ProxyRef(typeof(SphereCollider), "volume");
		public ProxyRef explodeAnimator = new ProxyRef(typeof(Animator), "animator");
		public ProxyRef targetableAnimator = new ProxyRef(typeof(Animator), "targetable_anim");
		public string explodeAnimParam = "Explode";
		public string targetedAnimParam = "Targeted";
		public string targetConfirmAnimParam = "TargetedConfirm";
		public float damageResistance = 100.0f;
		public bool removeOnExplode = true;
		public float removeDelay = 4.0f;

		[SerializeField]
		[Range(0,1)]
		private float current = 1.0f;

		private bool shownAsTargeted = false;
		private Selectable selectable = null;
		public Action UnitDestroyed;

		public float MaxHP { get { return damageResistance; } }
		public float HP { get { return damageResistance * current; } }
		public float HealthFactor { get { return current; } }

		public Vector3 Position { get { return transform.position; } }
		Selectable Selection { get { if(selectable==null) selectable = GetComponent<Selectable>(); return selectable; } }

		public override void OnVisualModuleSetted()
		{
			explodeAnimator.SafeAssign(ThisEntity);
			targetableAnimator.SafeAssign(ThisEntity);
			volume.SafeAssign(ThisEntity);
			base.OnVisualModuleSetted();
		}

		internal Vector3 GetClosestDamagePoint(Vector3 fromPosition)
		{
			if( ProxyRef.IsInvalid(volume) )
				return Vector3.zero;
			// A little trick to target points above of the attacker's plane.
			// (Visually more atractive)
			Vector3 from = fromPosition+Vector3.up * UnityEngine.Random.Range(0.0f, 6.0f);
			return volume.Get<SphereCollider>().ClosestPoint(from)-transform.position;
		}

		public override void OnVisualModuleRemoved()
		{
			explodeAnimator.SafeClear();
			targetableAnimator.SafeClear();
			volume.SafeClear();
			base.OnVisualModuleRemoved();
		}

		private void OnEnable()
		{
			Selection.Unhovered += OnUnhovered;
		}

		private void OnDisable()
		{
			Selection.Unhovered -= OnUnhovered;
		}

		private void OnUnhovered()
		{
			ShowTargetedMarker(false);
		}

		public bool IsAlive()
		{
			return current > 0;
		}

		public void ShowTargetedMarker(bool show)
		{
			if( shownAsTargeted != show &&  !ProxyRef.IsInvalid(targetableAnimator) && !string.IsNullOrEmpty(targetedAnimParam) )
			{
				targetableAnimator.Get<Animator>().SetBool(targetedAnimParam, show);
				shownAsTargeted = show;
			}
		}

		internal float GetRadius()
		{
			if( ProxyRef.IsInvalid(volume) )
				return 0.0f;
			return volume.Get<SphereCollider>().radius;
		}

		public void ShowTargetedConfirmMarker()
		{
			if ( !ProxyRef.IsInvalid(targetableAnimator) && !string.IsNullOrEmpty(targetConfirmAnimParam) )
				targetableAnimator.Get<Animator>().SetTrigger(targetConfirmAnimParam);
		}

		public void ShowValidTargetMarker()
		{
			ShowTargetedMarker(true);
		}

		public void OnDamageReceived(DamageType damage)
		{

		}

		public virtual void ApplyFixedDamage(float fixedAmmount)
		{
			ApplyFactorDamage(fixedAmmount/MaxHP);
		}

		public virtual void ApplyFixedHeal(float fixedHeal)
		{
			ApplyFactorHeal(fixedHeal/MaxHP);
		}

		public virtual void ApplyFactorDamage(float damageFactor)
		{
			if(!IsAlive())
				return;
			current -= damageFactor;
			if(current <= 0)
			{
				current = 0.0f;
				Explode();
			}
		}

		public virtual void ApplyFactorHeal(float healFactor)
		{
			if(!IsAlive())
				return;
			current += healFactor;
			if(current > 1.0f)
				current = 1.0f;
		}

		[ContextMenu("Explode")]
		public virtual void Explode()
		{
			if( !ProxyRef.IsInvalid(explodeAnimator) && !string.IsNullOrEmpty(explodeAnimParam) )
			{
				Animator explodeAnim = explodeAnimator.Get<Animator>();
				explodeAnim.SetTrigger(explodeAnimParam);
			}
			if( UnitDestroyed != null )
				UnitDestroyed.Invoke();
			if(removeOnExplode)
				StartCoroutine(AutoDestroy(removeDelay));
		}

		IEnumerator AutoDestroy(float seconds)
		{
			if(seconds>0)
				yield return new WaitForSeconds(seconds);
			GameObject.Destroy(this.gameObject);
		}


	}
}