using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	public GameObject levelLoadingScreen;
	public GameObject[] levelsPanels;
	public GameObject SettingsPanel;
	public Image[] levelButtons;

	public Image volumeButton, vibrationButton;
	public Sprite[] volumeImgs;
	public Sprite[] vibrationImgs;
	public Sprite[] levelButtonsImages;
	public GameObject loadingPanel;
	public GameObject playPanel;
	public GameObject levelSelectionPanel;
	public GameObject quitPanel;
	public GameObject modePanel;
	public GameObject comingSoonPanel;
	public float timer;
	public Text loadingText;
	public Text starsText;
	public Text coinsText;
	public Text cashText;
	//public FBManager fbManagerObj;
	static int checkStart = 0;
	bool coroutineStarts = false;
	float progressValue = 0;
	// Use this for initialization
	void Start () {
		//fbManagerObj = gameObject.GetComponent<FBManager>();
		if(!PlayerPrefs.HasKey("FBLoggedIn"))
			PlayerPrefs.SetInt("FBLoggedIn", 0);

		if(!PlayerPrefs.HasKey("levelsUnlocked"))
			PlayerPrefs.SetInt("levelsUnlocked", 1);

		if(!PlayerPrefs.HasKey("rollingBallVolume"))
			PlayerPrefs.SetInt("rollingBallVolume", 1);

		if(!PlayerPrefs.HasKey("rollingBallVibration"))
			PlayerPrefs.SetInt("rollingBallVibration", 1);

		if(PlayerPrefs.GetInt("rollingBallVolume") == 1) 
			volumeButton.sprite = volumeImgs[0];
		else 
			volumeButton.sprite = volumeImgs[1];
		
		if(PlayerPrefs.GetInt("rollingBallVibration") == 1)
			vibrationButton.sprite = vibrationImgs[0];
		else 
			vibrationButton.sprite = vibrationImgs[1];

		if(!PlayerPrefs.HasKey("LevelStars0"))
			for(int i=0; i<15; i++) 
				PlayerPrefs.SetInt("LevelStars"+i, 0);

		if(!PlayerPrefs.HasKey("LevelNo"))
			PlayerPrefs.SetInt("LevelNo", 0);

		for(int j=0; j<levelButtons.Length; j++){
			if(PlayerPrefs.GetInt("levelsUnlocked") > j) {
				if(PlayerPrefs.GetInt("LevelStars"+j) == 0)
					levelButtons[j].sprite = levelButtonsImages[0];
				else if(PlayerPrefs.GetInt("LevelStars"+j) == 1)
					levelButtons[j].sprite = levelButtonsImages[1];
				else if(PlayerPrefs.GetInt("LevelStars"+j) == 2)
					levelButtons[j].sprite = levelButtonsImages[2];
				else if(PlayerPrefs.GetInt("LevelStars"+j) == 3)
					levelButtons[j].sprite = levelButtonsImages[3];
			}
			else 
				levelButtons[j].sprite = levelButtonsImages[4];
		}
		
		if(checkStart == 0) {
			loadingPanel.SetActive(true);
			playPanel.SetActive(false);
			levelSelectionPanel.SetActive(false);
			quitPanel.SetActive(false);
			modePanel.SetActive(false);
			comingSoonPanel.SetActive(false);
			levelLoadingScreen.SetActive(false);	
			StartCoroutine(wait());
		} 
		else {
			loadingPanel.SetActive(false);
			playPanel.SetActive(false);
			levelSelectionPanel.SetActive(true);
			quitPanel.SetActive(false);
			modePanel.SetActive(false);
			comingSoonPanel.SetActive(false);
			levelLoadingScreen.SetActive(false);
		}
		if(checkStart == 0)
			checkStart = 1;
	
		int starsWon = 0;
		for(int i=0; i<15; i++) 
			starsWon += PlayerPrefs.GetInt("LevelStars"+i);
		starsText.text = starsWon + "/45";
		coinsText.text = "" + (starsWon * 10);
		cashText.text = "" + (starsWon * 60);
	}

	IEnumerator wait() {
		coroutineStarts = true;
		yield return new WaitForSeconds(4);
		if(Application.internetReachability != NetworkReachability.NotReachable && PlayerPrefs.GetInt("FBLoggedIn") == 1) {
			Nothing();
		}
		else {
			// Nothing
			loadingPanel.SetActive(false);
			playPanel.SetActive(true);
			levelSelectionPanel.SetActive(false);
			quitPanel.SetActive(false);
			modePanel.SetActive(false);
		}
		// loadingPanel.SetActive(false);
		// playPanel.SetActive(true);
		// levelSelectionPanel.SetActive(false);
		// quitPanel.SetActive(false);
		// modePanel.SetActive(false);
		coroutineStarts = false;
	}

	IEnumerator checkInternetConnection(){
		WWW www = new WWW("http://google.com");
		yield return www;
		if (www.error != null) {
			loadingPanel.SetActive(false);
			playPanel.SetActive(false);
			levelSelectionPanel.SetActive(false);
			quitPanel.SetActive(false);
			modePanel.SetActive(true);
		} else {
			// Nothing
			loadingPanel.SetActive(false);
			playPanel.SetActive(true);
			levelSelectionPanel.SetActive(false);
			quitPanel.SetActive(false);
			modePanel.SetActive(false);
		}
	}

	public void PlayGame() {
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(true);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(false);
	}
    public void PlayMenu()
    {
        loadingPanel.SetActive(false);
        playPanel.SetActive(false);
        levelSelectionPanel.SetActive(true);
        quitPanel.SetActive(false);
        modePanel.SetActive(false);
        comingSoonPanel.SetActive(false);
        levelLoadingScreen.SetActive(false);
    }

    public void LevelLoadingScreen() {
		// PlayerPrefs.SetInt  ("Intestritial", 1);
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(false);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(true);
	}

	public void LoadLevel(int levelNo) {
		PlayerPrefs.SetInt  ("Banner", -1);
		if(levelNo < PlayerPrefs.GetInt("levelsUnlocked")) {
			LevelLoadingScreen();
			PlayerPrefs.SetInt("LevelNo", levelNo);
			SceneManager.LoadScene(1);
		}
		else {
			// Nothing
		}
	}

	public void CloseGame() {
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(true);
		modePanel.SetActive(false);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(false);
	}

	public void ModePanel(string user) {
		//fbManagerObj.playerDetails();
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(true);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(false);
	}

	public void ComingSoon() {
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(true);
		comingSoonPanel.SetActive(true);
		levelLoadingScreen.SetActive(false);
	}

	public void Quit() {
		Application.Quit();
	}

	public void Nothing() {
		//fbManagerObj.playerDetails();
		loadingPanel.SetActive(false);
		playPanel.SetActive(false);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(true);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(false);
	}

	public void PlayPanel() {
		PlayerPrefs.SetInt  ("Banner",1);
		loadingPanel.SetActive(false);
		playPanel.SetActive(true);
		levelSelectionPanel.SetActive(false);
		quitPanel.SetActive(false);
		modePanel.SetActive(false);
		comingSoonPanel.SetActive(false);
		levelLoadingScreen.SetActive(false);
	}

	public void SettingBtn() {
		if(SettingsPanel.activeInHierarchy)
			SettingsPanel.SetActive(false);
		else
			SettingsPanel.SetActive(true);
	}

	public void VolumeBtn() {
		SettingsPanel.SetActive(false);
		if(PlayerPrefs.GetInt("rollingBallVolume") == 1) {
			volumeButton.sprite = volumeImgs[1];
			PlayerPrefs.SetInt("rollingBallVolume", 0);
		} else {
			volumeButton.sprite = volumeImgs[0];
			PlayerPrefs.SetInt("rollingBallVolume", 1);
		}
	}

	public void VibrationBtn() {
		SettingsPanel.SetActive(false);
		if(PlayerPrefs.GetInt("rollingBallVibration") == 1) {
			vibrationButton.sprite = vibrationImgs[1];
			PlayerPrefs.SetInt("rollingBallVibration", 0);
		} else {
			vibrationButton.sprite = vibrationImgs[0];
			PlayerPrefs.SetInt("rollingBallVibration", 1);
		}
	}

	public void NextLevelPanel() {
		if(levelsPanels[0].activeInHierarchy) {
			levelsPanels[0].SetActive(false);
			levelsPanels[1].SetActive(true);
			levelsPanels[2].SetActive(false);
		} else if(levelsPanels[1].activeInHierarchy) {
			levelsPanels[0].SetActive(false);
			levelsPanels[1].SetActive(false);
			levelsPanels[2].SetActive(true);
		} else if(levelsPanels[2].activeInHierarchy) {
			levelsPanels[0].SetActive(false);
			levelsPanels[1].SetActive(false);
			levelsPanels[2].SetActive(true);
		} 
	}

	public void PreviousLevelPanel() {
		if(levelsPanels[0].activeInHierarchy) {
			levelsPanels[0].SetActive(true);
			levelsPanels[1].SetActive(false);
			levelsPanels[2].SetActive(false);
		} else if(levelsPanels[1].activeInHierarchy) {
			levelsPanels[0].SetActive(true);
			levelsPanels[1].SetActive(false);
			levelsPanels[2].SetActive(false);
		} else if(levelsPanels[2].activeInHierarchy) {
			levelsPanels[0].SetActive(false);
			levelsPanels[1].SetActive(true);
			levelsPanels[2].SetActive(false);
		} 
	}

	// Update is called once per frame
	void Update () {
		if(coroutineStarts) {
			progressValue = progressValue + Time.deltaTime * 4;
			loadingText.text = "Loading..." + Mathf.RoundToInt(progressValue*6) + "%";
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Application.Quit(); 
		}
	}
}
