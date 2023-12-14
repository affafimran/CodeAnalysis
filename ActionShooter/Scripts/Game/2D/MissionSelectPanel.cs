using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// MissionSelectPanel.
/// Instancing a heap of mission select buttons.
/// Making sure each one gets the correct data inserted.
/// </summary> 

public class MissionSelectPanel : MonoBehaviour {

	public GameObject missionButtonPrefab;
	public GameObject missionLockedButtonPrefab;

	public GameObject missionList;
	public GameObject missionSelectBackButton;

	public GameObject previousPageButton;
	public GameObject nextPageButton;

	private MissionData missionData;
	private Atlas hammer2TargetsAtlas;

	private int currentPage;
	private int totalPages;

	void Awake()
	{
		// Need a reference to the atlas to display the correct images for each mission.
		hammer2TargetsAtlas = AtlasManager.hammer2TargetsAtlas;
		missionData = new MissionData();

		// We're determining later on which page to really start depending on progress.
		currentPage = 1;
		totalPages = 3;
	}

	void Start()
	{
		currentPage = 1; // (DG) Not sure why this is here in Start as well.
		if (GameData.mission>10) currentPage = 2;
		if (GameData.mission>20) currentPage = 3;
		StartCoroutine(MissionSelectPanelSequence());
	}
	
	// This panel has it's own button handling.
	// Buttons are referred here from the InterfaceScript.
	public void OnClick(GameObject aClickObject){ OnButton(aClickObject.name);}
	public void OnButton(string aButtonName)
	{
		switch (aButtonName)
		{
			case "NextPageButton":
			currentPage = Mathf.Min(currentPage+1, totalPages);
			ReBuildMissions();
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "PreviousPageButton":
			currentPage = Mathf.Max(currentPage-1, 1);
			ReBuildMissions();
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			default: Debug.Log("MissionSelectPanel OnButton cannot find button name!"); break;
		}

		if (!aButtonName.Contains("Button"))
		{
			GameData.mission = int.Parse(aButtonName);
			Scripts.interfaceScript.EndMenu();
			Scripts.audioManager.PlaySFX("Interface/Select");
			Scripts.audioManager.PlaySFX("Interface/StartGame");
		}
	}

	public void ReBuildMissions()
	{
		StartCoroutine(MissionSelectPanelSequence());
	}

	private IEnumerator MissionSelectPanelSequence()
	{
		Debug.Log("MissionSelectPanelSequence started on page: " + currentPage.ToString());

		nextPageButton.SetActive(false);
		previousPageButton.SetActive(false);
		missionSelectBackButton.SetActive(false);
		
		// remove the old ones.
		foreach (Transform mission in missionList.transform) Destroy (mission.gameObject);
		
		// Setup some temporary variables for the building
		GameObject _original; 
		GameObject _clone;
		Transform _t;
		Vector3 _scale = new Vector3 (0.8f,0.8f,0.8f); // (DG) Maybe we'll make it even smaller to fit more on one page.
		Vector3 _pos = new Vector3();
		
		//int columnSpacing = 380; // hardcoded now
		int rowSpacing = 83;
		int missionNumber = 0;
		int missionCount = 10; // Per page
		bool missionLocked = false;

		for (int i = 0; i < missionCount; i++)
		{
			missionNumber = (i + 1) + ((currentPage*10) - 10); // Calculate which exact mission depending on the page and location.

			// Get the position of the button
			if (i <5) { _pos.x = -190.0f; _pos.y = 150.0f - ( i * rowSpacing);} // was -460
			if (i >=5) { _pos.x = 190.0f; _pos.y = 150.0f - ((i-5) * rowSpacing);} // was 24

			// Check if this mission is locked.
			missionLocked = (missionNumber > GameData.unlockedMissions);

			// Check if we should build the locked or regular version of the button
			_original = (missionLocked)? missionLockedButtonPrefab : missionButtonPrefab;

			// Create the clone
			_clone = Instantiate(_original, transform.position, transform.rotation) as GameObject;
			_t = _clone.GetComponent<Transform>();
			_t.SetParent(missionList.transform);
			_t.localScale = _scale;
			_t.localPosition = _pos;
			_clone.name = missionNumber.ToString(); // (DG) Not really a proper name but easy to delimit.
			_clone.transform.Find("Number").GetComponent<Text>().text = missionNumber.ToString();
			_clone.transform.Find("Header").GetComponent<Text>().text = XLocalization.Get("Mission" + missionNumber.ToString() + "HeaderText");
			_clone.transform.Find("Description").GetComponent<Text>().text = (missionLocked)? XLocalization.Get("MissionLockedDescriptionText"): XLocalization.Get("Mission" + missionNumber.ToString() + "DescriptionText");

			missionData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Missions"].d["Mission"+ missionNumber.ToString()].d, missionData) as MissionData;

			// Set the target icon.
			_clone.transform.Find("Target").GetComponent<Image>().sprite = hammer2TargetsAtlas.Get(missionData.target);

			// Make the last one the New one. This is de-activated below in checking the progress of each mission.
			if (missionNumber == GameData.unlockedMissions)_clone.transform.Find("New").gameObject.SetActive(true); // (DG) FYI, only the regular buttons have the New child.

			// Check if we made any progress for this mission and enable the icons if so. 
			MissionData missionProgressData = MissionManager.GetMissionByID(missionNumber);
			if (missionProgressData != null)
			{
				bool missionCompleted = missionProgressData.completed;
				bool targetScoreMet = (missionProgressData.totalScore >= missionProgressData.targetScore);
				bool targetTimeMet = (missionProgressData.totalTime <= missionProgressData.targetTime);
				bool hiddenPackageFound = missionProgressData.hiddenPackage;
				_t.Find("Check").gameObject.SetActive(missionCompleted && !targetScoreMet);
				_t.Find("Trophy").gameObject.SetActive(missionCompleted && targetScoreMet);
				_t.Find("Timer").gameObject.SetActive(missionCompleted && targetTimeMet);
				_t.Find("HiddenPackage").gameObject.SetActive(hiddenPackageFound);
				_t.Find("New").gameObject.SetActive(false);
				Debug.Log ("Mission " + missionProgressData.mission + ": " + missionProgressData.score +  " / " + missionProgressData.targetScore + ", " + missionProgressData.totalTime +  " / " + missionProgressData.targetTime + ", " + missionProgressData.hiddenPackage );
			}

			Scripts.audioManager.PlaySFX("Interface/Toggle");
			yield return new WaitForSeconds(0.1f);
		}

		nextPageButton.SetActive((currentPage<totalPages));
		previousPageButton.SetActive((currentPage!=1));
		missionSelectBackButton.SetActive(true);

		GameObject selectedButton = GameObject.Find(GameData.mission.ToString());
		if (selectedButton == null) selectedButton = GameObject.Find("MissionSelectBackButton"); // (DG) hack in case the mission is not on this page.
		Scripts.interfaceScript.SetSelectedGameObject(selectedButton); // Select a button

		Debug.Log("MissionSelectPanelSequence done!");
	}
}
