using UnityEngine;

public class ProduceCurrency : MonoBehaviour
{
	public float timeToProduce = 5f;

	private float timer = 3f;

	public int productionAmount;

	public GameObject productionPrefab;

	public CurrencyManager.CurrencyType type;

	private void Start()
	{
	}

	private void Update()
	{
		if (GameManager._instance.isPaused)
		{
			return;
		}
		timer += Time.deltaTime;
		if (!DayNightManager._instance.isNight && timer > timeToProduce / Mathf.Max(1f, DayNightManager._instance.timeMultiplier / 2f))
		{
			timer = 0f;
			if (type == CurrencyManager.CurrencyType.CRYSTAL)
			{
				CurrencyManager._instance.AddCrystal(productionAmount);
			}
			if (type == CurrencyManager.CurrencyType.WOOD)
			{
				CurrencyManager._instance.AddWood(productionAmount);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(productionPrefab);
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.GetComponent<ResourceAnim>().value = productionAmount;
		}
	}
}
