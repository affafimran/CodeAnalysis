using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchievementsOverviewPanel : MonoBehaviour {

	public GameObject achievementPrefab;
	public GameObject achievementsOverview;
	public GameObject achievementsBackButton;

	private Atlas hammer2AchievementsAtlas;

	// This script rebuilds all the achievements when entering the screen.
	// It gets and uses the progress regarding the medals that is gotten through the medalmanager.
	
	void Awake()
	{
		hammer2AchievementsAtlas = AtlasManager.hammer2AchievementsAtlas;
	}
	
	void OnEnable()
	{
		StartCoroutine(AchievementsOverviewPanelSequence());
	}
	
	void Update () {
		#if UNITY_ANDROID
		if (Input.GetKeyUp(KeyCode.Escape)) Scripts.interfaceScript.OnButton(achievementsBackButton);
		#endif
	}

	public void ReBuildAchievements()
	{
		// Added this function to externally rebuild this page.
		// Used for cheating to get all achievements and seeing it update.
		StartCoroutine(AchievementsOverviewPanelSequence());
	}

	private IEnumerator AchievementsOverviewPanelSequence()
	{
		Debug.Log("AchievementsOverviewPanelSequence started!");

//		achievementsBackButton.SetActive(false);

		// remove the old ones.
		foreach (Transform achievement in achievementsOverview.transform) Destroy (achievement.gameObject);

		// Setup some temporary variables for the building
		GameObject _clone;
		Transform _t;
		Vector3 _pos = new Vector3();
		
		//int columnSpacing = 380; // hardcoded now
		int rowSpacing = 80;

		GameObject _achievement;
		
		for (int i = 0; i<10; i++)
		{
			
			if (i <5) { _pos.x = -190.0f; _pos.y = 150.0f - ( i * rowSpacing);} // was -460
			if (i >=5) { _pos.x = 190.0f; _pos.y = 150.0f - ((i-5) * rowSpacing);} // was 24
			
			// Create the clones
			_clone = Instantiate(achievementPrefab, transform.position, transform.rotation) as GameObject;
			_t = _clone.GetComponent<Transform>();
			_t.SetParent(achievementsOverview.transform);
			_t.localScale = new Vector3 (1,1,1);
			_t.localPosition = _pos;
			_clone.name = "Achievement" + (i+1).ToString();
			_clone.transform.Find("Header").GetComponent<Text>().text = XLocalization.Get("Achievement" + (i+1).ToString() + "HeaderText");
			_clone.transform.Find("Description").GetComponent<Text>().text = XLocalization.Get("Achievement" + (i+1).ToString() + "DescriptionText");
			// Progress should be gotten dynamically.
			_clone.transform.Find("Progress").GetComponent<Text>().text = XLocalization.Get("AchievementProgressText") + ": " + Scripts.medalsManager.GetMedalProgression(i+1);

			Scripts.audioManager.PlaySFX("Interface/Toggle");

			yield return new WaitForSeconds(0.1f);
		}
		
		yield return new WaitForSeconds(0.1f); // needed

		// Check which achievement is unlocked.
		for (int i = 0; i<10; i++)
		{
			_achievement = GameObject.Find("Achievement" + (i+1).ToString());
			_achievement.transform.Find("Icon").GetComponent<Image>().sprite = hammer2AchievementsAtlas.Get("Achievement" + (i+1).ToString());
			if (Scripts.medalsManager.IsMedalObtained(i+1))
				{
				Debug.Log("Achievement was obtained! Hurray! Number: " + (i+1).ToString() + " game object name: " + _achievement.name);
				Scripts.audioManager.PlaySFX("Interface/Achievement");
				_achievement.transform.Find("Icon").GetComponent<Image>().enabled = true;
				}
			else _achievement.transform.Find("Icon").GetComponent<Image>().enabled = false;
			
			yield return new WaitForSeconds(0.1f);
		}

//		achievementsBackButton.SetActive(true);

		Debug.Log("AchievementsOverviewPanelSequence done!");
	}
}
