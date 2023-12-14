using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NullPointerGame.BuildSystem;
using NullPointerGame.DamageSystem;

namespace SpaceRTS.UI
{
	public class UIUnitInfo : MonoBehaviour
	{
		public UIProgressBar buildProgress;
		public UIProgressBar health;
		

		//private GameEntity trackedEntity;
		private RectTransform rcTransf;
		private Vector2 srcSize;

		public RectTransform RTR { get { if (rcTransf==null) rcTransf = GetComponent<RectTransform>(); return rcTransf; } }

		/// <summary>
		/// Total size of the ProgressBar.
		/// </summary>
		public Vector2 SizeDelta 
		{
			get { return RTR.sizeDelta; }
			set { RTR.sizeDelta = value; }
		}

		private void Start()
		{
			//Component tr = this.gameObject.GetComponent("RectTransform");
			//rcTransf = tr as RectTransform;
			srcSize = SizeDelta;
		}

		public bool ShouldBeVisible()
		{
			return true;
		}

		public void UpdateInfo(GameEntity trackedEntity, float camRelativeSize )
		{
			//float radius_world = builder.UnitRadius;
			//float radius_pixels = radius_world * camRelativeSize;
			transform.position = Camera.main.WorldToScreenPoint(trackedEntity.transform.position);
			//transform.position += Vector3.up * radius_pixels;
			name = "UI_"+trackedEntity.name;
			//if(RTR!=null)
			RTR.sizeDelta = new Vector3(srcSize.x*camRelativeSize, srcSize.y);
			CheckBuildableProgressInfo(trackedEntity);
			CheckHealthInfo(trackedEntity);
		}

		private void CheckBuildableProgressInfo(GameEntity ent)
		{
			Buildable buildable = ent.GetComponent<Buildable>();
			if(buildable==null)
			{
				buildProgress.gameObject.SetActive(false);
				return;
			}
			Builder builder = buildable.Builder;
			if (builder == null || !builder.IsBuilding())
			{
				buildProgress.gameObject.SetActive(false);
				return;
			}
			//SizeDelta = new Vector2(radius_pixels * 2, SizeDelta.y);
			buildProgress.FillAmount = buildable.GetProgress();
			buildProgress.gameObject.SetActive(true);
		}

		private void CheckHealthInfo(GameEntity ent)
		{
			Damageable damageable = ent.GetComponent<Damageable>();
			if(damageable==null)
			{
				health.gameObject.SetActive(false);
				return;
			}
			Selectable selectable = ent.GetComponent<Selectable>();
			if( selectable && !selectable.IsHovered && !selectable.IsHighlighted && !selectable.IsSelected)
			{
				health.gameObject.SetActive(false);
				return;
			}
			health.FillAmount = damageable.HealthFactor;
			health.gameObject.SetActive(true);
		}
	}
}