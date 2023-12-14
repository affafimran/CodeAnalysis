using UnityEngine;

public class Tile : MonoBehaviour
{
	public GameManager.TileType type;

	public GameManager.TileSpecific spec;

	private SpriteRenderer sr;

	public Building building;

	public Tile[] neighbours;

	public bool isHighlighted;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	private void OnMouseOver()
	{
		if (!GameManager.IsPointerOverUIObject())
		{
			GameManager._instance.currentTile = this;
			if (!GameManager._instance.isBuildingUnderPlacement)
			{
				Select(new Color(0.5f, 0.5f, 0.5f, 1f));
			}
		}
	}

	private void OnMouseExit()
	{
		if (!GameManager._instance.isBuildingUnderPlacement)
		{
			DeSelect();
		}
	}

	private void OnMouseUpAsButton()
	{
		if (!GameManager.IsPointerOverUIObject())
		{
			foreach (Building allPlacedBuilding in GameManager._instance.allPlacedBuildings)
			{
				allPlacedBuilding.Deselect();
			}
		}
		if (spec == GameManager.TileSpecific.BUILDING)
		{
			building.OnClick();
		}
	}

	public void Select(Color c)
	{
		sr.color = c;
	}

	public void DeSelect()
	{
		if (!isHighlighted)
		{
			sr.color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			sr.color = Color.green;
		}
	}

	public void Highlight()
	{
		isHighlighted = true;
		sr.color = Color.green;
	}

	public void DisableHighlight()
	{
		isHighlighted = false;
		DeSelect();
	}
}
