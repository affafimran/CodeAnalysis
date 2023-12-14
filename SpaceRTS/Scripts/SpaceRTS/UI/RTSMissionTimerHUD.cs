using SpaceRTS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceRTS.UI
{
	public class RTSMissionTimerHUD : MonoBehaviour
	{
		public RTSGameController gameController;
		public Text counterText;

		private 

		// Use this for initialization
		void Start ()
		{
		
		}
	
		// Update is called once per frame
		void Update ()
		{
			if( gameController != null && counterText != null )
				counterText.text = string.Format("{0:00}:{1:00}", gameController.SecondsLeft / 60, gameController.SecondsLeft % 60);
		}
	}
}