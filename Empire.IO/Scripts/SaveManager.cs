using System;
using System.Collections;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public static SaveManager _instance;

	public static string building = "BUILDING";

	public static string crystals = "CRYSTALS";

	public static string woods = "WOODS";

	public static string essences = "ESSENCES";

	public static string days = "DAYS";

	private void Awake()
	{
		_instance = this;
	}

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		if (PlayerPrefs.HasKey("HAS_SAVE"))
		{
			Load();
		}
	}

	private void Update()
	{
	}

	public void Save()
	{
		SaveBuildings();
		SaveCurrencies();
		PlayerPrefs.SetInt(days, DayNightManager._instance.dayNum);
		SaveUpgrades();
		PlayerPrefs.SetFloat("PLAYER_POSX", UnityEngine.Object.FindObjectOfType<PlayerShooter>().transform.position.x);
		PlayerPrefs.SetFloat("PLAYER_POSY", UnityEngine.Object.FindObjectOfType<PlayerShooter>().transform.position.y);
		PlayerPrefs.SetInt("HAS_SAVE", 1);
	}

	public void Load()
	{
		LoadUpgrades();
		LoadBuildings();
		LoadCurrencies();
		DayNightManager._instance.dayNum = PlayerPrefs.GetInt(days, 1);
		DayNightManager._instance.SetText();
		UnityEngine.Object.FindObjectOfType<PlayerShooter>().transform.position = new Vector2(PlayerPrefs.GetFloat("PLAYER_POSX", -7f), PlayerPrefs.GetFloat("PLAYER_POSY", 1f));
	}

	private void SaveBuildings()
	{
		int i;
		for (i = 0; i < GameManager._instance.allPlacedBuildings.Count; i++)
		{
			GameManager._instance.allPlacedBuildings[i].Save(building + i);
		}
		while (PlayerPrefs.HasKey(building + ++i))
		{
			PlayerPrefs.DeleteKey(building + i);
		}
	}

	private void LoadBuildings()
	{
		for (int i = 0; PlayerPrefs.HasKey(building + i); i++)
		{
			string[] array = PlayerPrefs.GetString(building + i).Split('|');
			GameManager.BuildingType buildingType = ParseEnum<GameManager.BuildingType>(array[0]);
			BuildingButton[] buildingButtons = Tutorial._instance.buildingButtons;
			foreach (BuildingButton buildingButton in buildingButtons)
			{
				if (buildingButton.type == buildingType)
				{
					Building component = UnityEngine.Object.Instantiate(buildingButton.buildingPrefab).GetComponent<Building>();
					component.buildingButton = buildingButton;
					component.Init();
					component.levelObjects[0].SetActive(value: false);
					component.level = int.Parse(array[1]);
					component.SetStats();
					component.health = int.Parse(array[2]);
					component.transform.position = new Vector3(float.Parse(array[3]), float.Parse(array[4]), 0f);
					component.SearchOccupiedTiles();
					component.PlaceBuilding();
					component.EnableUpgradesSprites(component.level - 1);
				}
			}
		}
	}

	private void SaveCurrencies()
	{
		PlayerPrefs.SetInt(crystals, CurrencyManager._instance.crystals);
		PlayerPrefs.SetInt(woods, CurrencyManager._instance.woods);
		PlayerPrefs.SetInt(essences, CurrencyManager._instance.essences);
	}

	private void LoadCurrencies()
	{
		CurrencyManager._instance.crystals = (CurrencyManager._instance.woods = (CurrencyManager._instance.essences = 0));
		CurrencyManager._instance.AddCrystal(PlayerPrefs.GetInt(crystals));
		CurrencyManager._instance.AddWood(PlayerPrefs.GetInt(woods));
		CurrencyManager._instance.AddEssence(PlayerPrefs.GetInt(essences));
	}

	private void SaveUpgrades()
	{
		PlayerPrefs.SetInt("UPGRADE_ATKSPEED", Upgrades._instance.atkSpeed.level);
		PlayerPrefs.SetInt("UPGRADE_DMG", Upgrades._instance.playerDmg.level);
		PlayerPrefs.SetInt("UPGRADE_HP", Upgrades._instance.zombieHp.level);
		PlayerPrefs.SetInt("UPGRADE_HEAL", Upgrades._instance.buildingHeal.level);
		PlayerPrefs.SetInt("UPGRADE_SLOW", Upgrades._instance.slow.level);
	}

	private void LoadUpgrades()
	{
		Upgrades._instance.atkSpeed.level = PlayerPrefs.GetInt("UPGRADE_ATKSPEED");
		Upgrades._instance.playerDmg.level = PlayerPrefs.GetInt("UPGRADE_DMG");
		Upgrades._instance.zombieHp.level = PlayerPrefs.GetInt("UPGRADE_HP");
		Upgrades._instance.buildingHeal.level = PlayerPrefs.GetInt("UPGRADE_HEAL");
		Upgrades._instance.slow.level = PlayerPrefs.GetInt("UPGRADE_SLOW");
		Upgrades._instance.SetAll();
	}

	public static T ParseEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
	}
}
