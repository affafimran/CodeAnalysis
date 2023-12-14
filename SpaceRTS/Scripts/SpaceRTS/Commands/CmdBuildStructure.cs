using System.Collections;
using NullPointerCore.Backend.Commands;
using NullPointerGame.BuildSystem;
using NullPointerGame.ParkingSystem;

namespace SpaceRTS.Commands
{
	public class CmdBuildStructure : Command<MobileBuilder>
	{
		private Buildable buildableTarget = null;
		private Parkable parkable;

		public CmdBuildStructure(Buildable buildable)
		{
			buildableTarget = buildable;
		}

		protected override IEnumerator OnStarted()
		{
			base.OnStarted();
			// We wait one frame before to setup the build
			// This is to give at least one frame between the buildable instantiation and 
			// the setup of the current builder over that target.
			yield return null;
			// One frame later we can properly setup the build target.
			parkable = Context.GetComponent<Parkable>();
			parkable.ParkingEnded += OnParkingEnded;
			ComponentTarget.SetupBuild(buildableTarget);
		}

		private void OnParkingEnded()
		{
			if( CurrentState==State.Cancel || CurrentState==State.Run)
				End();
		}

		protected override IEnumerator OnCanceled()
		{
			parkable.CancelParkingRequest();
			yield return base.OnCanceled();
		}

		protected override IEnumerator OnEnded()
		{
			parkable.ParkingEnded -= OnParkingEnded;
			yield return base.OnEnded();
		}
	}
}

