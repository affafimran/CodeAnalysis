using System;
using UnityEngine;

namespace NullPointerGame.Extras
{
	public class WaitUntilActionTriggered : CustomYieldInstruction
	{
		bool triggered = false;
		private Action actionRef = null;

		public override bool keepWaiting 
		{
			get
			{
				if(triggered)
					actionRef -= Listener;
				return triggered;
			}
		}

		public WaitUntilActionTriggered(ref Action action) { actionRef = action; action += Listener; }

		public void Listener()
		{
			triggered = true;
		}
	}
}