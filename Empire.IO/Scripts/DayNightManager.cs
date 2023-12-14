using UnityEngine;
using UnityEngine.UI;

public class DayNightManager : MonoBehaviour
{
	public static DayNightManager _instance;

	[SerializeField]
	private Transform dayNightTransform;

	[SerializeField]
	private Image fogImage;

	public int dayNum = 1;

	[SerializeField]
	private Text dayNumText;

	public float time;

	[SerializeField]
	private Button speedButton;

	[SerializeField]
	private Button speedButton2;

	private float offset;

	public float timeMultiplier = 1f;

	public bool isNight;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		dayNumText.text = "DAY " + dayNum;
		offset = dayNightTransform.localEulerAngles.z;
	}

	private void Update()
	{
		if (!GameManager._instance.isPaused)
		{
			time += Time.deltaTime * timeMultiplier;
			if (isNight)
			{
				time += Time.deltaTime * timeMultiplier;
			}
			HandleTime();
			CheckTimeEvents();
		}
	}

	private void HandleTime()
	{
		dayNightTransform.localEulerAngles = Vector3.forward * (0f - time + offset);
		if (time > 160f && time < 180f)
		{
			fogImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, (time - 160f) / 20f));
		}
		else if (time > 330f && time <= 360f)
		{
			fogImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, (time - 330f) / 30f));
		}
		if (time > 360f)
		{
			dayNum++;
			dayNumText.text = "DAY " + dayNum;
			time = 0f;
		}
	}

	private void CheckTimeEvents()
	{
		if (time > 180f && !isNight)
		{
			speedButton.interactable = false;
			speedButton2.interactable = false;
			timeMultiplier = 0.6f;
			isNight = true;
			WaveManager._instance.StartWave();
			PlayerPrefs.SetInt("MAX_DAYS", Mathf.Max(dayNum, PlayerPrefs.GetInt("MAX_DAYS")));
		}
		else if (time < 180f && isNight)
		{
			speedButton.interactable = true;
			speedButton2.interactable = true;
			timeMultiplier = 1f;
			isNight = false;
			foreach (Building allPlacedBuilding in GameManager._instance.allPlacedBuildings)
			{
				allPlacedBuilding.Heal();
			}
			foreach (Enemy enemy in EnemySpawner._instance.enemies)
			{
				StartCoroutine(enemy.Burn());
			}
			SaveManager._instance.Save();
		}
	}

	public void FastenTimeSpeed()
	{
		if (!isNight)
		{
			if (timeMultiplier == 1f)
			{
				timeMultiplier = 20f;
			}
			else
			{
				timeMultiplier = 1f;
			}
		}
	}

	public void SetText()
	{
		dayNumText.text = "Day " + dayNum;
	}
}
