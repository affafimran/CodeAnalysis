using GameBase;
using NullPointerCore.Backend;
using NullPointerCore.CoreSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.BuildSystem
{
	/// <summary>
	/// GameEntityComponent that gives to the GameEntity the property of been able to build buildable entities.
	/// </summary>
	public abstract class Builder : GameEntityComponent
	{
		/// <summary>
		/// Struct info for each build item in the queue
		/// </summary>
		protected struct BuildItem
		{
			public UnitConfig toBuild;
			public Vector3 pos;
			public Vector3 dir;
			public BuildItem(UnitConfig config, Vector3 pos, Vector3 dir)
			{
				this.toBuild = config;
				this.pos = pos;
				this.dir = dir;
			}
		}
		/// <summary>
		/// The temporal prefab to use as gameplay controller for the constructed unit, until its finished.
		/// </summary>
		public Buildable constructPrefab;
		/// <summary>
		/// List of Units that this one is capable of build.
		/// </summary>
		public List<UnitConfig> buildables = new List<UnitConfig>();

		/// <summary>
		/// Means how many buildPoints will be built per second according with the Buildable.buildPoints.
		/// </summary>
		public float buildRate = 1.0f;
		/// <summary>
		/// This is the radius of the builder. This shouldn't be here, because it's not builder related,
		/// but for now, its the only place where is used it. Probably will be changed in the future.
		/// </summary>
		[SerializeField]
		private float radius=2.0f;

		private Buildable target;
		private bool started = false;

		/// <summary>
		/// Build queue holding the list of units to build.
		/// </summary>
		protected List<BuildItem> buildQueue = new List<BuildItem>();

		/// <summary>
		/// has a buildable target assigned?
		/// </summary>
		public bool HasTarget { get { return target != null; } }
		/// <summary>
		/// Buildable target asigned to this builder. can be null if there is no buildable assigned.
		/// </summary>
		public Buildable BuildTarget { get { return target; } }

		/// <summary>
		/// Indicated if the builder must delete the buildable unit when interrupted.
		/// </summary>
		public abstract bool RemoveOnInterrupt { get; }
		/// <summary>
		/// Offset from the Builder position to the rally point.
		/// Deprecated. Use BuildExpelLocation instead.
		/// </summary>
		[System.Obsolete("Use BuildExpelLocation instead.")]
		public Vector3 RallyPointOffset { get { return Vector3.zero; } }
		/// <summary>
		/// Means the radius of this builder unit. Its used to calculate distance between the
		/// builder and the buildable.<br />
		/// Probably it's not the proper place to have this property, maybe in a more spatialy
		/// related component. For now, its here because its the only one that its realy using it.
		/// </summary>
		public float UnitRadius { get { return radius; } set { radius = value; } }
		/// <summary>
		/// Indicates the current quantity of units to build in the queue.
		/// </summary>
		public int BuildsInQueue { get { return buildQueue.Count; } }
		/// <summary>
		/// Triggered each time the internal build queue changes.
		/// </summary>
		public Action BuildQueueChanged;
		/// <summary>
		/// Triggered each time the build progress starts by any builder.
		/// </summary>
		public Action BuildStarted;
		/// <summary>
		/// Triggered after each build progress is completed.
		/// </summary>
		public Action BuildCompleted;
		/// <summary>
		/// Triggered when the builder its about to build but one of the conditions has failed.
		/// </summary>
		public Action UnableToBuild;
		/// <summary>
		/// Triggered after each build progress is canceled.
		/// </summary>
		public Action BuildCanceled;

		private Conditionals<Buildable> buildConditionals = new Conditionals<Buildable>();
		/// <summary>
		/// A list of conditionals to check if its possible to start to build the buildable passed
		/// as parameter. only when all of this conditionals returns true then the construction will
		/// start.
		/// </summary>
		public Conditionals<Buildable> BuildConditionals { get { return buildConditionals; } }

		private IEnumerator buildProgress = null;

		private bool isBuilding = false;
		/// <summary>
		/// Returns true if this builder is actually working over a buildable.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsBuilding() { return isBuilding; }
		/// <summary>
		/// Returns the transform that must be used as parent for the instantiated buildable.
		/// Must be overiden to change its behaviour, will use the builder's transform parent 
		/// as default transform to be used as parent of the buildable.
		/// </summary>
		/// <returns>The transform that must be used as parent for the instantiated buildable.</returns>
		protected virtual Transform GetBuildableParent()
		{
			return this.transform.parent;
		}

		/// <summary>
		/// Enumerates the units to build in queue.
		/// </summary>
		public IEnumerable<UnitConfig> QueuedUnits 
		{
			get
			{
				foreach(BuildItem item in buildQueue)
					yield return item.toBuild;
				yield break;
			}
		}

		/// <summary>
		/// Request the 'toBuild' unit to be built. 
		/// </summary>
		/// <param name="toBuild">The unit configuration to be built.</param>
		public void Build(UnitConfig toBuild)
		{
			if(toBuild==null)
				return;
			if(!buildables.Contains(toBuild))
				Debug.LogWarning("This unit "+toBuild.name + " is not part of the buildable list.");
			OnBuildRequested(toBuild);
		}

		protected abstract void OnBuildRequested(UnitConfig toBuild);

		/// <summary>
		/// Adds a Unit of type UnitConfig to the build queue.
		/// </summary>
		/// <param name="toBuild">Unit tye to be builded.</param>
		/// <param name="location">location where be built the unit.</param>
		/// <param name="dir">Face direction that will have the builded unit once completed.</param>
		public void AddUnitToBuild(UnitConfig toBuild, Vector3 location, Vector3 dir)
		{
			buildQueue.Add(new BuildItem(toBuild, location, dir));
			if(!started)
				CheckNextBuild();
			if(BuildQueueChanged!=null)
				BuildQueueChanged.Invoke();
			//Messages.Dispatch(new BuildQueueChanged(this));
		}

		/// <summary>
		/// If there's a item in the build queue then start its construction.
		/// </summary>
		private void CheckNextBuild()
		{
			if( buildQueue.Count > 0 )
			{
				started = true;
				BuildItem current = buildQueue[0];
				// Prepares the rotation for the next builded object.
				Quaternion rotation = current.dir != Vector3.zero ? Quaternion.LookRotation(current.dir, Vector3.up) : transform.rotation;
				// The Buildable is the temporal object during its construction (with all the visual properties)
				// that later will be replaced with the real controller.
				Buildable buildable = CreateBuildable(current.toBuild, current.pos, rotation, GetBuildableParent());
				SetupBuild(buildable);
			}
		}

		/// <summary>
		/// Sets the given buildable as as the current working target to build.
		/// </summary>
		/// <param name="buildable">the buildable to be used as target to build.</param>
		public void SetupBuild(Buildable buildable)
		{
			if(target!=null)
				CancelCurrentBuild();
			target = buildable;
			if(target!=null)
				target.SetBuilder(this);

			OnWorkOverBuildableRequested(buildable);
		}

		/// <summary>
		/// overridable method to control how must procceed after a buildable was requested as
		/// next target to build.
		/// </summary>
		/// <param name="buildable">The buildable requested to build.</param>
		protected abstract void OnWorkOverBuildableRequested(Buildable buildable);

		/// <summary>
		/// Starts the build progress for the current buildable target.
		/// </summary>
		protected void StartBuildProgress()
		{
			if(buildProgress==null)
			{
				buildProgress = TickBuildProgress();
				StartCoroutine(buildProgress);
			}
			else
				Debug.LogWarning("Atempting to build when already working.");
		}

		IEnumerator TickBuildProgress()
		{
			if(target==null)
				Debug.LogWarning("Startung Build Progress but target is null.");
			if( !CanBuildBeStarted() && UnableToBuild != null)
				UnableToBuild.Invoke();

			// Can the build be started?
			yield return new WaitUntil( CanBuildBeStarted );

			isBuilding = true;
			if(BuildStarted!=null)
				BuildStarted.Invoke();			

			// Actually starts the build increasing the progress ieach frame
			while(!target.IsConstructionFinished)
			{
				target.ChangeProgress(buildRate * Time.deltaTime);
				yield return null;
			}
			if(BuildCompleted!=null)
				BuildCompleted.Invoke();
			ClearBuildProgress();
			OnBuildCompleted();
			CurrentBuildInterrupted();
		}

		/// <summary>
		/// Indicates if the current build work can be started
		/// </summary>
		/// <returns></returns>
		public virtual bool CanBuildBeStarted()
		{
			if( target==null )
				return false;
			if( !BuildConditionals.Invoke(target) )
				return false;
			return true;
		}

		private void ClearBuildProgress()
		{
			isBuilding = false;
			buildProgress = null;
		}

		/// <summary>
		/// Cancels the current build.
		/// </summary>
		public void CancelCurrentBuild()
		{
			CancelBuildAt(0);
		}

		/// <summary>
		/// Removes the build item from the queue, also cancel the build if index is zero.
		/// </summary>
		/// <param name="index"></param>
		public void CancelBuildAt(int index)
		{
			if(index==0)
			{
				if(RemoveOnInterrupt && target!=null)
					GameObject.Destroy(target.gameObject);
				CancelBuildProgress();
				CurrentBuildInterrupted();
			}
			else
				RemoveQueueItemAt(index);
		}

		protected virtual void OnBuildCompleted()
		{
		}

		private void CurrentBuildInterrupted()
		{
			ClearBuildProgress();
			if (target)
				target.OnBuilderWorkEnded();
			RemoveQueueItemAt(0);
			target = null;
			started = false;
			CheckNextBuild();
		}

		private void RemoveQueueItemAt(int index)
		{
			if (index < 0 || index >= buildQueue.Count)
				return;

			buildQueue.RemoveAt(index);
			if(BuildQueueChanged!=null)
				BuildQueueChanged.Invoke();
			//Messages.Dispatch(new BuildQueueChanged(this));
		}

		private void CancelBuildProgress()
		{
			if(buildProgress!=null)
			{
				StopCoroutine(buildProgress);
				if(BuildCanceled!=null)
					BuildCanceled.Invoke();
			}
		}

		/// <summary>
		/// Indicates if the buildable can be built by this builder.
		/// </summary>
		/// <param name="buildable">The buildable to check.</param>
		/// <returns>true if this Builder is able to build the specified buildable; false in otherwise.</returns>
		public bool CanBuild(Buildable buildable)
		{
			if(buildable==null)
				return false;
			 if( buildables.Contains(buildable.BuildType) )
				return true;
			return false;
		}

		/// <summary>
		/// Creates the Buildable object with the visual module of the final object.
		/// This object should control the build process until it's finished.
		/// </summary>
		/// <param name="toBuild">The config info of the object to build.</param>
		/// <param name="location">The world coords of the object builded.</param>
		/// <param name="rotation">The rotation that will have the object during its build process.</param>
		/// <param name="parent">The parent transform fo the new created buildable entity.</param>
		/// <returns>The Buildable component of the object to build.</returns>
		public Buildable CreateBuildable(UnitConfig toBuild, Vector3 location, Quaternion rotation, Transform parent)
		{
			Buildable buildable = GameObject.Instantiate<Buildable>(constructPrefab, location, rotation, parent);
			GameEntity ge = buildable.ThisEntity;
			buildable.buildType = toBuild;
			PlayerControlled pcSrc = GetComponent<PlayerControlled>();
			PlayerControlled pcDst = ge.GetComponent<PlayerControlled>();
			if(pcSrc != null)
			{
				if(pcDst != null)
					pcDst.Owner = pcSrc.Owner;
				else
					Debug.LogWarning("The Builder has a PlayerControlled component but the Builded unit hasn't.");
			}
			ge.ChangeVisualModule( CreateVisual(toBuild, ge.transform) );
			ge.gameObject.SetActive(true);
			if(toBuild!=null)
				toBuild.SetupGameEntity(ge);

			return ge.GetComponent<Buildable>();
		}

		/// <summary>
		/// Creates a VisualModule with the given config and attach it to the given parent.
		/// </summary>
		/// <param name="config">Config of the unit to instantiate.</param>
		/// <param name="parent">Transform that'll become the parent of the created VisualModule.</param>
		/// <returns>Returns the ComponentProxy component attached to the Root of the created VisualModule.</returns>
		private static ComponentProxy CreateVisual(UnitConfig config, Transform parent)
		{
			return config.CreateVisual(parent);
		}
	}
}
