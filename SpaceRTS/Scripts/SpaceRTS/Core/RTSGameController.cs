using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SpaceRTS.Core
{
	public class RTSGameController : MonoBehaviour
	{
		public GameObject entityToKeep = null;
		public float timeToSuccess = 10.0f;
		public UnityEvent MissionComplete; 
		public UnityEvent MissionFailed;
		public List<MonoBehaviour> activateOnMissionStart = new List<MonoBehaviour>();

		private bool gameEnded = false;
		private float secondsLeft = 0;
		private bool started = false;
		
		public int SecondsLeft { get { return Mathf.CeilToInt(secondsLeft); } }

		private void Start()
		{
			secondsLeft = timeToSuccess * 60.0f;
		}

		// Update is called once per frame
		void Update ()
		{
			if( !started || gameEnded )
				return;
			secondsLeft -= Time.deltaTime;
			if( secondsLeft <= 0 && !gameEnded )
			{
				gameEnded = true;
				secondsLeft = 0.0f;
				OnMisssionComplete();
			}
			if( entityToKeep == null & !gameEnded )
			{
				gameEnded = true;
				OnMissionFailed();
			}
		}

		public void StartMission()
		{
			started = true;
			foreach( MonoBehaviour comp in activateOnMissionStart )
			{
				if( comp != null )
				{
					comp.enabled = true;
				}
			}
		}

		protected virtual void OnMisssionComplete()
		{
			if( MissionComplete != null )
				MissionComplete.Invoke();
		}

		protected virtual void OnMissionFailed()
		{
			if( MissionFailed != null )
				MissionFailed.Invoke();
		}

		public void ExitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		public void ReplayGame()
		{
			SceneManager.LoadScene(this.gameObject.scene.name);
		}
	}
}