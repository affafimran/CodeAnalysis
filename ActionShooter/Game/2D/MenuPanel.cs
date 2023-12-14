using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// MenuPanel.
/// An example of a scripts that handles a bit of flow when activating a specific panel.
/// </summary> 

public class MenuPanel : MonoBehaviour {

	public GameObject brandingButton;
	public GameObject brandingBadge;
	public GameObject xformGamesPresents;
	public GameObject hammer2Logo;
	public GameObject hammer;
	public GameObject two;
	public GameObject reloaded;
	public GameObject smallPrint;
	public GameObject shopButton;
	public GameObject achievementsButton;
	public GameObject creditsButton;
	public GameObject moreGamesButton;
	public GameObject playButton;
	public GameObject menuSkipButton;
	public GameObject quitApplicationButton;

	void Start ()
	{
		Scripts.audioManager.StopAllSFX();
		Scripts.audioManager.StopAllMusic();
		// The brandingbadge is just an additional sprite on this screen.
		// If no brandingbadge is localized (and put in the Branding folder), the badge will not be visible.
		if(brandingBadge.GetComponent<Image>().sprite == null) brandingBadge.GetComponent<Image>().enabled = false;
		foreach (Transform t in this.transform) t.gameObject.SetActive(false);
		StartCoroutine("MenuPanelSequence");
	}
	
	void Update	()
	{
		if (Input.GetButtonUp("Submit") && menuSkipButton.activeSelf) SkipMenuPanelSequence();
	}

	public IEnumerator MenuPanelSequence()
	{
		Debug.Log("[MenuPanel] MenuPanelSequence started");

		Scripts.audioManager.PlayMusic("MainTheme", Data.Shared["Misc"].d["MusicVolume"].f, -1);

		yield return 0; // Waiting for the old camera setting to be done

		CameraManager.UpdateSettings("Menu", GameObject.Find("MenuCamera_Dummy"));

		if (GameData.skipMenuPanelSequence)
		{
			SkipMenuPanelSequence();
			yield break;
		}
		
		menuSkipButton.SetActive(true);

		yield return new WaitForSeconds(2.5f);
		
		xformGamesPresents.SetActive(true);

		yield return new WaitForSeconds(2.5f);
		hammer.SetActive(true);
		CameraManager.activeCameraData.shakeIntensity = 2.0f;
		Scripts.interfaceScript.Fade("FadeWhiteShort");
		Scripts.audioManager.PlaySFX("Weapons/MagnumFire");

		yield return new WaitForSeconds(0.5f);
		two.SetActive(true);
		CameraManager.activeCameraData.shakeIntensity = 2.0f;
		Scripts.interfaceScript.Fade("FadeWhiteShort");
		Scripts.audioManager.PlaySFX("Weapons/MagnumFire");

		yield return new WaitForSeconds(0.5f);
		Scripts.audioManager.PlaySFX("Interface/PickUpAmmo");

		yield return new WaitForSeconds(1.5f);
		reloaded.SetActive(true);
		CameraManager.activeCameraData.shakeIntensity = 3.0f;
		Scripts.interfaceScript.Fade("FadeWhiteFlash");
		Scripts.audioManager.PlaySFX("Weapons/FiftyCaliberFire");
		Scripts.audioManager.PlaySFX("Effects/JetPackIgnition");

		yield return new WaitForSeconds(2.5f);

		playButton.SetActive(true);
		Scripts.interfaceScript.SetSelectedGameObject(playButton);

		brandingButton.SetActive(true);

		brandingBadge.SetActive(true);
		creditsButton.SetActive(true);
		achievementsButton.SetActive(true);

		shopButton.SetActive(true);

		smallPrint.SetActive(true);

		Scripts.interfaceScript.optionsPanel.SetActive(true);

		ChangeNavigation();

		menuSkipButton.SetActive(false);

		GameData.skipMenuPanelSequence = true;

		Debug.Log("[MenuPanel] MenuPanelSequence ended");
	}

	public void SkipMenuPanelSequence()
	{
		menuSkipButton.SetActive(false);

		StopAllCoroutines();

		CameraManager.activeCameraData.shakeIntensity = 3.0f;
		Scripts.interfaceScript.Fade("FadeWhiteFlash");
		Scripts.audioManager.PlaySFX("Weapons/FiftyCaliberFire");
		Scripts.audioManager.PlaySFX("Effects/JetPackIgnition");

		xformGamesPresents.SetActive(true);
		hammer.SetActive(true);
		two.SetActive(true);
		reloaded.SetActive(true);
		playButton.SetActive(true);
		Scripts.interfaceScript.SetSelectedGameObject(playButton);
		brandingButton.SetActive(true);
		brandingBadge.SetActive(true);
		creditsButton.SetActive(true);
		achievementsButton.SetActive(true);
		moreGamesButton.SetActive(true);
		shopButton.SetActive(true);
		smallPrint.SetActive(true);
		Scripts.interfaceScript.optionsPanel.SetActive(true);
		ChangeNavigation();
		GameData.skipMenuPanelSequence = true;


		// Advertising
		if (Scripts.advertising.forceShowInterstitial){
			Scripts.advertising.ShowInterstitial();
			Scripts.advertising.forceShowInterstitial = false;
		}
			
	}

	// Hack: Determines to show a Quit or a More Games button.
	// Necessary to dynamically change where some buttons take you for controller support!
	private void ChangeNavigation()
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
		{
			moreGamesButton.SetActive(false);
			quitApplicationButton.SetActive(true); // Only show the quit button in standalone player for now.
		}
		else
		{
			moreGamesButton.SetActive(true);
			quitApplicationButton.SetActive(false);
		}

		// I wish this could be done easier... I might write some sort of helper for this later on....

		Button tempButton;
		Navigation tempNav = new Navigation();
		tempNav.mode = Navigation.Mode.Explicit;

		// Changing the achievementsbutton....
		tempButton = achievementsButton.GetComponent<Button>();
		tempNav.selectOnUp = playButton.GetComponent<Button>();
		tempNav.selectOnDown = null;
		tempNav.selectOnLeft = creditsButton.GetComponent<Button>();
		if (moreGamesButton.activeSelf) tempNav.selectOnRight = moreGamesButton.GetComponent<Button>();
		if (quitApplicationButton.activeSelf) tempNav.selectOnRight = quitApplicationButton.GetComponent<Button>();
		tempButton.navigation = tempNav;

		// Changing the shopbutton....
		tempButton = shopButton.GetComponent<Button>();
		tempNav.mode = Navigation.Mode.Explicit;
		tempNav.selectOnUp = Scripts.interfaceScript.optionsPanel.GetComponent<OptionsPanel>().settingsButton.GetComponent<Button>(); // Nice...
		if (moreGamesButton.activeSelf) tempNav.selectOnDown = moreGamesButton.GetComponent<Button>();
		if (quitApplicationButton.activeSelf) tempNav.selectOnDown = quitApplicationButton.GetComponent<Button>();
		tempNav.selectOnLeft = playButton.GetComponent<Button>();
		tempNav.selectOnRight = null;
		tempButton.navigation = tempNav;
	}
}
