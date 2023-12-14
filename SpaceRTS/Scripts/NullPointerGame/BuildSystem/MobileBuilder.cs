using NullPointerCore.Backend.Commands;
using NullPointerGame.ParkingSystem;
using UnityEngine;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// Gives the capacity to the entity to move and build units in distant places of the map. 
	/// <p>This component assumes that the entity is a mobile builder, meaning that the user 
	/// must specify a location where to build the unit and will move to that location in 
	/// order to start the build progress.</p>
	/// <p>In order to obtain the location where to build the unit, the component will use 
	/// the BuildHandler displaying to the user a semi-transparent unit representation moving 
	/// it freely in space waiting a confirmation of the place where to build.</p>
	/// </summary>
	[RequireComponent(typeof(Parkable))]
	public class MobileBuilder : Builder, Parkable.IParkingEvents
	{
		private BuildHandler buildLocator;

		/// <summary>
		/// Indicated if the builder must delete the buildable unit when interrupted.
		/// </summary>
		public override bool RemoveOnInterrupt { get { return false; } }

		protected void Start()
		{
			buildLocator = GetSceneSystem<BuildHandler>();
		}

		protected override void OnBuildRequested(UnitConfig toBuild)
		{
			if( buildLocator )
				buildLocator.SetupGhost(toBuild, this);
		}

		//private void OnConstructionConfirmed(UnitConfig toBuild, Vector3 pos, Vector3 dir)
		//{
		//	Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
		//	Buildable buildable = CreateBuildable(toBuild, pos, rotation, transform.parent);

		//	// This a little bit messy, but i need to use my new CommandController to assign
		//	// a command and don't want to use a direct assignment.
		//	// I'll try to find a better and clear method to do this avoiding the crossed references.
		//	CommandController commands = GetComponent<CommandController>();
		//	commands.Set(new CmdBuildStructure(buildable));
		//	SetupBuild(buildable);
		//}

		protected override void OnWorkOverBuildableRequested(Buildable buildable)
		{
			Parkable parkable = GetComponent<Parkable>();
			parkable.RequestParkingSlot(buildable.Parking, this);
		}

		public virtual void OnParkingEnter()
		{
			StartBuildProgress();
		}

		public virtual void OnParkingExit()
		{
			if(IsBuilding())
				CancelCurrentBuild();
		}

		public virtual void OnParkingEnded()
		{
		}

		protected override void OnBuildCompleted()
		{
			Parkable parkable = GetComponent<Parkable>();
			parkable.CancelParkingRequest();
			BuildTarget.ChangeToFinalController();
			base.OnBuildCompleted();
		}
	}
}