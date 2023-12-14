using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
	public static WaveManager _instance;

	[SerializeField]
	private GameObject victoryWindow;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void StartWave()
	{
		if (DayNightManager._instance.dayNum == 1)
		{
			Tutorial._instance.StartNightTutorial();
		}
		EnemySpawner._instance.StartWave();
	}

	public IEnumerator Victory()
	{
		MessageHandler._instance.ShowMessage("Day " + DayNightManager._instance.dayNum + " completed", 1f, Color.green);
		victoryWindow.SetActive(value: true);
		yield return new WaitForSeconds(2f);
		victoryWindow.SetActive(value: false);
	}
}
