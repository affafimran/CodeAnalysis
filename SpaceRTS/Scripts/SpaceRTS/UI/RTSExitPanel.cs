using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceRTS.UI
{
	public class RTSExitPanel : MonoBehaviour
	{
		public KeyCode exitKey = KeyCode.Escape;
		public GameObject exitPanel;
		// Use this for initialization
		void Start ()
		{
		
		}
	
		// Update is called once per frame
		void Update ()
		{
			if(Input.GetKeyUp(exitKey) && exitPanel!=null)
				exitPanel.SetActive(!exitPanel.activeSelf);
		}

		public void OnExitRequest()
		{
			#if UNITY_EDITOR
			if( Application.isEditor )
				UnityEditor.EditorApplication.isPlaying = false;
			#endif
			Application.Quit();
		}
	}
}