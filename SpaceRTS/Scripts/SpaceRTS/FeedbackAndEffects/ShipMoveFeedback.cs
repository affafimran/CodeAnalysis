using GameBase;
using NullPointerGame.NavigationSystem;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	/// <summary>
	/// Handles the movement feedback for the unit. in this case, the thrusters glowing.
	/// </summary>
	[RequireComponent(typeof(Navigation))]
	public class ShipMoveFeedback : GameEntityComponent
	{
		/// <summary>
		/// Name of the property in the visual module that access the FXThrusters component
		/// </summary>
		//public string thrusterProperty = "thrusters";
		//private FXThrusters thrusters;

		/// <summary>
		/// Proxy Reference to the FXThrusters component that controls the Thrusters effect.
		/// </summary>
		public ProxyRef thrusters = new ProxyRef(typeof(FXThrusters), "thrusters");

		private FXThrusters cachedThrusters = null;
		private Navigation nav;

		// Use this for initialization
		void Start ()
		{
			nav = GetComponent<Navigation>();
		}

		/// <summary>
		/// overrided implementation to configure the component with the VisualModule properties.
		/// </summary>
		public override void OnVisualModuleSetted()
		{
			//thrusters = VisualProxy.GetPropertyValue<FXThrusters>(thrusterProperty);
			thrusters.SafeAssign(ThisEntity);
			cachedThrusters = thrusters.Get<FXThrusters>();
			base.OnVisualModuleSetted();
		}

		/// <summary>
		/// Removes all references to the VisualModule.
		/// </summary>
		public override void OnVisualModuleRemoved()
		{
			//thrusters = null;
			thrusters.SafeClear();
			cachedThrusters = null;
			base.OnVisualModuleRemoved();
		}

		// Update is called once per frame
		void LateUpdate ()
		{
			if(nav == null || cachedThrusters == null)
				return;
			
			cachedThrusters.RefreshCameraLookAt();
			cachedThrusters.RefreshIntensity(nav.CurrentSpeed.magnitude / nav.moveConfig.maxSpeed);
		}
	}
}
