public class WizardTowerBuilding
{
	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.crystalPrice = 300;
			mixedPrice.woodPrice = 50;
			break;
		case 2:
			mixedPrice.crystalPrice = 400;
			mixedPrice.woodPrice = 100;
			break;
		case 3:
			mixedPrice.crystalPrice = 600;
			mixedPrice.woodPrice = 150;
			break;
		case 4:
			mixedPrice.crystalPrice = 1000;
			mixedPrice.woodPrice = 225;
			break;
		case 5:
			mixedPrice.crystalPrice = 1300;
			mixedPrice.woodPrice = 500;
			break;
		case 6:
			mixedPrice.crystalPrice = 1750;
			mixedPrice.woodPrice = 800;
			break;
		case 7:
			mixedPrice.crystalPrice = 2000;
			mixedPrice.woodPrice = 1000;
			break;
		}
		return mixedPrice;
	}

	public static int GetDamage(int level)
	{
		switch (level)
		{
		case 1:
			return 3;
		case 2:
			return 5;
		case 3:
			return 8;
		case 4:
			return 13;
		case 5:
			return 21;
		case 6:
			return 33;
		case 7:
			return 42;
		default:
			return 42;
		}
	}

	public static int GetHp(int level)
	{
		switch (level)
		{
		case 1:
			return 50;
		case 2:
			return 70;
		case 3:
			return 100;
		case 4:
			return 135;
		case 5:
			return 170;
		case 6:
			return 200;
		case 7:
			return 250;
		default:
			return 250;
		}
	}
}
