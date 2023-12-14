using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
	/// <summary>
	/// Mobile joystick - Xform Stylee.
	/// This script is based upon the Joystick.cs which can be found in the crossplatform assets.
	/// However, since this is pretty modified we moved it to the Game scripts.
	/// See the settings in the editor per joystick (left/right)
	/// </summary>
	public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption{
			// Options for which axes to use
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}

		public int MovementRange = 100; // Maximum offset
		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

		public bool lerp = true; // [NEW] Should we lerp when dragging/offsetting the joystick
		private float lerpFactor = 0.0f; // [NEW] counter to reach 1 (else we'll never reach the end position of our finger)
		public float lerpMultiplier = 7.5f; // [NEW] Multiplier for faster/slower interpolating

		private bool onTouch = false; // [NEW] Bool to check if we touched/released a joystick
		private bool onJoystickDrag = false; // [NEW] bool to check if we were(!) dragging

		private PointerEventData storedData; // [NEW] Reference to stored PointerEventData

		public float x = 0f; // [NEW] Public x, so we can see and check it in Editor
		public float y = 0f; // [NEW] Public y, so we can see and check it in Editor

		public bool allowDoubleTap = false; // [NEW] Allow double tapping/clicking on the joystick ('similar' to pressing a analog button)
		private bool doubleTap = false; // [NEW] bool to check if we double tapped
		private float tapTimer = 0f; // [NEW] Timer to check if we tapped/touched in time
		public float tapLimit = 0.3f; // [NEW] Maximum allowed time for double tapping.

		Vector3 m_StartPos; // Start position of the joystick (where you initially placed it on your screen/canvas)
		Vector3 m_TouchPos; // [NEW] Position where you touched the joystick! See below
		bool m_UseX; // Toggle for using the x axis
		bool m_UseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

		void OnEnable(){	
			CreateVirtualAxes();
		}

        void Start(){
			SetStartPosition();
        }

		// [NEW] Public function so we can set/override the startposition from somewhere else
		public void SetStartPosition(){
			m_StartPos = transform.position;
		}

		void Update(){
			// If we were dragging and we're NOT touching the joystick lerp back to the START POSITION
			// Lerping back doesn't do anything (yet) actually, but it looks nicer ;)
			if (onJoystickDrag && !onTouch){
				PointerEventData data = new PointerEventData(EventSystem.current);
				m_TouchPos = m_StartPos;
				data.position = m_StartPos;
				OnDrag(data, false);
				if (Vector3.Distance(transform.position, m_StartPos) <= 1f) onReset();
			} else if (onJoystickDrag && onTouch){ // Call drag for lerping
				OnDrag(storedData);
			}

			// Check for double tapping
			if (allowDoubleTap){
				if (tapTimer > 0) tapTimer = Mathf.Max(0f, tapTimer - Time.deltaTime); // Count down tap timer
				if (doubleTap){ // if we indeed double tapped do something!
					// [NEW] [HACK] So this is pretty nasty, but I'll leave this anyway
					// Ideally we would have something nice (in Editor) to call a proper function
					if (!Scripts.hammer.vehicleData.isInVehicle) CameraManager.UpdateSettings("Hammer");
					doubleTap = false; // reset the bool!
				}
			}
		}

		// Nothing [NEW] here
		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = m_TouchPos - value;
			delta.y = -delta.y;
			delta /= MovementRange;
			if (m_UseX){
				x = -delta.x;
				m_HorizontalVirtualAxis.Update(x);
			}

			if (m_UseY){
				y = delta.y;
				m_VerticalVirtualAxis.Update(y);
			}
		}

		void CreateVirtualAxes(){
			// set axes to use
			m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (m_UseX){
				// [NEW] There seems to be a weird issue where the existing axis don't get removed properly when they are registered (again)
				// Here I am forcing them to unregister to avoid that!
				CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);
				m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
			}
			if (m_UseY)	{
				// [NEW] There seems to be a weird issue where the existing axis don't get removed properly when they are registered (again)
				// Here I am forcing them to unregister to avoid that!
				CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
				m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
			}
		}


		public void OnDrag(PointerEventData data){OnDrag(data, true);}
		public void OnDrag(PointerEventData data, bool updateAxis)
		{
			Vector3 newPos = Vector3.zero;
			Vector3 currentPos = transform.position;

			lerpFactor = Math.Min(1f, lerpFactor+(Time.deltaTime * lerpMultiplier)); // Count the lerpfactor up so we reach 1 and thus the actual position of the touch/mouseposition.
			storedData = data; // Store the data for future (lerp) use

			if (m_UseX){
				int delta = (int)(data.position.x - m_TouchPos.x); // Calc difference
				if (lerp) newPos.x = Mathf.Lerp((currentPos.x - m_TouchPos.x), delta, lerpFactor); // Lerp
				else newPos.x = delta; // Don't lerp
			}

			if (m_UseY){
				int delta = (int)(data.position.y - m_TouchPos.y); // Calc difference
				if (lerp) newPos.y = Mathf.Lerp((currentPos.y - m_TouchPos.y), delta, lerpFactor);  // Lerp
				else newPos.y = delta;  // Don't lerp
			}

			// Clamp so we have a circular joystick instead of the original square
			transform.position = Vector3.ClampMagnitude(new Vector3(newPos.x, newPos.y, newPos.z), MovementRange) + m_TouchPos; 
			if(updateAxis) UpdateVirtualAxes(transform.position);
		}

		// [NEW] We need to track some stuff when the pointer is down on this joystick
		public void OnPointerDown(PointerEventData data){
			// PIETER. Start dragging the moment you put your finger down.
			// You need instant interaction!
			m_TouchPos = data.position; // Update the touchposition. This is the position we start dragging from. This prevents 
			transform.position = data.position; // update position
			onTouch = true; // we're touching
			onJoystickDrag = true; // we're starting to drag
			lerpFactor = 0f; // reset this so we can properly lerp again
			OnDrag(data); // Drag it

			if (tapTimer > 0) doubleTap = true; // if we call this function again and the tapTimer is above 0 we are actually double tapping!
			tapTimer = tapLimit; // reset the taptimer
		}

		// [NEW] We need to track some stuff when the pointer is up on this joystick
		public void OnPointerUp(PointerEventData data){
			onTouch = false; // We are not touching anymore
			lerpFactor = 0f; // reset the lerpfactor
		}

		// [NEW] We need a quick full reset function
		void onReset(){
			onJoystickDrag = false;
			m_TouchPos = m_StartPos;
			transform.position = m_StartPos;
			UpdateVirtualAxes(m_StartPos);
		}

		void OnDisable(){
			// remove the joysticks from the cross platform input
			if (m_UseX){
				m_HorizontalVirtualAxis.Remove();
			}
			if (m_UseY){
				m_VerticalVirtualAxis.Remove();
			}
		}
	}
}