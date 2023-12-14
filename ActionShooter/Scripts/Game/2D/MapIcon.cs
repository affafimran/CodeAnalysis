using UnityEngine;
using System.Collections;

public class MapIcon : MonoBehaviour {

	public Transform itemTransform; // The 3d object to follow.
	public bool staticItem; // Should we follow this object or just use it's initial position?

	private float initialHeight; // The Y position is set on creation in the MapSystem.

	void Start ()
	{
		if (itemTransform == null)
		{
//			Debug.Log("[MapIcon] " + this.gameObject.name + "item not set on creation. Selfdestructing...");
			Destroy(this.gameObject);
		}

		initialHeight = transform.localPosition.y; // Store the height we have set in the MapSystem.
	}

	void Update ()
	{
		if (itemTransform == null) // If the item we're following is destroyed, destroy this too.
		{
			Destroy(this.gameObject);
		}
		else if (!staticItem)
		{
			transform.localPosition = new Vector3(itemTransform.position.x, initialHeight, itemTransform.position.z);
			// No rotation functionality yet.
		}
	}
}
