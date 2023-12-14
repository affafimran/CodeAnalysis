using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
	public static Tutorial _instance;

	private bool isFinished;

	[SerializeField]
	private GameObject moveTutorial;

	[SerializeField]
	private GameObject castleTutorial;

	[SerializeField]
	private GameObject resourceTutorial;

	[SerializeField]
	private GameObject nightTutorial;

	[SerializeField]
	private GameObject speedButton;

	public BuildingButton[] buildingButtons;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		if (PlayerPrefs.HasKey("TUTORIAL"))
		{
			isFinished = true;
			return;
		}
		for (int i = 0; i < buildingButtons.Length; i++)
		{
			buildingButtons[i].GetComponent<Button>().interactable = false;
		}
		moveTutorial.SetActive(value: true);
		DayNightManager._instance.timeMultiplier = 0f;
		speedButton.SetActive(value: false);
	}

	private void Update()
	{
	}

	public void OnClickMoveTutorial()
	{
		moveTutorial.SetActive(value: false);
		castleTutorial.SetActive(value: true);
		buildingButtons[0].GetComponent<Button>().interactable = true;
	}

	public void OnCastleTutorialFinished()
	{
		castleTutorial.SetActive(value: false);
		resourceTutorial.SetActive(value: true);
		buildingButtons[1].GetComponent<Button>().interactable = true;
		buildingButtons[2].GetComponent<Button>().interactable = true;
		DayNightManager._instance.timeMultiplier = 1f;
		PlayerPrefs.SetInt("TUTORIAL", 1);
		speedButton.SetActive(value: true);
	}

	public void OnResourceTutorialFinished()
	{
		resourceTutorial.SetActive(value: false);
		for (int i = 0; i < buildingButtons.Length; i++)
		{
			buildingButtons[i].GetComponent<Button>().interactable = true;
		}
	}

	public void StartNightTutorial()
	{
		nightTutorial.SetActive(value: true);
	}
}
