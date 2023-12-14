using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayMenu : MonoBehaviour {
	public GameObject gameplayPanel;
	public Text currentScore;
	public Text pauseLevelNo;
	public Text pauseTimer;
	public Text gameOverLevelNo;
	public Text gameOverTimer;
	public Text gameOverHighScore;
	public Text clearScore;
	public Text clearHighScore;
	public Text clearLevelNo;
	public Text clearTimer;
	public GameObject[] scoreStars;
	public GameObject pausePanel;
	public GameObject levelClearedPanel;
	public GameObject levelFailedPanel;
	public Game_Manager game_Manager;
	bool isGameOver = false;
	int[] starsArray;
	int gameScore = 0;
	int levelStars = 0;
	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		gameplayPanel.SetActive(true);
		pausePanel.SetActive(false);
		levelClearedPanel.SetActive(false);
		levelFailedPanel.SetActive(false);
		PlayerPrefs.SetInt  ("Banner", 1);
	}
	
	// Update is called once per frame
	void Update () {
		if(game_Manager.isGameFinished && !isGameOver) {
			isGameOver = true;
			if(game_Manager.currentTime <= 60) {
				// if(!PlayerPrefs.HasKey("levelStarts"+game_Manager.levelNo))
				// 	PlayerPrefs.SetInt("levelStars"+game_Manager.levelNo, 3);
				// else {
				// 	if(PlayerPrefs.GetInt("levelStar"))
				// }
				levelStars = 3;
				gameScore = 500;
				PlayerPrefs.SetInt("GameScore", PlayerPrefs.GetInt("GameScore") + 500);
				if(PlayerPrefs.GetInt("levelsUnlocked")-1 == game_Manager.levelNo)
					PlayerPrefs.SetInt("levelsUnlocked", PlayerPrefs.GetInt("levelsUnlocked") + 1);
				StartCoroutine(wait());
			} else if(game_Manager.currentTime <= 120) {
				// 2 stars
				levelStars = 2;
				gameScore = 300;
				PlayerPrefs.SetInt("GameScore", PlayerPrefs.GetInt("GameScore") + 300);
				if(PlayerPrefs.GetInt("levelsUnlocked")-1 == game_Manager.levelNo)
					PlayerPrefs.SetInt("levelsUnlocked", PlayerPrefs.GetInt("levelsUnlocked") + 1);
				scoreStars[0].SetActive(false);
				StartCoroutine(wait());
			} else {
				// 1 star
				levelStars = 1;
				gameScore = 100;
				PlayerPrefs.SetInt("GameScore", PlayerPrefs.GetInt("GameScore") + 100);
				scoreStars[0].SetActive(false);
				scoreStars[1].SetActive(false);
				StartCoroutine(gameOverWait());
			}
			currentScore.text = "" + gameScore;
			if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) < levelStars)
				PlayerPrefs.SetInt("LevelStars"+game_Manager.levelNo, levelStars);
			for(int i=0; i<levelStars; i++)
				scoreStars[i].SetActive(false);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { 
			MainMenu();
		}
	}

	IEnumerator wait() {
		yield return new WaitForSeconds(3);
		LevelCleared();
	}
	IEnumerator gameOverWait() {
		yield return new WaitForSeconds(2);
		GameOver();
	}

	void LevelCleared() {
		PlayerPrefs.SetInt  ("Banner", -1);
		if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 3) 
			clearHighScore.text = "500";
		else if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 2) 
			clearHighScore.text = "300";
		else if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 1) 
			clearHighScore.text = "100";

		float time = game_Manager.currentTime;
		float minutes = (int) time/60;
		float seconds = time%60;
		clearScore.text = "" + gameScore;
		clearTimer.text = minutes + ":" + Mathf.RoundToInt(seconds);
		clearLevelNo.text = "" + (game_Manager.levelNo + 1);
		PlayerPrefs.SetInt  ("Intestritial", 1);
		gameplayPanel.SetActive(false);
		pausePanel.SetActive(false);
		levelClearedPanel.SetActive(true);
		levelFailedPanel.SetActive(false);
	} 

	void GameOver() {
		PlayerPrefs.SetInt  ("Banner", -1);
		if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 3) 
			gameOverHighScore.text = "500";
		else if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 2) 
			gameOverHighScore.text = "300";
		else if(PlayerPrefs.GetInt("LevelStars"+game_Manager.levelNo) == 1) 
			gameOverHighScore.text = "100";
		float time = game_Manager.currentTime;
		float minutes = (int) time/60;
		float seconds = time%60;
		gameOverTimer.text = minutes + ":" + Mathf.RoundToInt(seconds);
		gameOverLevelNo.text = "" + (game_Manager.levelNo + 1);
		PlayerPrefs.SetInt  ("Intestritial", 1);
		gameplayPanel.SetActive(false);
		pausePanel.SetActive(false);
		levelClearedPanel.SetActive(false);
		levelFailedPanel.SetActive(true);
	}
	
	public void NextLevel() {
		PlayerPrefs.SetInt("LevelNo", PlayerPrefs.GetInt("LevelNo") + 1);
		SceneManager.LoadScene(1);
	}

	public void Pause() {
		float time = game_Manager.currentTime;
		float minutes = (int) time/60;
		float seconds = time%60;
		pauseTimer.text = minutes + ":" + Mathf.RoundToInt(seconds);
		pauseLevelNo.text = "" + (game_Manager.levelNo + 1);
		Time.timeScale = 0;
		gameplayPanel.SetActive(false);
		pausePanel.SetActive(true);
		levelClearedPanel.SetActive(false);
		levelFailedPanel.SetActive(false);
	}

	public void Resume () {
		Time.timeScale = 1;
		gameplayPanel.SetActive(true);
		pausePanel.SetActive(false);
		levelClearedPanel.SetActive(false);
		levelFailedPanel.SetActive(false);
	}

	public void Restart() {
		PlayerPrefs.SetInt  ("Banner", -1);
		SceneManager.LoadScene(1);
	}

	public void MainMenu() {
		PlayerPrefs.SetInt  ("Banner", -1);
		SceneManager.LoadScene(0);
	}
}
