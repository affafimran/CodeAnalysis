using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
	public GameManager.BuildingType type;

	[Space]
	[Header("STATS")]
	public int level = 1;

	public float health = 100f;

	public float maxHealth = 100f;

	[Space(20f)]
	[SerializeField]
	private SpriteRenderer sr;

	private MixedPrice upgradePrice;

	[SerializeField]
	private GameObject placeParticle;

	[SerializeField]
	private Image healthBar;

	[HideInInspector]
	public BuildingButton buildingButton;

	public GameObject[] levelObjects;

	[SerializeField]
	private Vector3 tileOffset;

	[SerializeField]
	private Vector3 placeOffset;

	[SerializeField]
	private float placingRadius = 0.5f;

	private bool isPlaced;

	private List<Tile> tilesOccupied;

	[SerializeField]
	private MonoBehaviour[] scriptsToEnable;

	[SerializeField]
	private PolygonCollider2D defaultCollider;

	[SerializeField]
	private StatPanel statPanel;

	private void Start()
	{
		if (!isPlaced)
		{
			Init();
		}
	}

	public void Init()
	{
		placeParticle.SetActive(value: false);
		tilesOccupied = new List<Tile>();
		sr.color = new Color(1f, 1f, 1f, 0.5f);
		if (isPlaced && type == GameManager.BuildingType.CASTLE)
		{
			GameManager._instance.CastlePlaced(base.transform);
		}
		defaultCollider.enabled = false;
		HighlightPossiblePositions();
		healthBar.transform.parent.gameObject.SetActive(value: false);
		SetStats();
	}

	private void Update()
	{
		if (!isPlaced)
		{
			FitGrid();
			HandlePlacement();
		}
	}

	private void HighlightPossiblePositions()
	{
		Tile[] gemTiles;
		if (type == GameManager.BuildingType.CRYSTAL_MINE)
		{
			gemTiles = GameManager._instance.gemTiles;
			foreach (Tile obj in gemTiles)
			{
				obj.Highlight();
				Tile[] neighbours = obj.neighbours;
				for (int j = 0; j < neighbours.Length; j++)
				{
					neighbours[j].Highlight();
				}
			}
		}
		if (type != GameManager.BuildingType.LUMBER)
		{
			return;
		}
		gemTiles = GameManager._instance.treeTiles;
		foreach (Tile obj2 in gemTiles)
		{
			obj2.Highlight();
			Tile[] neighbours = obj2.neighbours;
			for (int j = 0; j < neighbours.Length; j++)
			{
				neighbours[j].Highlight();
			}
		}
	}

	private void HidePossiblePositions()
	{
		Tile[] gemTiles;
		if (type == GameManager.BuildingType.CRYSTAL_MINE)
		{
			gemTiles = GameManager._instance.gemTiles;
			foreach (Tile obj in gemTiles)
			{
				obj.DisableHighlight();
				Tile[] neighbours = obj.neighbours;
				for (int j = 0; j < neighbours.Length; j++)
				{
					neighbours[j].DisableHighlight();
				}
			}
		}
		if (type != GameManager.BuildingType.LUMBER)
		{
			return;
		}
		gemTiles = GameManager._instance.treeTiles;
		foreach (Tile obj2 in gemTiles)
		{
			obj2.DisableHighlight();
			Tile[] neighbours = obj2.neighbours;
			for (int j = 0; j < neighbours.Length; j++)
			{
				neighbours[j].DisableHighlight();
			}
		}
	}

	private void FitGrid()
	{
		if (!(base.transform.position == GameManager._instance.currentTile.transform.position + tileOffset))
		{
			base.transform.position = GameManager._instance.currentTile.transform.position + tileOffset;
			ClearOccupiedTiles();
			SearchOccupiedTiles();
			CheckTilesAvailability();
		}
	}

	public void SearchOccupiedTiles()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position + placeOffset, placingRadius);
		foreach (Collider2D collider2D in array)
		{
			if (collider2D.GetComponent<Tile>() != null)
			{
				tilesOccupied.Add(collider2D.GetComponent<Tile>());
			}
		}
	}

	private void HandlePlacement()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (!IsAvailableToBuild())
			{
				CancelPlacement();
				MessageHandler._instance.ShowMessage("Cannot build here", 0.6f, Color.red);
			}
			else if (GameManager._instance.isCastlePlaced && type == GameManager.BuildingType.CASTLE)
			{
				CancelPlacement();
				MessageHandler._instance.ShowMessage("1  Castle is already built", 0.6f, Color.red);
			}
			else
			{
				PlaceBuilding();
			}
		}
		else if (Input.GetMouseButtonDown(1))
		{
			CancelPlacement();
		}
	}

	public void PlaceBuilding()
	{
		isPlaced = true;
		defaultCollider.enabled = true;
		placeParticle.SetActive(value: true);
		MonoBehaviour[] array = scriptsToEnable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		foreach (Tile item in tilesOccupied)
		{
			item.spec = GameManager.TileSpecific.BUILDING;
			item.building = this;
			item.DeSelect();
		}
		if (type == GameManager.BuildingType.CASTLE)
		{
			GameManager._instance.CastlePlaced(base.transform);
		}
		sr.color = Color.white;
		HidePossiblePositions();
		GameManager._instance.isBuildingUnderPlacement = false;
		GameManager._instance.allPlacedBuildings.Add(this);
		DecreaseCurrency();
		buildingButton.IncreasePrice();
	}

	private void ClearOccupiedTiles()
	{
		foreach (Tile item in tilesOccupied)
		{
			item.DeSelect();
		}
		tilesOccupied = new List<Tile>();
	}

	private void CheckTilesAvailability()
	{
		Color c = IsAvailableToBuild() ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.red;
		foreach (Tile item in tilesOccupied)
		{
			item.Select(c);
		}
	}

	private bool IsAvailableToBuild()
	{
		bool result = true;
		foreach (Tile item in tilesOccupied)
		{
			if (item.type == GameManager.TileType.WATER || item.type == GameManager.TileType.WATER_PLANT || item.type == GameManager.TileType.ICE_MOUNTAIN || item.spec != 0)
			{
				result = false;
			}
			if (type == GameManager.BuildingType.CRYSTAL_MINE && !item.isHighlighted)
			{
				result = false;
			}
			if (type == GameManager.BuildingType.LUMBER && !item.isHighlighted)
			{
				result = false;
			}
		}
		return result;
	}

	private void CancelPlacement()
	{
		ClearOccupiedTiles();
		UnityEngine.Object.Destroy(base.gameObject);
		HidePossiblePositions();
		GameManager._instance.isBuildingUnderPlacement = false;
	}

	private void DecreaseCurrency()
	{
		BuildingButton buildingButton = GameManager._instance.GetBuildingButton(type);
		CurrencyManager._instance.AddCrystal(-buildingButton.crystalPrice);
		CurrencyManager._instance.AddWood(-buildingButton.woodPrice);
	}

	public void Deselect()
	{
		if (GetComponent<ShooterBuilding>() != null)
		{
			GetComponent<ShooterBuilding>().Deselect();
		}
		if (statPanel != null)
		{
			statPanel.gameObject.SetActive(value: false);
		}
	}

	public void OnClick()
	{
		if (!GameManager.IsPointerOverUIObject())
		{
			if (GetComponent<ShooterBuilding>() != null)
			{
				GetComponent<ShooterBuilding>().OnClick();
			}
			SetStatsPanel();
		}
	}

	public void OnClickUpgrade()
	{
		if (level < 7)
		{
			if (CurrencyManager._instance.woods < upgradePrice.woodPrice || CurrencyManager._instance.crystals < upgradePrice.crystalPrice)
			{
				MessageHandler._instance.ShowMessage("Not enough currency", 0.5f, Color.red);
				return;
			}
			if (DayNightManager._instance.isNight)
			{
				MessageHandler._instance.ShowMessage("Cannot upgrade at night", 0.5f, Color.red);
				return;
			}
			CurrencyManager._instance.AddCrystal(-upgradePrice.crystalPrice);
			CurrencyManager._instance.AddWood(-upgradePrice.woodPrice);
			UpgradeSuccessful();
			SetStatsPanel();
		}
	}

	private void SetStatsPanel()
	{
		if (!(statPanel != null))
		{
			return;
		}
		if (statPanel.woodUpgradePriceText != null || statPanel.crystalUpgradePriceText != null)
		{
			upgradePrice = GetUpgradePrice();
			statPanel.woodUpgradePriceText.text = CurrencyManager.GetSuffix(upgradePrice.woodPrice);
			statPanel.crystalUpgradePriceText.text = CurrencyManager.GetSuffix(upgradePrice.crystalPrice);
			if (CurrencyManager._instance.woods < upgradePrice.woodPrice)
			{
				statPanel.woodUpgradePriceText.color = Color.red;
			}
			else
			{
				statPanel.woodUpgradePriceText.color = Color.white;
			}
			if (CurrencyManager._instance.crystals < upgradePrice.crystalPrice)
			{
				statPanel.crystalUpgradePriceText.color = Color.red;
			}
			else
			{
				statPanel.crystalUpgradePriceText.color = Color.white;
			}
		}
		statPanel.gameObject.SetActive(value: true);
	}

	private void UpgradeSuccessful()
	{
		EnableUpgradesSprites(level);
		level++;
		SetStats();
		placeParticle.SetActive(value: true);
		healthBar.transform.parent.gameObject.SetActive(value: false);
	}

	public void EnableUpgradesSprites(int l)
	{
		levelObjects[Mathf.Min(Mathf.Max(l - 1, 0), levelObjects.Length - 1)].SetActive(value: false);
		levelObjects[Mathf.Min(l, levelObjects.Length - 1)].SetActive(value: true);
	}

	public void SetStats()
	{
		switch (type)
		{
		case GameManager.BuildingType.BARRACK:
			maxHealth = (health = SpawnBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(SpawnBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(SpawnBuilding.GetSoldierDamage(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(SpawnBuilding.GetSoldierDamage(level + 1));
			}
			break;
		case GameManager.BuildingType.BALLISTA_TOWER:
			GetComponent<ShooterBuilding>().damage = BallistaBuilding.GetDamage(level);
			maxHealth = (health = BallistaBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(BallistaBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(BallistaBuilding.GetDamage(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(BallistaBuilding.GetDamage(level + 1));
			}
			break;
		case GameManager.BuildingType.CANNON_TOWER:
			GetComponent<ShooterBuilding>().damage = CannonBuilding.GetDamage(level);
			maxHealth = (health = CannonBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(CannonBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(CannonBuilding.GetDamage(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(CannonBuilding.GetDamage(level + 1));
			}
			break;
		case GameManager.BuildingType.WIZARD_TOWER:
			GetComponent<ShooterBuilding>().damage = WizardTowerBuilding.GetDamage(level);
			maxHealth = (health = WizardTowerBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(WizardTowerBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(WizardTowerBuilding.GetDamage(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(WizardTowerBuilding.GetDamage(level + 1));
			}
			break;
		case GameManager.BuildingType.LUMBER:
			GetComponent<ProduceCurrency>().productionAmount = LumberBuilding.GetProductionAmount(level);
			maxHealth = (health = LumberBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(LumberBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(LumberBuilding.GetProductionAmount(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(LumberBuilding.GetProductionAmount(level + 1));
			}
			break;
		case GameManager.BuildingType.CRYSTAL_MINE:
			GetComponent<ProduceCurrency>().productionAmount = CrystalBuilding.GetProductionAmount(level);
			maxHealth = (health = CrystalBuilding.GetHp(level));
			if (statPanel.stat1Next != null)
			{
				statPanel.stat1Next.text = string.Concat(CrystalBuilding.GetHp(level + 1));
			}
			statPanel.stat2Current.text = string.Concat(CrystalBuilding.GetProductionAmount(level));
			if (statPanel.stat2Next != null)
			{
				statPanel.stat2Next.text = string.Concat(CrystalBuilding.GetProductionAmount(level + 1));
			}
			break;
		}
		if (statPanel.stat1Current != null)
		{
			statPanel.stat1Current.text = string.Concat(health);
		}
		if (level >= 7)
		{
			statPanel.stat1Next.gameObject.SetActive(value: false);
			statPanel.stat2Next.gameObject.SetActive(value: false);
			statPanel.crystalUpgradePriceText.transform.parent.gameObject.SetActive(value: false);
		}
		TakeDamage(0f);
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (damage != 0f)
		{
			healthBar.transform.parent.gameObject.SetActive(value: true);
		}
		healthBar.fillAmount = health / maxHealth;
		if (health <= 0f)
		{
			if (type == GameManager.BuildingType.CASTLE)
			{
				GameManager._instance.GameOver();
			}
			DestroyBuilding();
		}
	}

	private MixedPrice GetUpgradePrice()
	{
		switch (type)
		{
		case GameManager.BuildingType.BARRACK:
			return SpawnBuilding.GetPrice(level);
		case GameManager.BuildingType.BALLISTA_TOWER:
			return BallistaBuilding.GetPrice(level);
		case GameManager.BuildingType.CANNON_TOWER:
			return CannonBuilding.GetPrice(level);
		case GameManager.BuildingType.CRYSTAL_MINE:
			return CrystalBuilding.GetPrice(level);
		case GameManager.BuildingType.WIZARD_TOWER:
			return WizardTowerBuilding.GetPrice(level);
		case GameManager.BuildingType.LUMBER:
			return LumberBuilding.GetPrice(level);
		default:
			return null;
		}
	}

	public void DestroyBuilding()
	{
		GameManager._instance.allPlacedBuildings.Remove(this);
		foreach (Tile item in tilesOccupied)
		{
			item.spec = GameManager.TileSpecific.NONE;
		}
		buildingButton.DecreasePrice();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Heal()
	{
		health += maxHealth * (float)Upgrades._instance.buildingHeal.bonus / 100f;
		if (health > maxHealth)
		{
			health = maxHealth;
		}
	}

	public void OpenUpgrades()
	{
		Upgrades._instance.OpenUpgradeWindow();
	}

	public void Save(string saveName)
	{
		string text = type.ToString() + "|";
		text = text + level + "|" + health + "|";
		text = text + base.transform.position.x + "|" + base.transform.position.y + "|";
		PlayerPrefs.SetString(saveName, text);
	}
}
