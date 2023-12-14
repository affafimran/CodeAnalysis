using GameBase;
using NullPointerCore.CoreSystem;
using NullPointerGame.Extras;
using SpaceRTS.Core;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 618 // To ignore the warning for the use of the obsolete class RTSPlayer

namespace SpaceRTS.UI
{
	/// <summary>
	/// Controls all the InGame HUD.
	/// Currently we have only the Construction progress bars to show but the idea is to place here 
	/// all the additional HUD overlays, like the units life when the cursor is over a unit, etc.
	/// </summary>
	public class RTSIngameHUD : MonoBehaviour, PlayerUnitsInView.IInViewListener
	{
		/// <summary>
		/// Reference to the player where to get the player systems
		/// </summary>
		public Player player;
		/// <summary>
		/// ContentPool reference for the progress bar buffer.
		/// </summary>
		public ContentPool buildablesProgress;
		/// <summary>
		/// Max distance to the camera where the progress bar is allowed to be seen.
		/// </summary>
		public float visibilityRange = 100.0f;

		Dictionary<GameEntity, UIUnitInfo> inScreenBuildables = new Dictionary<GameEntity, UIUnitInfo>();

		// Use this for initialization
		void Start ()
		{
			if(player!=null)
			{
				// This is going to be used to know what units of the player are inside of the current camera view.
				PlayerUnitsInView inViewSystem = player.GetSystem<PlayerUnitsInView>();
				if(inViewSystem==null)
					Debug.LogWarning("Error! Requires component of type: PlayerUnitsInView in the Player.");
				else
					inViewSystem.RegisterInViewListener(this);
			}
		}

		private void OnDestroy()
		{
			if(player!=null)
			{
				PlayerUnitsInView inViewSystem = player.GetSystem<PlayerUnitsInView>();
				inViewSystem.UnregisterInViewListener(this);
			}
		}

		private void LateUpdate()
		{
			Vector3 cameraPos = Camera.main.transform.position;
			// Refresh the progress bar screen positions and information.
			foreach(KeyValuePair<GameEntity, UIUnitInfo> pair in inScreenBuildables)
			{
				if(pair.Key == null || pair.Value == null)
					return;

				float dist = Vector3.Distance(pair.Key.transform.position, cameraPos);
				if(dist > visibilityRange)
				{
					gameObject.SetActive(false);
					continue;
				}
				gameObject.SetActive(true);
				float frustumHeight = 2.0f * dist * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
				float camRelativeSize = Screen.height / frustumHeight;
				pair.Value.UpdateInfo(pair.Key, camRelativeSize);
			}
		}

		/// <summary>
		/// Called when a GameEntity becomes inside of the current camera view frustrum.
		/// </summary>
		/// <param name="entity">The entity that is now inside of the camera view frustrum.</param>
		public void BecomeInsideOfView(GameEntity entity)
		{
			//Buildable buildable = entity.GetComponent<Buildable>();
			//if(buildable)
			//	AddInScreenBuildable(buildable);
			AddInScreenBuildable(entity);
		}

		/// <summary>
		/// Called when a RTSEntity becomes outside of the current camera view frustrum.
		/// </summary>
		/// <param name="entity">The entity that is now outside of the camera view frustrum.</param>
		public void BecomeOutsideOfView(GameEntity entity)
		{
			//Buildable buildable = entity.GetComponent<Buildable>();
			//if(buildable)
			//	RemoveInScreenBuildable(buildable);
			RemoveInScreenBuildable(entity);
		}

		private void AddInScreenBuildable(GameEntity ent)
		{
			UIUnitInfo uiInfo = buildablesProgress.Instantiate().GetComponent<UIUnitInfo>();
			//Buildable buildable = ent.GetComponent<Buildable>();
			//uiInfo.buildProgress.FillAmount = buildable.GetProgress();
			inScreenBuildables.Add(ent, uiInfo);
		}

		private void RemoveInScreenBuildable(GameEntity ent)
		{
			UIUnitInfo uiBar = null;
			if( !inScreenBuildables.TryGetValue(ent, out uiBar) )
				return;
			buildablesProgress.Destroy(uiBar.gameObject);
			inScreenBuildables.Remove(ent);
		}
	}
}