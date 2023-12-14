using UnityEngine;

public class Upgrades : MonoBehaviour
{
	public static Upgrades _instance;

	[SerializeField]
	private GameObject upgradeWindow;

	public UpgradeStuff atkSpeed;

	public UpgradeStuff playerDmg;

	public UpgradeStuff zombieHp;

	public UpgradeStuff buildingHeal;

	public UpgradeStuff slow;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		SetAll();
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.N))
		{
			OpenUpgradeWindow();
		}
	}

	public void OpenUpgradeWindow()
	{
		if (!HasGreatHall())
		{
			MessageHandler._instance.ShowMessage("You need to build a Great Hall first", 2f, Color.red);
			return;
		}
		SetAll();
		upgradeWindow.SetActive(value: true);
	}

	private bool HasGreatHall()
	{
		bool result = false;
		foreach (Building allPlacedBuilding in GameManager._instance.allPlacedBuildings)
		{
			if (allPlacedBuilding.type == GameManager.BuildingType.GREAT_HALL)
			{
				result = true;
			}
		}
		return result;
	}

	public void SetAll()
	{
		SetAttackSpeed();
		SetPlayerDmg();
		SetZombieHp();
		SetBuildingHeal();
		SetSlow();
	}

	private void SetAttackSpeed()
	{
		atkSpeed.lvlText.text = atkSpeed.level + "/" + atkSpeed.maxLevel;
		atkSpeed.basePrice = 10 + Mathf.Min(atkSpeed.level * 6, 90);
		atkSpeed.priceText.text = string.Concat(atkSpeed.basePrice);
		atkSpeed.bonus = atkSpeed.level * 5;
		atkSpeed.currentValueText.text = "+" + atkSpeed.bonus + "%";
		atkSpeed.nextValueText.text = "+" + (atkSpeed.bonus + 5) + "%";
	}

	public void BuyAtkSpeed()
	{
		if ((float)CurrencyManager._instance.essences < atkSpeed.basePrice)
		{
			MessageHandler._instance.ShowMessage("Not enough essences", 1f, Color.red);
		}
		else if (atkSpeed.level < atkSpeed.maxLevel)
		{
			CurrencyManager._instance.AddEssence((int)(0f - atkSpeed.basePrice));
			atkSpeed.level++;
			SetAttackSpeed();
		}
	}

	private void SetPlayerDmg()
	{
		playerDmg.lvlText.text = playerDmg.level + "/" + playerDmg.maxLevel;
		playerDmg.basePrice = 10 + Mathf.Min(playerDmg.level * 6, 90);
		playerDmg.priceText.text = string.Concat(playerDmg.basePrice);
		playerDmg.bonus = 1 + playerDmg.level;
		playerDmg.currentValueText.text = playerDmg.bonus + "x";
		playerDmg.nextValueText.text = playerDmg.bonus + 1 + "x";
		if (playerDmg.level >= 5)
		{
			playerDmg.bonus *= 2;
		}
		if (playerDmg.level >= 15)
		{
			playerDmg.bonus *= 2;
		}
		if (playerDmg.level >= 30)
		{
			playerDmg.bonus *= 2;
		}
	}

	public void BuyPlayerDamage()
	{
		if ((float)CurrencyManager._instance.essences < playerDmg.basePrice)
		{
			MessageHandler._instance.ShowMessage("Not enough essences", 1f, Color.red);
		}
		else if (playerDmg.level < playerDmg.maxLevel)
		{
			CurrencyManager._instance.AddEssence((int)(0f - playerDmg.basePrice));
			playerDmg.level++;
			SetPlayerDmg();
		}
	}

	private void SetZombieHp()
	{
		zombieHp.lvlText.text = zombieHp.level + "/" + zombieHp.maxLevel;
		zombieHp.basePrice = 50 + zombieHp.level * 50;
		zombieHp.priceText.text = string.Concat(zombieHp.basePrice);
		zombieHp.bonus = zombieHp.level * 5;
		zombieHp.currentValueText.text = "-" + zombieHp.bonus + "%";
		zombieHp.nextValueText.text = "-" + (zombieHp.bonus + 5) + "%";
	}

	public void BuyZombieHp()
	{
		if ((float)CurrencyManager._instance.essences < zombieHp.basePrice)
		{
			MessageHandler._instance.ShowMessage("Not enough essences", 1f, Color.red);
		}
		else if (zombieHp.level < zombieHp.maxLevel)
		{
			CurrencyManager._instance.AddEssence((int)(0f - zombieHp.basePrice));
			zombieHp.level++;
			SetZombieHp();
		}
	}

	private void SetBuildingHeal()
	{
		buildingHeal.lvlText.text = buildingHeal.level + "/" + buildingHeal.maxLevel;
		buildingHeal.basePrice = 40 + buildingHeal.level * 30;
		buildingHeal.priceText.text = string.Concat(buildingHeal.basePrice);
		buildingHeal.bonus = buildingHeal.level * 4;
		buildingHeal.currentValueText.text = "-" + buildingHeal.bonus + "%";
		buildingHeal.nextValueText.text = "-" + (buildingHeal.bonus + 4) + "%";
	}

	public void BuyBuildingHeal()
	{
		if ((float)CurrencyManager._instance.essences < buildingHeal.basePrice)
		{
			MessageHandler._instance.ShowMessage("Not enough essences", 1f, Color.red);
		}
		else if (buildingHeal.level < buildingHeal.maxLevel)
		{
			CurrencyManager._instance.AddEssence((int)(0f - buildingHeal.basePrice));
			buildingHeal.level++;
			SetBuildingHeal();
		}
	}

	private void SetSlow()
	{
		slow.lvlText.text = slow.level + "/" + slow.maxLevel;
		slow.basePrice = 20 + slow.level * 50;
		slow.priceText.text = string.Concat(slow.basePrice);
		slow.bonus = slow.level * 3;
		slow.currentValueText.text = "+" + slow.bonus + "%";
		slow.nextValueText.text = "+" + (slow.bonus + 3) + "%";
	}

	public void BuySlow()
	{
		if ((float)CurrencyManager._instance.essences < slow.basePrice)
		{
			MessageHandler._instance.ShowMessage("Not enough essences", 1f, Color.red);
		}
		else if (slow.level < slow.maxLevel)
		{
			CurrencyManager._instance.AddEssence((int)(0f - slow.basePrice));
			slow.level++;
			SetSlow();
		}
	}
}
