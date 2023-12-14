using GameBase;
using GameBase.RTSKit;
using NullPointerCore.Backend.Commands;
using NullPointerGame.Spatial;
using SpaceRTS.Commands;
using UnityEngine;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// Construction Handler that will let the user choose the next build location.
	/// Generally will be a semitransparent unit (like a ghost).
	/// </summary>
	public class BuildHandler : GameSceneSystem
	{
		/// <summary>
		/// Reference to the scene bounds where to track the current cursor position.
		/// </summary>
		public SceneBounds sceneBounds;
		/// <summary>
		/// Indicates when a point is queried that must be inside the current nav mesh.
		/// </summary>
		public bool useNavMesh = false;
		/// <summary>
		/// Entity that will handle the visual representation of the unit to build in the map.
		/// </summary>
		public GameEntity handlerEntity;

		/// <summary>
		/// Offset of the GameObject in relation to the CursorLookPoint.
		/// </summary>
		public float baseOffset;

		private UnitConfig toBuild = null;
		private Builder builder = null;

		/// <summary>
		/// Has this system something to build?
		/// </summary>
		/// <returns></returns>
		public bool IsGhostRequested() { return toBuild != null; } 

		/// <summary>
		/// Setups the ghost with the unit info to build.
		/// </summary>
		/// <param name="toBuild">The UnitConfig that belongs to the unit to be constructed.</param>
		/// <param name="builder">The builder that invokes the handler and will create the buildable after 
		/// the location confirmation.</param>
		public void SetupGhost(UnitConfig toBuild, Builder builder)
		{
			if(toBuild!=null)
				ClearGhost();

			this.toBuild = toBuild;
			this.builder = builder;
			if(handlerEntity)
			{
				this.handlerEntity.ChangeVisualModule(toBuild.CreateVisual(this.handlerEntity.transform));
				this.handlerEntity.transform.parent = this.transform;
				this.handlerEntity.transform.localPosition = Vector3.zero;
				this.handlerEntity.GetComponent<BuildLocationValidator>().RefreshPosition();
			}
		}

		/// <summary>
		/// Enable or disable the ghost visualization of the current setted unit.
		/// </summary>
		/// <param name="enabled">indicates if the handler must be visible or hidden.</param>
		public void Show(bool enabled)
		{
			if(handlerEntity)
				handlerEntity.gameObject.SetActive(enabled);
		}

		/// <summary>
		/// Confirms the current ghost location as the position where to build the unit. Also calls the previously registered delegate.
		/// </summary>
		public void Confirm()
		{
			BuildLocationValidator blv = handlerEntity.GetComponent<BuildLocationValidator>();
			if(blv && !blv.isAtValidPosition)
			{
				Cancel();
				return;
			}
			Vector3 pos = handlerEntity.transform.position;
			Quaternion rotation = handlerEntity.transform.rotation;
			Buildable buildable = builder.CreateBuildable(toBuild, pos, rotation, builder.transform.parent);

			CommandController commands = builder.GetComponent<CommandController>();
			if(commands!=null)
				commands.Set(new CmdBuildStructure(buildable));
			else
				builder.SetupBuild(buildable);
			ClearGhost();
		}

		/// <summary>
		/// Cancels the ghost
		/// </summary>
		public void Cancel()
		{
			ClearGhost();
		}

		/// <summary>
		/// Clears the ghost, canceling the process.
		/// </summary>
		private void ClearGhost()
		{
			toBuild = null;
			if(handlerEntity && handlerEntity.VisualProxy!=null)
			{
				GameObject toDestroy = handlerEntity.VisualProxy.gameObject;
				handlerEntity.ChangeVisualModule(null);
				GameObject.Destroy(toDestroy);
				//handlerEntity = null;
			}
		}
	}
}