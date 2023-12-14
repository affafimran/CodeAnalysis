using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
	public GameObject buildingPrefab;

	public int woodPrice;

	public int crystalPrice;

	[SerializeField]
	private Text woodPriceText;

	[SerializeField]
	private Text crystalPriceText;

	[SerializeField]
	private GameObject overPanel;

	public GameManager.BuildingType type;

	[SerializeField]
	private KeyCode quickCastKey;

	private void Start()
	{
		woodPriceText.text = string.Concat(woodPrice);
		crystalPriceText.text = string.Concat(crystalPrice);
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(quickCastKey))
		{
			OnClick();
		}
	}

	public void OnClick()
	{
		if (!GameManager._instance.isBuildingUnderPlacement)
		{
			if (DayNightManager._instance.isNight)
			{
				MessageHandler._instance.ShowMessage("Cannot build during the night", 0.5f, Color.red);
				return;
			}
			if (woodPrice > CurrencyManager._instance.woods)
			{
				MessageHandler._instance.ShowMessage("Not Enough Woods", 0.5f, Color.red);
				return;
			}
			if (crystalPrice > CurrencyManager._instance.crystals)
			{
				MessageHandler._instance.ShowMessage("Not Enough Crystals", 0.5f, Color.red);
				return;
			}
			GameManager._instance.isBuildingUnderPlacement = true;
			Object.Instantiate(buildingPrefab).GetComponent<Building>().buildingButton = this;
		}
	}

	public void HoverOver()
	{
		woodPriceText.color = Color.white;
		crystalPriceText.color = Color.white;
		if (woodPrice > CurrencyManager._instance.woods)
		{
			woodPriceText.color = Color.red;
		}
		if (crystalPrice > CurrencyManager._instance.crystals)
		{
			crystalPriceText.color = Color.red;
		}
		overPanel.SetActive(value: true);
	}

	public void IncreasePrice()
	{
		crystalPrice = (int)((float)crystalPrice * 1.12f);
		woodPrice = (int)((float)woodPrice * 1.12f);
		woodPriceText.text = string.Concat(woodPrice);
		crystalPriceText.text = string.Concat(crystalPrice);
	}

	public void DecreasePrice()
	{
		crystalPrice = (int)((float)crystalPrice / 1.12f);
		woodPrice = (int)((float)woodPrice / 1.12f);
		woodPriceText.text = string.Concat(woodPrice);
		crystalPriceText.text = string.Concat(crystalPrice);
	}
}
