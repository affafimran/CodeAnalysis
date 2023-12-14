using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceRTS.UI
{
	public class RTSMessageDisplay : MonoBehaviour
	{
		public GameObject mainPanel;
		public Text textCtrl;
		public float defaultDuration=4.0f;

		private Queue<Msg> msgQueue = new Queue<Msg>();
		private IEnumerator loopRoutine = null;

		static RTSMessageDisplay main = null;

		class Msg
		{
			public string text;
			public float duration;
			public Msg(string txt, float time) {text =txt; duration=time; }
		}

		// Use this for initialization
		void Start ()
		{
			if (mainPanel && mainPanel.activeSelf)
				mainPanel.SetActive(false);
			if(main==null)
				main = this;
		}

		public void EnqueueMessage(string msg, float duration=0)
		{
			msgQueue.Enqueue(new Msg(msg, duration==0?defaultDuration:duration));
			if( loopRoutine==null )
			{
				loopRoutine = DisplayNextMessage();
				StartCoroutine(loopRoutine);
			}
		}

		public static void Show(string msg, float duration=0)
		{
			if(main==null)
			{
				Debug.LogWarning("Requires the proper setup of the RTSMessageDisplay UI Component.");
				return;
			}
			main.EnqueueMessage(msg, duration);
		}

		IEnumerator DisplayNextMessage()
		{
			while( msgQueue.Count > 0 )
			{
				yield return new WaitForSeconds(0.2f);
				if(mainPanel)
					mainPanel.SetActive(true);
				Msg msg = msgQueue.Dequeue();
				if(textCtrl)
					textCtrl.text = msg.text;
				yield return new WaitForSeconds(msg.duration);
				if (mainPanel)
					mainPanel.SetActive(false);
			}
			loopRoutine = null;
		}
	}
}