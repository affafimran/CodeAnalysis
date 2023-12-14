using GameBase;
using NullPointerGame.Extras;
using NullPointerGame.BuildSystem;
using SpaceRTS.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SpaceRTS.UI
{
	/// <summary>
	/// UI Panel controller for the build capability of ships and structures.
	/// </summary>
	public class RTSUnitBuildUI : MonoBehaviour
	{
		public HumanPlayerHandler rtsScene;
		/// <summary>
		/// Reference to the Construction Handler that will let the user choose the next build location.
		/// </summary>
		public BuildHandler ghostControl;
		/// <summary>
		/// The group controller keeping track of the selected units.
		/// </summary>
		public GroupController group;
		/// <summary>
		/// Root GameObject for the builder panel that will be shown when a builder is selected.
		/// </summary>
		public GameObject builderPanel;
		/// <summary>
		/// Instantiation handler for the button list of units to build.
		/// </summary>
		public ContentPool builderPool;
		/// <summary>
		/// Panel container for the units queued to build.
		/// </summary>
		public GameObject buildQueuePanel;
		/// <summary>
		/// Instantiation handler for the button list of queued units to build.
		/// </summary>
		public ContentPool buildQueuePool;

		/// <summary>
		/// List of buttons to enable or disable if they are available to build.
		/// </summary>
		[System.Obsolete("Use instead the builderPool to instantiate the buttons.")]
		public List<Button> buttons = new List<Button>();
		/// <summary>
		/// List of Text labels to configure with the names of the units to build.
		/// </summary>
		[System.Obsolete("Use instead the builderPool to instantiate the buttons.")]
		public List<Text> btnTexts = new List<Text>();

		private class QueueItemInfo
		{
			public UnitConfig unit;
			public int queueIndex;
			public int count;
		}

		private Dictionary<GameObject, UnitConfig> buildBtns = new Dictionary<GameObject, UnitConfig>();
		private Dictionary<GameObject, QueueItemInfo> buildQueueBtns = new Dictionary<GameObject, QueueItemInfo>();
		private Builder selectedBuilder;

		// Use this for initialization
		void Start ()
		{
			group.OnShipsChanged += RefreshBuilderPanel;
			if(!builderPool)
				Debug.LogWarning("Must properly set the ContentPool in BuilderPool to control the build buttons.", this.gameObject);
			if(!buildQueuePool)
				Debug.LogWarning("Must properly set the ContentPool in BuildQueuePool to control the build queue buttons.", this.gameObject);
		}

		void OnDestroy ()
		{
			group.OnShipsChanged -= RefreshBuilderPanel;
		}
	
		/// <summary>
		/// Refresh the builder ui panel with the current selected builder info.
		/// </summary>
		private void RefreshBuilderPanel()
		{
			if (group.EntitiesCount == 0)
				OnCurrentBuilderChanged(null);
			else
				OnCurrentBuilderChanged( group.GetBestBuilder<Builder>() );
		}

		private void OnCurrentBuilderChanged( Builder newBuilder )
		{
			if(selectedBuilder == newBuilder)
				return;

			if (selectedBuilder != null)
				selectedBuilder.BuildQueueChanged -= OnBuildQueueChanged;

			selectedBuilder = newBuilder;

			if(selectedBuilder != null)
			{
				selectedBuilder.BuildQueueChanged += OnBuildQueueChanged;
				SetupBuilderPanel();
				SetupBuildQueuePanel();
			}
			else
			{
				ShowPanel(builderPanel, false);
				ShowPanel(buildQueuePanel, false);
			}
		}

		private void OnBuildQueueChanged()
		{
			SetupBuildQueuePanel();
		}

		private void SetupBuilderPanel()
		{
			if(builderPool)
				builderPool.Destroy(buildBtns.Keys);
			buildBtns.Clear();

			if( builderPool && selectedBuilder && selectedBuilder.buildables.Count > 0)
			{
				foreach (UnitConfig uc in selectedBuilder.buildables)
				{
					RTSUnitConfig config = uc as RTSUnitConfig;
					GameObject obj = builderPool.Instantiate();
					buildBtns.Add(obj, config);
					//ComponentProxy proxy = obj.GetComponent<ComponentProxy>();
					RTSBuildableButton btn = obj.GetComponent<RTSBuildableButton>();
					//Image img = proxy.GetPropertyValue<Image>("content_img");
					//obj.name = config.name + " Item";
					//img.sprite = config.uiImage;
					btn.Config = config;
				}
				ShowPanel(builderPanel, true);
			}
			else
				ShowPanel(builderPanel, false);
		}

		private void SetupBuildQueuePanel()
		{
			if(buildQueuePool)
				buildQueuePool.Destroy(buildQueueBtns.Keys);

			buildQueueBtns.Clear();

			StaticBuilder staticBuilder = selectedBuilder.GetComponent<StaticBuilder>();
			if (buildQueuePool && staticBuilder && staticBuilder.BuildsInQueue > 0)
			{
				List<QueueItemInfo> itemsList = new List<QueueItemInfo>();
				QueueItemInfo current = null;
				int index = 0;
				foreach (UnitConfig config in staticBuilder.QueuedUnits)
				{
					if(current == null || current.unit != config)
					{
						current = new QueueItemInfo();
						current.unit = config;
						current.count = 1;
						current.queueIndex = index;
						itemsList.Add(current);
					}
					else
						current.count = current.count + 1;
					
					index++;
				}

				for(index = 0; index < itemsList.Count; index++)
				{
					QueueItemInfo queueItem = itemsList[index];
					GameObject obj = buildQueuePool.Instantiate();
					ComponentProxy proxy = obj.GetComponent<ComponentProxy>();
					Image img = proxy.GetPropertyValue<Image>("content_img");
					Text txt = proxy.GetPropertyValue<Text>("content_count");
					Button btnRemove = proxy.GetPropertyValue<Button>("content_remove");
					RTSUnitConfig queuedUnitConfig = queueItem.unit as RTSUnitConfig;
					img.sprite = queuedUnitConfig.uiImage;
					txt.text = queueItem.count.ToString();
					btnRemove.onClick.RemoveAllListeners();
					btnRemove.onClick.AddListener(() => { OnRemoveFromQueue(obj); } );
					obj.name = queueItem.unit.name + " Item";
					obj.transform.SetSiblingIndex(index);
					buildQueueBtns.Add(obj, queueItem);
				}
				ShowPanel(buildQueuePanel, true);
			}
			else
				ShowPanel(buildQueuePanel, false);
		}

		/// <summary>
		/// Called when a build selection button is clicked and setup the ghost builder controller
		/// for structures that requires a build location calling next to OnConstructionConfirmed.
		/// </summary>
		/// <param name="btn"></param>
		public void OnBuildSlotClicked(Button btn)
		{
			UnitConfig toBuild = null;
			
			if(!buildBtns.TryGetValue(btn.gameObject, out toBuild))
				return; // some error here

			selectedBuilder.Build(toBuild);
		}

		/// <summary>
		/// Removes a specific build item from the queue
		/// </summary>
		/// <param name="clickedObject">The refrence to the queue item that made the remove from queue request.</param>
		public void OnRemoveFromQueue(GameObject clickedObject)
		{
			QueueItemInfo itemInfo;
			if( !buildQueueBtns.TryGetValue(clickedObject, out itemInfo) )
			{
				Debug.LogError("The clicked object is not tracked by the system.");
				return;
			}
			selectedBuilder.CancelBuildAt(itemInfo.queueIndex+itemInfo.count-1);
		}

		/// <summary>
		/// Shows or hide the builder panel.
		/// </summary>
		/// <param name="show"></param>
		[System.Obsolete("Use ShowPanel() instead.")]
		private void ShowBuilderPanel(bool show)
		{
			if( builderPanel && builderPanel.activeSelf != show)
				builderPanel.SetActive(show);
		}

		/// <summary>
		/// Shows or hide a generic panel.
		/// </summary>
		/// <param name="panel">The panel to show/hide.</param>
		/// <param name="show">Must be true to show the panel, false otherwise.</param>
		private void ShowPanel(GameObject panel, bool show)
		{
			if( panel && panel.activeSelf != show)
				panel.SetActive(show);
		}
	}
}