using SpaceRTS.Core;
using System.Collections.Generic;
using UnityEngine;
using GameBase;
using NullPointerGame.DamageSystem;
using NullPointerGame.Extras;

namespace SpaceRTS.UI
{
	public class RTSHealthHUD : MonoBehaviour, PlayerUnitsInView.IInViewListener
	{
		private RTSIngameHUD ingameHUD;

		/// <summary>
		/// ContentPool reference for the progress bar buffer.
		/// </summary>
		public ContentPool buildablesProgress;

		private Dictionary<Damageable, UIProgressBar> inScreenHealth = new Dictionary<Damageable, UIProgressBar>();

		// Use this for initialization
		void Start ()
		{
			ingameHUD = GetComponentInParent<RTSIngameHUD>();

		}

		void OnEnable()
		{
			if(ingameHUD != null && ingameHUD.player!=null)
			{
				PlayerUnitsInView inViewSystem = ingameHUD.player.GetSystem<PlayerUnitsInView>();
				if(inViewSystem==null)
					Debug.LogWarning("Error! Requires component of type: PlayerUnitsInView in the Player.");
				else
					inViewSystem.RegisterInViewListener(this);
			}
		}

		void OnDisable()
		{
			if(ingameHUD != null && ingameHUD.player!=null)
			{
				PlayerUnitsInView inViewSystem = ingameHUD.player.GetSystem<PlayerUnitsInView>();
				inViewSystem.RegisterInViewListener(this);
			}
		}
	
		// Update is called once per frame
		void Update ()
		{
		
		}

		public void BecomeInsideOfView(GameEntity entity)
		{
			Damageable damagable = entity.GetComponent<Damageable>();
			if(damagable)
				AddInScreenBuildable(damagable);
		}

		public void BecomeOutsideOfView(GameEntity entity)
		{
			Damageable damageable = entity.GetComponent<Damageable>();
			if(damageable)
				RemoveInScreenBuildable(damageable);
		}

		private void AddInScreenBuildable(Damageable buildable)
		{
			UIProgressBar uiBar = buildablesProgress.Instantiate().GetComponent<UIProgressBar>();
			uiBar.FillAmount = buildable.HealthFactor;
			inScreenHealth.Add(buildable, uiBar);
		}

		private void RemoveInScreenBuildable(Damageable buildable)
		{
			UIProgressBar uiBar = null;
			if( !inScreenHealth.TryGetValue(buildable, out uiBar) )
				return;
			buildablesProgress.Destroy(uiBar.gameObject);
			inScreenHealth.Remove(buildable);
		}
	}
}