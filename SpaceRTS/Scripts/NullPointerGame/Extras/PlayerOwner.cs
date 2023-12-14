using NullPointerCore.CoreSystem;
using UnityEngine;

namespace NullPointerGame.Extras
{
	/// <summary>
	/// Helper class to handle the existance of a Player owner for certain entity.
	/// Just is in charge of the dirty job ;)
	/// </summary>
	public class PlayerOwner
	{
		/// <summary>
		/// Events interface to handle all the player owner changes
		/// </summary>
		public interface Events
		{
			void OnOwnerRemoved();
			void OnOwnerAssigned();
		}
		private Player cachedPlayer = null;
		private PlayerControlled cachedPlayerControlled = null;
		private Events currentEventHandler = null;

		/// <summary>
		/// The current Player owner.
		/// </summary>
		public Player Player { get { return cachedPlayer; } }

		/// <summary>
		/// Indicates if the PlayerOwner is valid and has a valid player owner.
		/// </summary>
		/// <param name="owner">The PlayerOwner class to test.</param>
		/// <returns>True in case owner is not null and owner.Player is not null; false in otherwise.</returns>
		public static bool HasValidOwner(PlayerOwner owner)
		{
			return owner != null && owner.Player != null;
		}

		/// <summary>
		/// PlayerOwner constructor. Finds the Player through the findAt component. must have a 
		/// Player sibling component or a PlayerControlled component.
		/// </summary>
		/// <param name="findAt">Starting point to find the Player component.</param>
		/// <param name="eventHandler">The object that will receive the owner changed events.</param>
		public PlayerOwner(MonoBehaviour findAt, Events eventHandler)
		{
			Init(findAt, eventHandler);
		}

		/// <summary>
		/// Finds the Player through the findAt component. must have a 
		/// Player sibling component or a PlayerControlled component.
		/// </summary>
		/// <param name="findAt">Starting point to find the Player component.</param>
		/// <param name="eventHandler">The object that will receive the owner changed events.</param>
		public void Init(MonoBehaviour findAt, Events eventHandler)
		{
			if(findAt==null)
				return;
			currentEventHandler = eventHandler;

			if(cachedPlayer==null)
				cachedPlayer = findAt.GetComponent<Player>();
			if(cachedPlayer==null)
			{
				cachedPlayerControlled = findAt.GetComponent<PlayerControlled>();
				if(cachedPlayerControlled==null)
				{
					Debug.LogError("This component Requires a Player or a PlayerControlled component attached with it.", findAt);
					return;
				}
				cachedPlayer = cachedPlayerControlled.Owner;
				if(cachedPlayer!=null)
					OnPlayerOwnerChanged();
				cachedPlayerControlled.OwnerChanged += OnPlayerControlledEvent;
			}
			else
				OnPlayerOwnerChanged();
		}
		/// <summary>
		/// Clears Player, PlayerControlled and event handler references
		/// </summary>
		public void Clear()
		{
			currentEventHandler = null;
			cachedPlayer = null;
			cachedPlayerControlled = null;
		}

		private void OnPlayerControlledEvent()
		{
			if(cachedPlayerControlled==null)
				return;
			if( cachedPlayerControlled.Owner == cachedPlayer )
				return;
			OnPlayerOwnerChanged();

		}
		private void OnPlayerOwnerChanged()
		{
			if(cachedPlayer!=null && currentEventHandler!=null)
				currentEventHandler.OnOwnerRemoved();
			cachedPlayer = cachedPlayerControlled.Owner;
			if(cachedPlayer!=null && currentEventHandler!=null)
				currentEventHandler.OnOwnerAssigned();
		}
	}
}
