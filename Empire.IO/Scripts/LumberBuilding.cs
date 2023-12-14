public class LumberBuilding
{
	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.crystalPrice = 100;
			break;
		case 2:
			mixedPrice.crystalPrice = 150;
			break;
		case 3:
			mixedPrice.crystalPrice = 250;
			break;
		case 4:
			mixedPrice.crystalPrice = 500;
			break;
		case 5:
			mixedPrice.crystalPrice = 900;
			break;
		case 6:
			mixedPrice.crystalPrice = 1300;
			break;
		case 7:
			mixedPrice.crystalPrice = 2000;
			break;
		}
		return mixedPrice;
	}

	public static int GetProductionAmount(int level)
	{
		switch (level)
		{
		case 1:
			return 10;
		case 2:
			return 15;
		case 3:
			return 25;
		case 4:
			return 35;
		case 5:
			return 50;
		case 6:
			return 65;
		case 7:
			return 80;
		default:
			return 80;
		}
	}

	public static int GetHp(int level)
	{
		switch (level)
		{
		case 1:
			return 50;
		case 2:
			return 75;
		case 3:
			return 100;
		case 4:
			return 125;
		case 5:
			return 150;
		case 6:
			return 200;
		case 7:
			return 250;
		default:
			return 250;
		}
	}
}
