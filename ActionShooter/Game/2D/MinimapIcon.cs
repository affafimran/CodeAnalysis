using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour {

	public Minimap minimap;
	public Transform itemTransform; // (DG) The 3d object transform.
	public Transform mapTargetTransform; // (DG) The player transform.
	public bool stickNorth;

	private Vector3 itemDirection;

	private float itemAngle;
	public float itemDistance;

	private Image image;

	
	void Awake ()
	{
		image = transform.GetComponent<Image>();
	}
	
	void Update ()
	{
		mapTargetTransform = minimap.map.mapTargetTransform;

		if (mapTargetTransform != null) // This is essential for both behaviors.
		{
			if (stickNorth) StickNorth();
			else if (itemTransform != null) RotateIcon(); // If there's no 3d item associated with this thing on creation, delete this.
			else Destroy(this.gameObject);
		}
	}

	private void RotateIcon()
	{
		itemDirection = itemTransform.position - mapTargetTransform.position; // A vector between the item and the player.
		itemDirection.y = 0.0f; // Flatten the direction on the y component, mainly to fix the visible range of the icon.
		itemDistance = itemDirection.magnitude;
		
		if (itemDistance > minimap.minimapViewRadius) // Check if it's still in view of the minimap. Otherwise make invisible. Could be dynamic! TBD.
		{
			image.enabled = true;
			
			itemAngle = Vector3.Angle(mapTargetTransform.forward, itemDirection); // The smallest angle between the two lines. 
			if (Vector3.Dot(mapTargetTransform.right,itemDirection) < 0) itemAngle = 360 - itemAngle; // Correction to make sure we have a 0 - 360 range.
			
			// Change the rotation of this icon.
			transform.localEulerAngles = new Vector3(0,0,-itemAngle);
			transform.Rotate(0,0,-90.0f); // Offset
		}
		else image.enabled = false;
	}

	private void StickNorth()
	{
		transform.localEulerAngles = new Vector3(0,0, mapTargetTransform.eulerAngles.y);
		transform.Rotate(0,0,-90.0f);
	}
}
