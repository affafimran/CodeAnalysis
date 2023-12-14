using SpaceRTS.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceRTS.UI
{
	public class RTSBuildableButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public GameObject resourceRequirementPanel;
		public Text resourceQtyText;
		public Text crewQtyText;
		public Image img;
		private RTSUnitConfig config;

		public RTSUnitConfig Config 
		{
			get { return config; }
			set
			{
				config = value;
				if(config!=null)
				{
					if(img!=null)
						img.sprite = config.uiImage;
					gameObject.name = config.name + " Item";
				}
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if(resourceRequirementPanel!=null)
				resourceRequirementPanel.SetActive(true);
			if(config!=null)
			{
				float quantity = 0;
				if(resourceQtyText!=null)
				{
					quantity = config.GetResourceRequirementQuantity(0);
					resourceQtyText.text = quantity.ToString();
				}
				if(crewQtyText!=null)
				{
					quantity = config.GetResourceRequirementQuantity(2);
					crewQtyText.text = quantity.ToString();
				}
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if(resourceRequirementPanel!=null)
				resourceRequirementPanel.SetActive(false);
		}

		public void OnSelect(BaseEventData eventData)
		{
			
		}
	}
}