using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GameOverPanel.
/// Displays buttons when you are dead.
/// </summary>

public class GameOverPanel : MonoBehaviour {

	public GameObject background;
	public GameObject filter;
	public GameObject brandingButton;
	public GameObject header;
	public GameObject comment;
	public GameObject quitButton;
	public GameObject retryButton;


	void Start ()
	{
		Scripts.audioManager.StopAllSFX();
		Scripts.audioManager.StopAllMusic();

		foreach (Transform t in this.transform) t.gameObject.SetActive(false);
		
		StartCoroutine("GameOverPanelSequence");
	}

	public IEnumerator GameOverPanelSequence()
	{
		Debug.Log("GameOverPanelSequence started");

		background.SetActive(true);
//		filter.SetActive(true);
		
		header.SetActive(true);
		Scripts.audioManager.PlaySFX("Interface/GameOver");
		
		yield return new WaitForSeconds(1.5f);
		
//		Scripts.audioManager.PlayMusic("Results", Data.Shared["Misc"].d["MusicVolume"].f, -1);
		
		comment.SetActive(true);
		// if (MissionManager.missionData.targetScore < MissionManager.missionData.score)
		string commentKey = "GameOverComment" + Random.Range(1,6).ToString() + "Text";
		comment.GetComponent<Text>().text = XLocalization.Get(commentKey);
		Scripts.audioManager.PlaySFX("Interface/UnEquip");
		
		yield return new WaitForSeconds(1.0f);
		
		brandingButton.SetActive(true);
		
		quitButton.SetActive(true);
		retryButton.SetActive(true);

		Scripts.interfaceScript.SetSelectedGameObject(retryButton);

		// Show interstitial
		Scripts.advertising.ShowInterstitial();

		Debug.Log("GameOverPanelSequence ended");
	}
}