using GameBase;
using NullPointerCore.CoreSystem;
using NullPointerGame.BuildSystem;
using NullPointerGame.DamageSystem;
using SpaceRTS.UI;
using UnityEngine;

namespace SpaceRTS.Core
{
	public class RTSUnitHUD : GameEntityComponent
	{
		public GameObject unitHUD;
		public ProxyRef unitVolume = new ProxyRef(typeof(Collider), "volume");
		public float hideDelay = 1.0f;

		private GameObject instancedHUD;
		private ComponentProxy proxy;
		private float radius = 1.0f;

		UIProgressBar healthUI;
		UIProgressBar buildUI;
		CanvasGroup content;

		PlayerControlled playerControlled;
		Damageable damageable;
		Buildable buildable;
		Selectable selectable;
		RectTransform hudRectTransform;
		float hideCountdown;

		// Use this for initialization
		void Start ()
		{
			if( !HasHUDCreated() )
			{
				if( ShouldHaveHUD() )
					CreateHUD();
				else
					return;
			}
			
			hudRectTransform = instancedHUD.GetComponent<RectTransform>();
			proxy = instancedHUD.GetComponent<ComponentProxy>();
			if( proxy == null )
				return;

			content = proxy.GetPropertyValue<CanvasGroup>("content");
			healthUI = proxy.GetPropertyValue<UIProgressBar>("health");
			buildUI = proxy.GetPropertyValue<UIProgressBar>("build");

			playerControlled = GetComponent<PlayerControlled>();
			damageable = GetComponent<Damageable>();
			buildable = GetComponent<Buildable>();
			selectable = GetComponent<Selectable>();
		}

		public override void OnVisualModuleSetted()
		{
			unitVolume.SafeAssign(ThisEntity);
			Collider volume = unitVolume.Get<Collider>();
			if( volume != null )
				radius = volume.bounds.size.x;

			base.OnVisualModuleSetted();
		}

		public override void OnVisualModuleRemoved()
		{
			unitVolume.SafeClear();
			base.OnVisualModuleRemoved();
		}

		private void LateUpdate()
		{
			bool isLocalPlayerOwned = playerControlled == null || playerControlled.Owner == HumanPlayerHandler.GetLocalPlayer();
			bool selectableVisible = selectable==null || (selectable.IsSelected || selectable.IsHighlighted || selectable.IsHovered);
			// Is the unit lossing or gaining health and is owned by the local player? then should show that.
			bool isLocalHealthChanged = isLocalPlayerOwned && healthUI != null && damageable != null && healthUI.FillAmount != damageable.HealthFactor;

			if (selectableVisible || isLocalHealthChanged)
				hideCountdown = hideDelay;
			else if(hideCountdown > 0)
				hideCountdown -= Time.deltaTime;

			bool shouldBeVisible = isLocalHealthChanged || hideCountdown > 0;

			if( content != null )
			{
				 if( shouldBeVisible && content.alpha < 1 )
					 content.alpha = 1;
				 if( !shouldBeVisible && content.alpha > 0 )
					 content.alpha = 0;
			}
			hudRectTransform.sizeDelta = Vector2.one * radius;
			if( healthUI != null )
			{
				if(damageable != null)
					healthUI.FillAmount = damageable.HealthFactor;
				else if(healthUI.gameObject.activeSelf)
					healthUI.gameObject.SetActive(false);
			}

			if ( buildUI != null )
			{
				if( buildable != null)
					buildUI.FillAmount = buildable.GetProgress();
				else if( buildUI.gameObject.activeSelf )
					buildUI.gameObject.SetActive(false);
			}
		}

		public virtual bool ShouldHaveHUD()
		{
			return unitHUD != null;
		}

		void CreateHUD()
		{
			if( !unitHUD.scene.IsValid() ) // It's a prefab? we need to use it to instantiate the real object
				instancedHUD = GameObject.Instantiate(unitHUD, this.transform);
			else // It's already in the scene? we use it directly.
				instancedHUD = unitHUD;
		}

		bool HasHUDCreated()
		{
			return instancedHUD != null;
		}
	}
}