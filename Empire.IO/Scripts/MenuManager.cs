using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	[SerializeField]
	private Button loadButton;

	[SerializeField]
	private GameObject menu;

	[SerializeField]
	private GameObject loadScreen;

	[SerializeField]
	private GameObject[] tipTexts;

	[SerializeField]
	private Text daysText;

	[SerializeField]
	private GameObject exitButton;

	[SerializeField]
	private GameObject fbButton;

	private void Start()
	{
		if (PlayerPrefs.HasKey("HAS_SAVE"))
		{
			loadButton.interactable = true;
		}
		else
		{
			loadButton.interactable = false;
		}
		tipTexts[Random.Range(0, tipTexts.Length)].SetActive(value: true);
		daysText.text = "Max days survived: " + PlayerPrefs.GetInt("MAX_DAYS", 0);
	}

	public void Play(bool delete)
	{
		loadScreen.SetActive(value: true);
		menu.SetActive(value: false);
		if (delete)
		{
			PlayerPrefs.DeleteKey("HAS_SAVE");
		}
		StartCoroutine(LateLoad());
	}

	public void OpenFacebook()
	{
		Application.OpenURL("https://www.facebook.com/FMGamesStudio/");
	}

	private IEnumerator LateLoad()
	{
		yield return new WaitForSeconds(3.5f);
		SceneManager.LoadSceneAsync("Game");
	}

	public void Quit()
	{
		Application.Quit();
	}
}
