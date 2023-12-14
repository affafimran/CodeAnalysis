using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ResultsPanel.
/// Showing how you've done.
/// Showing the buttons at the end of the game.
/// </summary> 

public class ResultsPanel : MonoBehaviour {

	public GameObject filter;
	public GameObject brandingButton;
	public GameObject cash;
	public GameObject time;

	private Text timeValueText;

	public GameObject header;
	public GameObject comment;
	public GameObject score;
	public GameObject newHighscore;

	public GameObject quitButton;
	public GameObject continueButton;
	public GameObject submitButton;

	public GameObject endGameButton;
	
	public GameObject progress;

	void Start ()
	{
		Scripts.audioManager.StopAllSFX();
		Scripts.audioManager.StopAllMusic();

		foreach (Transform t in this.transform) t.gameObject.SetActive(false);

		StartCoroutine("ResultsPanelSequence");
	}
	
	public IEnumerator ResultsPanelSequence()
	{
		Debug.Log("[ResultsPanel] ResultsPanelSequence started");

		filter.SetActive(true);

		header.SetActive(true);

		Scripts.audioManager.PlaySFX("Interface/Results");
		Scripts.audioManager.PlayMusic("Results", Data.Shared["Misc"].d["MusicVolume"].f, -1);

		yield return new WaitForSeconds(1.5f);

		comment.SetActive(true);
		// if (MissionManager.missionData.targetScore < MissionManager.missionData.score)
		string commentKey = "ResultsComment" + Random.Range(1,6).ToString() + "Text";
		comment.GetComponent<Text>().text = XLocalization.Get(commentKey);
		Scripts.audioManager.PlaySFX("Interface/Achievement");

		yield return new WaitForSeconds(1.0f);

		score.SetActive(true);
		score.transform.Find("ScoreValue").GetComponent<Text>().text = MissionManager.missionData.score.ToString(); // CURRENT GAINED SCORE
		Scripts.audioManager.PlaySFX("Interface/ScoreLarge");

		if (GameData.highscore < MissionManager.missionData.totalScore) // here we DO use totalScore, since it could be updated in CompleteMission()
		{
			// not the greatest but alas,...
			GameData.highscore = MissionManager.missionData.score; 
			yield return new WaitForSeconds(1.0f);
			newHighscore.SetActive(true);
			Scripts.audioManager.PlaySFX("Interface/HighScore");
		}

		time.gameObject.SetActive(true);
		timeValueText = time.transform.Find("Value").GetComponent<Text>();
		timeValueText.text = GenericFunctionsScript.ConvertTimeToStringMSS(MissionManager.missionData.time); // CURRENT TIME TAKEN

		yield return new WaitForSeconds(1.0f);

		yield return StartCoroutine(CheckProgressSequence());

		cash.SetActive(true);
		cash.transform.Find("CashValue").GetComponent<Text>().text =  "$" + GenericFunctionsScript.AddSeparatorInInt(GameData.cash, ","); 
		Scripts.audioManager.PlaySFX("Interface/Buy");

		yield return new WaitForSeconds(1.0f);

		brandingButton.SetActive(true);

		// Advertising!
		submitButton.SetActive(false);
		//if (Application.isEditor) submitButton.SetActive(true); // DEBUG
		if (GameData.mobile){
			if(Scripts.advertising.hasRewardedVideo) submitButton.SetActive(true);
			else Scripts.advertising.ShowInterstitial();
		}

		if (MissionManager.missionData.mission == GameData.loopMissionsAt)
		{
			quitButton.SetActive(false);
			continueButton.SetActive(false);
			endGameButton.SetActive(true);
		}
		else
		{
			quitButton.SetActive(true);
			continueButton.SetActive(true);
			endGameButton.SetActive(false);
		}

		Scripts.interfaceScript.SetSelectedGameObject(continueButton);

		// PIETER. We now save here instead instantly after a mission. 
		UserData.Save();

		Debug.Log("[ResultsPanel] ResultsPanelSequence ended");
	}

	public void UpdateCash(){
		cash.SetActive(true);
		cash.transform.Find("CashValue").GetComponent<Text>().text =  "$" + GenericFunctionsScript.AddSeparatorInInt(GameData.cash, ","); 
		Scripts.audioManager.PlaySFX("Interface/Buy");
	}

	// For the challenges.
	// Similar to the one we have in InterfaceScript, so that could be merged into one thing one day.
	private IEnumerator CheckProgressSequence()
	{
		progress.SetActive(true);
	
		GameObject targetTime = progress.transform.Find("TargetTime").gameObject;
		GameObject targetTimeText = targetTime.transform.Find("Text").gameObject;
		GameObject targetScore = progress.transform.Find("TargetScore").gameObject;
		GameObject targetScoreText = targetScore.transform.Find("Text").gameObject;
		GameObject hiddenPackage = progress.transform.Find("HiddenPackage").gameObject;
		GameObject hiddenPackageText = hiddenPackage.transform.Find("Text").gameObject;

		foreach (Transform t in progress.transform) t.gameObject.SetActive(false);

		MissionData missionProgressData = MissionManager.missionData;

		yield return new WaitForSeconds(1.0f);

		bool targetScoreMet = (missionProgressData != null)? (missionProgressData.totalScore >= missionProgressData.targetScore) : false;
		if (targetScoreMet) yield return new WaitForSeconds(0.5f);
		targetScore.gameObject.SetActive(true);
		targetScoreText.GetComponent<Text>().text =  (targetScoreMet) ? XLocalization.Get("BeatScoreSuccesText") : XLocalization.Get("BeatScoreFailureText");
		targetScoreText.GetComponent<Text>().color = (targetScoreMet)? Color.white : Color.grey;
		targetScore.transform.Find("Icon").gameObject.SetActive(targetScoreMet);
		targetScore.transform.Find("Circle").gameObject.SetActive(!targetScoreMet);
		Scripts.audioManager.PlaySFX("Interface/Trophy");

		bool targetTimeMet = (missionProgressData != null)? (missionProgressData.totalTime <= missionProgressData.targetTime) : false;
		if (targetTimeMet) yield return new WaitForSeconds(0.5f);
		targetTime.gameObject.SetActive(true);
		targetTimeText.GetComponent<Text>().text = (targetTimeMet) ? XLocalization.Get("BeatTimeSuccesText") : XLocalization.Get("BeatTimeFailureText");
		targetTimeText.GetComponent<Text>().color = (targetTimeMet)? Color.white : Color.grey;
		targetTime.transform.Find("Icon").gameObject.SetActive(targetTimeMet);
		if (targetTimeMet) Scripts.audioManager.PlaySFX("Interface/Trophy");

		bool hiddenPackageFound = (missionProgressData != null)? missionProgressData.hiddenPackage : false;
		if (hiddenPackageFound) yield return new WaitForSeconds(0.5f);
		hiddenPackage.gameObject.SetActive(true);
		hiddenPackageText.GetComponent<Text>().text = (hiddenPackageFound) ? XLocalization.Get("FindPackageSuccesText") : XLocalization.Get("FindPackageFailureText");
		hiddenPackageText.GetComponent<Text>().color = (hiddenPackageFound) ? Color.white : Color.grey;
		hiddenPackage.transform.Find("Icon").gameObject.SetActive(hiddenPackageFound);
		if (hiddenPackage) Scripts.audioManager.PlaySFX("Interface/Trophy");
	}
}
