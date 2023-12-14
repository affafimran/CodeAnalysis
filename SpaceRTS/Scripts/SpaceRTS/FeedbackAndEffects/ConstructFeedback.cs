using GameBase;
using NullPointerGame.BuildSystem;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	[RequireComponent(typeof(Buildable))]
	public class ConstructFeedback : GameEntityComponent
	{
		[SerializeField]
		private ProxyRef constructFX = new ProxyRef(typeof(FXUnderConstruction), "construct_fx");

		private FXUnderConstruction ConstructFX { get { return constructFX.Get<FXUnderConstruction>(); } }

		private Buildable buildable;

		public void Start()
		{
			buildable = GetComponent<Buildable>();
			OnConstructProgressChanged();
			buildable.ConstructProgressChanged += OnConstructProgressChanged;
		}

		public void OnDestroy()
		{
			buildable.ConstructProgressChanged -= OnConstructProgressChanged;
		}

		/// <summary>
		/// overrided implementation to configure the component with the VisualModule properties.
		/// </summary>
		public override void OnVisualModuleSetted()
		{
			constructFX.SafeAssign(ThisEntity);
			base.OnVisualModuleSetted();
		}

		/// <summary>
		/// Removes all references to the VisualModule.
		/// </summary>
		public override void OnVisualModuleRemoved()
		{
			constructFX.SafeClear();
			base.OnVisualModuleRemoved();
		}

		private void OnConstructProgressChanged()
		{
			float progress = buildable.GetProgress();
			if (ConstructFX)
				ConstructFX.SetProgress(progress);

		}
	}
}