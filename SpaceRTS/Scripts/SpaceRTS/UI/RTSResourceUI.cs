using NullPointerCore.Backend.ResourceGathering;
using NullPointerCore.CoreSystem;
using NullPointerGame.ResourceSystem;
using SpaceRTS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceRTS.UI
{
	public class RTSResourceUI : MonoBehaviour
	{
		public ResourceID resource;
		public bool infiniteCapacity = true;
		public Text resourceQtyText;

		private Player player;
		private Storage resourceStorage;

		// Use this for initialization
		void Start ()
		{
			HumanPlayerHandler humanHandler = GameObject.FindObjectOfType<HumanPlayerHandler>();
			if(humanHandler==null)
			{
				Debug.LogError("Requires a RTSScene.");
				return;
			}
			player = humanHandler.ThisPlayer;
			if(player==null)
			{
				Debug.LogError("Requires to define the human player in RTSScene.");
				return;
			}
			StorageContainer playerStorage = player.GetComponent<StorageContainer>();
			if(playerStorage==null)
			{
				Debug.LogError("Requires to define a StorageContainer in the Player.");
				return;
			}
			resourceStorage = playerStorage.Get(resource);

			if(resourceQtyText==null)
				resourceQtyText = GetComponent<Text>();
		}

		public void OnValidate()
		{
			if(resourceQtyText==null)
				resourceQtyText = GetComponent<Text>();
		}

		// Update is called once per frame
		void Update ()
		{
			if(resourceStorage != null && resourceQtyText != null)
			{
				string content = resourceStorage.Stored.ToString();
				if(!infiniteCapacity)
					content += "/" + resourceStorage.Capacity;
				resourceQtyText.text = content;
			}
		
		}
	}
}