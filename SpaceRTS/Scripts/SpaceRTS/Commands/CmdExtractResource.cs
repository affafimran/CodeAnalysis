using NullPointerCore.Backend.Commands;
using NullPointerGame.Extras;
using NullPointerGame.ParkingSystem;
using NullPointerGame.ResourceSystem;
using System.Collections;
using UnityEngine;

namespace SpaceRTS.Commands
{
	public class CmdExtractResource : Command<ResourceCarrier>
	{
		private ResourceWarehouse warehouseTarget = null;

		private ResourceWarehouse loadingAt = null;
		private Parkable parkable;

		public CmdExtractResource(ResourceWarehouse target)
		{
			warehouseTarget = target;
		}

		protected override IEnumerator OnStarted()
		{
			base.OnStarted();
			parkable = Context.GetComponent<Parkable>();
			parkable.ParkingEnded += OnParkingEnded;

			if ( ComponentTarget.IsAbleToLoadCargoFrom(warehouseTarget) )
				loadingAt = warehouseTarget;

			if (loadingAt!=null)
				ComponentTarget.AssignWarehouse(warehouseTarget);
			else
				End();
			return null;
		}

		protected override IEnumerator OnCanceled()
		{
			parkable.CancelParkingRequest();
			parkable.ParkingEnded -= OnParkingEnded;
			yield return new WaitUntilActionTriggered(ref parkable.ParkingEnded);
			yield return base.OnCanceled();
		}

		protected override IEnumerator OnEnded()
		{
			yield return null;
			parkable.ParkingEnded -= OnParkingEnded;
			yield return base.OnEnded();
		}

		private void OnParkingEnded()
		{
			ResourceWarehouse newTarget = FindNewDestination();
			if(ResourceWarehouse.IsValid(newTarget))
				ComponentTarget.AssignWarehouse(newTarget);
			else
				End();
		}

		private ResourceWarehouse FindNewDestination()
		{
			ResourceWarehouse result = null;

			if(ComponentTarget.HasAnyCargo())
				result = RTSUtilities.GetNearestPlayerStorageWarehouse(ComponentTarget);
			else if(loadingAt != null && ComponentTarget.IsAbleToLoadCargoFrom(loadingAt, true) )
				result = loadingAt;

			return result;
		}
	}
}