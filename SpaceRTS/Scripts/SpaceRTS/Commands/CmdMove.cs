using NullPointerCore.Backend.Commands;
using NullPointerGame.NavigationSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace SpaceRTS.Commands
{
	public class CmdMove : Command<Navigation>
	{
		private Vector3 pos;
		private Vector3 dir;

		public CmdMove(Vector3 position, Vector3 dir)
		{
			this.pos = position;
			this.dir = dir;
		}
		protected override IEnumerator OnStarted()
		{
			base.OnStarted();
			Assert.IsNotNull(ComponentTarget, "The Navigation component is null.");
			
			if(dir==Vector3.zero)
				dir = (pos - ComponentTarget.BasePosition).normalized;
			ComponentTarget.PrepareToMove(pos, dir);
			ComponentTarget.EngageMovement(onReached);
			return null;
		}

		private void onReached(bool succesfull)
		{
			End();
		}

		protected override IEnumerator OnCanceled()
		{
			ComponentTarget.StopMovement();
			yield return base.OnCanceled();
		}
	}
}