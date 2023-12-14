public class BallistaBuilding
{
	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.woodPrice = 200;
			mixedPrice.crystalPrice = 100;
			break;
		case 2:
			mixedPrice.woodPrice = 300;
			mixedPrice.crystalPrice = 150;
			break;
		case 3:
			mixedPrice.woodPrice = 400;
			mixedPrice.crystalPrice = 175;
			break;
		case 4:
			mixedPrice.woodPrice = 600;
			mixedPrice.crystalPrice = 225;
			break;
		case 5:
			mixedPrice.woodPrice = 850;
			mixedPrice.crystalPrice = 300;
			break;
		case 6:
			mixedPrice.woodPrice = 1100;
			mixedPrice.crystalPrice = 400;
			break;
		case 7:
			mixedPrice.woodPrice = 1400;
			mixedPrice.crystalPrice = 500;
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
			return 12;
		case 5:
			return 18;
		case 6:
			return 25;
		case 7:
			return 35;
		default:
			return 35;
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
			return 125;
		case 4:
			return 150;
		case 5:
			return 200;
		case 6:
			return 250;
		case 7:
			return 325;
		default:
			return 325;
		}
	}
}
