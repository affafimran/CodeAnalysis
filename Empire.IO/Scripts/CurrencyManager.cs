using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
	public enum CurrencyType
	{
		CRYSTAL,
		WOOD,
		ESSENCE
	}

	public static CurrencyManager _instance;

	public int crystals;

	public int woods;

	public int essences;

	public Text crystalText;

	public Text woodText;

	public Text essenceText;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		//woods = 150;
		//crystals = 150;
		//essences = 0;
		crystalText.text = GetSuffix(crystals);
		woodText.text = GetSuffix(woods);
		essenceText.text = GetSuffix(essences);
	}

	public static string GetSuffix(long num)
	{
		string text = "";
		string text2 = num.ToString();
		if (num < 1000)
		{
			return num.ToString();
		}
		switch ((text2.Length - 1) / 3)
		{
		case 0:
			text = string.Empty;
			break;
		case 1:
			text = "K";
			break;
		case 2:
			text = "M";
			break;
		case 3:
			text = "B";
			break;
		case 4:
			text = "T";
			break;
		case 5:
			text = "Qa";
			break;
		case 6:
			text = "Qi";
			break;
		case 7:
			text = "S";
			break;
		}
		if (text2.Length % 3 == 1)
		{
			return text2.Substring(0, 1) + "." + text2.Substring(1, 1) + text2.Substring(2, 1) + text;
		}
		if (text2.Length % 3 == 2)
		{
			return text2.Substring(0, 1) + text2.Substring(1, 1) + "." + text2.Substring(2, 1) + text;
		}
		if (text2.Length % 3 == 0)
		{
			return text2.Substring(0, 1) + text2.Substring(1, 1) + text2.Substring(2, 1) + text;
		}
		return "";
	}

	public void AddWood(int n)
	{
		woods += n;
		woodText.text = GetSuffix(woods);
	}

	public void AddCrystal(int n)
	{
		crystals += n;
		crystalText.text = GetSuffix(crystals);
	}

	public void AddEssence(int n)
	{
		essences += n;
		essenceText.text = GetSuffix(essences);
	}
}
