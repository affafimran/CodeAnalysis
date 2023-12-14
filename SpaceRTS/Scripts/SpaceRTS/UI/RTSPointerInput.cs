using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SpaceRTS.UI
{
	public class RTSPointerInput : MonoBehaviour, IPointerClickHandler
	{
		public UnityEvent onUnitSelectEvent;
		public UnityEvent onTargetSelectEvent;

		public void OnPointerClick(PointerEventData eventData)
		{
			if(eventData.button== PointerEventData.InputButton.Left)
				onUnitSelectEvent.Invoke();
			else if(eventData.button== PointerEventData.InputButton.Right)
				onTargetSelectEvent.Invoke();
		}
	}
}