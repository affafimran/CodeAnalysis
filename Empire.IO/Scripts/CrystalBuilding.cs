public class CrystalBuilding
{
	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.woodPrice = 100;
			break;
		case 2:
			mixedPrice.woodPrice = 200;
			break;
		case 3:
			mixedPrice.woodPrice = 400;
			break;
		case 4:
			mixedPrice.woodPrice = 750;
			break;
		case 5:
			mixedPrice.woodPrice = 1000;
			break;
		case 6:
			mixedPrice.woodPrice = 1300;
			break;
		case 7:
			mixedPrice.woodPrice = 2000;
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
			return 100;
		case 2:
			return 150;
		case 3:
			return 200;
		case 4:
			return 225;
		case 5:
			return 275;
		case 6:
			return 325;
		case 7:
			return 400;
		default:
			return 400;
		}
	}
}
