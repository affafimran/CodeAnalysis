using GameBase;
using NullPointerCore.CoreSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.Core
{
	public class PlayerUnitsInView : PlayerSystem
	{
		/// <summary>
		/// Interface that must be implemented by any class that wants to listen when an RTSEntity
		/// becomes in and out of the camera view frustrum.
		/// </summary>
		public interface IInViewListener
		{
			void BecomeInsideOfView(GameEntity entity);
			void BecomeOutsideOfView(GameEntity entity);
		}

		// For each registered RTSEntity collects the ones that are within 
		// the camera look at position.
		private HashSet<GameEntity> inViewUnits = new HashSet<GameEntity>();
	
		/// <summary>
		/// The Collection of GameEntity that belongs to this player and are currently
		/// inside of the camera view.
		/// </summary>
		public IEnumerable<GameEntity> InViewEntities { get { return inViewUnits; } }

		private List<IInViewListener> inViewListeners = new List<IInViewListener>();

		// Use this for initialization
		void Start ()
		{
			Camera cam = Camera.main;
			foreach(GameEntity ent in ThisPlayer.GetOwnUnits())
				CheckInViewStatus(ent, IsInsideView(ent, Camera.main));
			ThisPlayer.OnRegistered += OnGameEntityRegistered;
			ThisPlayer.OnUnregistered += OnGameEntityUnregistered;
		}
	
		// Update is called once per frame
		void Update () {
		
		}

		private void OnDestroy()
		{
			ThisPlayer.OnRegistered -= OnGameEntityRegistered;
			ThisPlayer.OnUnregistered -= OnGameEntityUnregistered;
		}

		/// <summary>
		/// Registration method for game entities in order to become controlled by this player.
		/// </summary>
		/// <param name="unitToRegister"></param>
		internal void OnGameEntityRegistered(PlayerControlled unitToRegister)
		{
			CheckInViewStatus(unitToRegister.ThisEntity, IsInsideView(unitToRegister.ThisEntity, Camera.main));
		}

		/// <summary>
		/// Unregister method for game entities in order to become no longer controlled by this player.
		/// </summary>
		/// <param name="unitToUnregister"></param>
		internal void OnGameEntityUnregistered(PlayerControlled unitToUnregister)
		{
			CheckInViewStatus(unitToUnregister.ThisEntity, false);
		}

		/// <summary>
		/// Registration method for game entities in order to become controlled by this player.
		/// </summary>
		/// <param name="unitToRegister"></param>
		internal void RegisterInViewListener(IInViewListener listener)
		{
			inViewListeners.Add(listener);
			foreach( GameEntity ent in InViewEntities )
				listener.BecomeInsideOfView(ent);

		}

		/// <summary>
		/// Unregister method for game entities in order to become no longer controlled by this player.
		/// </summary>
		/// <param name="unitToUnregister"></param>
		internal void UnregisterInViewListener(IInViewListener listener)
		{
			inViewListeners.Remove(listener);
		}

		private bool IsInsideView(GameEntity ent, Camera cam=null)
		{
			if( cam==null)
				cam = Camera.main;
			if( cam==null)
				return false;
			Vector3 screenPoint = cam.WorldToViewportPoint(ent.transform.position);
			bool currOnScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
			return currOnScreen;
		}

		private void CheckInViewStatus(GameEntity ent, bool currentlyOnScreen)
		{
			bool prevOnScreen = inViewUnits.Contains(ent);
			if (!prevOnScreen && currentlyOnScreen)
			{
				inViewUnits.Add(ent);
				foreach (IInViewListener listener in inViewListeners)
					listener.BecomeInsideOfView(ent);
			}
			else if (prevOnScreen && !currentlyOnScreen)
			{
				inViewUnits.Remove(ent);
				foreach (IInViewListener listener in inViewListeners)
					listener.BecomeOutsideOfView(ent);
			}
		}
	}
}