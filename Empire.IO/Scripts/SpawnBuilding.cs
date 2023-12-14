using System.Collections.Generic;
using UnityEngine;

public class SpawnBuilding : MonoBehaviour
{
	[SerializeField]
	private GameObject spawnPrefab;

	[SerializeField]
	private GameObject[] spawnPlaces;

	private List<GameObject> soldiers;

	public float timeToSpawn = 30f;

	private float timer;

	private Building building;

	private void Start()
	{
		soldiers = new List<GameObject>();
		building = GetComponent<Building>();
	}

	private void Update()
	{
		timer += Time.deltaTime * DayNightManager._instance.timeMultiplier;
		if (!(timer > timeToSpawn) || spawnPlaces.Length <= soldiers.Count)
		{
			return;
		}
		timer = 0f;
		GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab);
		GameObject[] array = spawnPlaces;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2.transform.childCount == 0)
			{
				gameObject.transform.SetParent(gameObject2.transform);
				break;
			}
		}
		gameObject.transform.localPosition = Vector3.zero;
		PlayerNPC component = gameObject.GetComponent<PlayerNPC>();
		component.hp = 50 + DayNightManager._instance.dayNum * 5;
		component.damage = GetSoldierDamage(building.level);
		component.spawnBuilding = this;
		soldiers.Add(gameObject);
	}

	public void RemoveSoldier(GameObject g)
	{
		soldiers.Remove(g);
	}

	public static int GetHp(int level)
	{
		switch (level)
		{
		case 1:
			return 200;
		case 2:
			return 300;
		case 3:
			return 450;
		case 4:
			return 550;
		case 5:
			return 675;
		case 6:
			return 750;
		case 7:
			return 900;
		default:
			return 900;
		}
	}

	public static MixedPrice GetPrice(int level)
	{
		MixedPrice mixedPrice = new MixedPrice();
		mixedPrice.crystalPrice = (mixedPrice.woodPrice = 0);
		switch (level)
		{
		case 1:
			mixedPrice.woodPrice = 500;
			mixedPrice.crystalPrice = 500;
			break;
		case 2:
			mixedPrice.woodPrice = 700;
			mixedPrice.crystalPrice = 700;
			break;
		case 3:
			mixedPrice.woodPrice = 1000;
			mixedPrice.crystalPrice = 1000;
			break;
		case 4:
			mixedPrice.woodPrice = 1250;
			mixedPrice.crystalPrice = 1250;
			break;
		case 5:
			mixedPrice.woodPrice = 1500;
			mixedPrice.crystalPrice = 1500;
			break;
		case 6:
			mixedPrice.woodPrice = 2000;
			mixedPrice.crystalPrice = 2000;
			break;
		case 7:
			mixedPrice.woodPrice = 4000;
			mixedPrice.crystalPrice = 4000;
			break;
		}
		return mixedPrice;
	}

	public static int GetSoldierDamage(int level)
	{
		switch (level)
		{
		case 1:
			return 2;
		case 2:
			return 4;
		case 3:
			return 8;
		case 4:
			return 16;
		case 5:
			return 25;
		case 6:
			return 35;
		case 7:
			return 55;
		default:
			return 55;
		}
	}
}
