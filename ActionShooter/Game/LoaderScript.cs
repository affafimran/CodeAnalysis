using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LoaderScript : MonoBehaviour
{	
	private GameObject preloaderPanel;
	private Transform loadingBarFillTransform;
	private Text loadingBarText;
	private float loadingProgress;

	/// <summary>
	/// This script handles the preloading of the game files as WebPlayer, so players can look at a nice custom loading screen.
	/// For this to work, you need to tick 'Streamed' in the Build settings
	/// In the Player settings, you need to check for which Level the Resources should be loaded.
	/// The Resources contain the most data, since almost all objects in the scenes are prefabs or dynamically instantiated.
	/// In this case we've set it to level 3 (the actual game scene).
	/// IMPORTANT: If you want to use something from the resources BEFORE the resources are loaded, this will not work!
	/// You'll have to manually create a dependency inside this scene or the Startup scene. This way it's saved and loaded along with the scene.
	/// We did that for the localization and branding files. Pretty unhandy.
	/// </summary>

	void Awake()
	{
		Debug.Log("[LoaderScript] Awake called");

		if (GameObject.Find("StartUp") == null)
		{
			Debug.LogWarning("[LoaderScript] Switching to StartUp scene!");
			gameObject.SetActive(false);
			Application.LoadLevel("StartUp");
			return;
		}

		preloaderPanel = transform.Find("PreloaderPanel").gameObject;
		loadingBarFillTransform = preloaderPanel.transform.Find("LoadingBar/Fill");
		loadingBarText = preloaderPanel.transform.Find("LoadingBar/Text").GetComponent<Text>();
		loadingBarFillTransform.localScale = new Vector3(0.0f, 1.0f, 1.0f); // Resetting the loading bar
		preloaderPanel.SetActive(false); // (DG) Intially not showing it, in case we want to skip it all together.
	}

	void Start()
	{
		Debug.Log("[LoaderScript] Start called");
		if (Data.preloader)
		{
			SetUpButtonsInHierarchy(gameObject);
			StartCoroutine(LoadLevelSequence());
		}
		else Application.LoadLevel("Game");
	}

	private IEnumerator LoadLevelSequence() {

		Debug.Log("[LoaderScript] LoadLevelSequence started");

		preloaderPanel.SetActive(true);
		preloaderPanel.transform.Find("FadeIn").gameObject.SetActive(true);
		loadingBarText.text = XLocalization.Get("LoadingText");

		yield return new WaitForSeconds(1.0f);

		preloaderPanel.transform.Find("FadeIn").gameObject.SetActive(false); // (DG) Otherwise it will catch clicks.... sigh...

		yield return new WaitForSeconds(1.0f);

		Debug.Log("[LoaderScript] WaitForLevelStreamed....");
		yield return StartCoroutine(WaitForLevelStreamed("Game")); // Note: The streaming itself is started by Unity.

		loadingBarText.text = XLocalization.Get("LoadingDoneText");
		loadingBarFillTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Forcing the loading bar to be full.

		yield return new WaitForSeconds(1.0f);

		// If you want to initialize things that are only just available from the streamed resources, here is the place.

		preloaderPanel.transform.Find("FadeOut").gameObject.SetActive(true); // (DG) A nice fade out to make things appear more smooth.

		yield return new WaitForSeconds(1.0f);

		// (DG) The game scene itself seems to load / initialize very fast into memory, so no need to have a loading bar for that in this case.
		Debug.Log("[LoaderScript] LoadLevel...");
		Application.LoadLevel("Game");
		// However, I still want to try to make it work for bigger scenes as well. Later.
//		Debug.Log("LoadLevelAsync...");
//		yield return StartCoroutine(LoadLevelAsync());

		// Below never runs because the scene is automatically started and all these objects are gone.

//		Debug.Log("LoadLevel async done");
//		Application.ExternalCall("console.log", " LoadLevel async done ");
//		loadingBarText.text = "LoadLevel async done";
//		yield return new WaitForSeconds( 2.0f);
//
//		Application.ExternalCall("console.log", " LoadLevelSequence ended");
//		Debug.Log("LoadLevelSequence ended");
	}

	private IEnumerator WaitForLevelStreamed(string level)
	{
		while (!Application.CanStreamedLevelBeLoaded(3))
		{
			loadingProgress = Application.GetStreamProgressForLevel(level);
			loadingBarFillTransform.localScale = new Vector3(loadingProgress, 1.0f, 1.0f);
//			Application.ExternalCall("console.log", " Streaming level " + loadingProgress*100);
			yield return 0;
		}
	}

	private IEnumerator LoadLevelAsync() {
		AsyncOperation async = Application.LoadLevelAsync("Game");
		yield return async;
		Debug.Log("[LoaderScript] LoadingLevelAsync complete");
	}


	// Below is the button functionality, only for the BrandingButton on this panel.
	// A bit overkill for now, I hope to change it to a more modular functionality one day.

	public void SetUpButtonsInHierarchy(GameObject go)
	{
		Button[] buttons = go.GetComponentsInChildren<Button>();
		foreach (Button buttonItem in buttons)
		{
			SetUpButton(buttonItem);
		}
	}

	public void SetUpButton(Button button)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => OnButton(button.gameObject));
	}

	public void OnButton (GameObject aButton) { OnButton(aButton.name);}
	public void OnButton(string aButtonName)
	{
		Debug.Log("[LoaderScript] OnButton received: " + aButtonName);
		switch(aButtonName)
		{
		case "BrandingButton":
			OpenURL("BrandingURL");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;
		}
	}

	public void OpenURL(string url)
	{
		if (!Data.clickLinks) { Debug.Log("[LoaderScript] OpenURL called. Globals ClickLinks was false!"); return;}
		string myURL;
		// Check if it's an url or a localization key
		if (url.Contains(".")) myURL = url;
		else myURL = XLocalization.Get(url);
		
		// create an evaluation
		string myEval;
		myEval = "window.open(\"" + myURL + "\", \"_blank\")";
		
		Debug.Log("[LoaderScript] OpenURL called,  evaluate: " + myEval);
		
		if (Application.platform == RuntimePlatform.WebGLPlayer ) 
		{
			Application.ExternalEval(myEval);
		}
		else
		{
			Application.OpenURL(myURL);
		}
	}
}
