public class CannonBuilding
{
	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.crystalPrice = 200;
			mixedPrice.woodPrice = 200;
			break;
		case 2:
			mixedPrice.crystalPrice = 300;
			mixedPrice.woodPrice = 300;
			break;
		case 3:
			mixedPrice.crystalPrice = 500;
			mixedPrice.woodPrice = 500;
			break;
		case 4:
			mixedPrice.crystalPrice = 700;
			mixedPrice.woodPrice = 700;
			break;
		case 5:
			mixedPrice.crystalPrice = 1000;
			mixedPrice.woodPrice = 1000;
			break;
		case 6:
			mixedPrice.crystalPrice = 1500;
			mixedPrice.woodPrice = 1500;
			break;
		case 7:
			mixedPrice.crystalPrice = 2000;
			mixedPrice.woodPrice = 2000;
			break;
		}
		return mixedPrice;
	}

	public static int GetDamage(int level)
	{
		switch (level)
		{
		case 1:
			return 4;
		case 2:
			return 7;
		case 3:
			return 12;
		case 4:
			return 20;
		case 5:
			return 30;
		case 6:
			return 40;
		case 7:
			return 55;
		default:
			return 55;
		}
	}

	public static int GetHp(int level)
	{
		switch (level)
		{
		case 1:
			return 75;
		case 2:
			return 100;
		case 3:
			return 150;
		case 4:
			return 200;
		case 5:
			return 275;
		case 6:
			return 350;
		case 7:
			return 500;
		default:
			return 500;
		}
	}
}
