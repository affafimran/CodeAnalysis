// Descrition : InitMobileButtons.cs : Use to find which car is manage by the mobile buttons. This script is on each mobile button
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class MobileButtons : MonoBehaviour {

	public 	InitMobileInputs	grpInputs;								// access InitMobileInputs	component.						
	private CarController 		carController;							// access CarController component.
	public bool 				btn_Left;								// if true : button is used for Left control
	public bool 				btn_Right;								// if true : button is used for Rightcontrol
	public bool 				btn_Accelerate;							// if true : button is used for Accelerartion control
	public bool 				btn_Break;								// if true : button is used for Break control
	public bool 				btn_Respawn;							// if true : button is used for Respawn control



	void Start()
	{
		EventTrigger trigger = GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();													// Create event Pointer Down
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
		trigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();													// Create event Pointer Up
		entry2.eventID = EventTriggerType.PointerUp;
		entry2.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
		trigger.triggers.Add(entry2);
	}

	public void OnPointerDownDelegate(PointerEventData data)
	{
		if (carController == null)
			carController = grpInputs.carController;

		if (btn_Left)
			carController.Btn_LeftActivate ();
		if (btn_Right)
			carController.Btn_RightActivate ();
		if (btn_Accelerate)
			carController.Btn_AccelerationActivate ();
		if (btn_Break)
			carController.Btn_BreakActivate ();
		if (btn_Respawn)
			carController.Btn_Respawn ();
		
	}

	public void OnPointerUpDelegate(PointerEventData data)
	{
		if (carController == null)
			carController = grpInputs.carController;

		if (btn_Left)
			carController.Btn_LeftDeactivate ();
		if (btn_Right)
			carController.Btn_RightDeactivate();
		if (btn_Accelerate)
			carController.Btn_AccelerationDeactivate ();
		if (btn_Break)
			carController.Btn_BreakDeactivate ();
	}
}
