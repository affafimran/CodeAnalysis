using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GamePanel.
/// Very important scripts that handles the updating of all interface objects during the game itself.
/// Some of the functionality defined and updated here could be chopped of onto various smallers scripts,
/// on the UI gameobjects themselves. For manageability reasons, we kept these here.
/// It also contains 'get ready' functionality, which could have had it's own panel and script as well.
/// It also handles a lot of the important PickUp functionality.
/// </summary>

public class GamePanel : MonoBehaviour {

	public GameObject brandingButton;
	private bool gamePanelSequenceInProgress;

	public GameObject controls;

	public GameObject weapon;
	private string  currentWeapon;
	private string  oldWeapon;
	private string weaponIconName;
	private Image weaponIconImage;
	private Text weaponAmmo;
	private GameObject weaponInfinityIcon;
	private XTweenScale weaponTween;

	public GameObject crosshair;
	public GameObject currentObjectUnderMouse;
	private Image crosshairImage;
	private XTweenScale crosshairTweenScale;
	private XTweenAlpha crosshairTweenAlpha;

	public GameObject explosive;
	private string explosiveIconName;
	private Image explosiveIconImage;
	private Text explosiveAmmo;
	private GameObject explosiveInfinityIcon;
	private XTweenScale explosiveTween;

	public GameObject gadgets;
	private XTweenBlink gadgetTween;

	public GameObject getReady;
	public GameObject go;
	public GameObject mission;
	public GameObject progress;

	public GameObject score;
	private Text scoreValueText;
	private XTweenScale scoreTween;

	public GameObject killStreak;
	private int killStreakAmount;
	private Text killStreakText;
	private XTweenScale killStreakTweenScale;
	private XTweenAlpha killStreakTweenAlpha;
	private float killStreakTimer;

	public GameObject target;
	private XTweenScale targetTween;
	private Image targetIcon;
	private Text targetValue;
	private int targetsLeft;
	private int oldTargetsLeft;

	public GameObject player;
	private Image playerIcon;

	public GameObject health;
	private XTweenColor healthTween;
	private float healthAsPercentage;
	private Vector3 healthScale;
	private bool healthWarning;

	public GameObject bulletTime;
	private XTweenColor bulletTimeTween;
	private float bulletTimeAsPercentage;
	private Vector3 bulletTimeScale;
	private bool bulletTimeWarning;

	public GameObject hit;
	private Image hitImage;
	private int hitAmount;
	private bool hitLooping;
	private int hitTimes;

	public GameObject cash;
	private Text cashValueText;
	private XTweenScale cashTweenScale;
	private int cashMultiplier;

	public GameObject message;
	private Text messageText;

	public GameObject pickUps;
	public GameObject pickUpPrefab;
	private Vector3 pickUpPos;

	public GameObject vehicle;
	private GameObject vehicleEnter;
	private GameObject vehicleExit;
	private bool inVehicleRange;
	private bool inVehicle;
	private string inVehicleName;

	public GameObject time;
	private GameObject timeValue;
	private int timeInt;
	private float timeFloat;
	private string timeString;
	private Text timeValueText;

	public GameObject minimap;
	public Minimap minimapScript;

	private Atlas hammer2IconsAtlas;
	private Atlas hammer2TargetsAtlas;
	private Atlas hammer2InterfaceAtlas;
	private Atlas hammer2CrosshairsAtlas;
	
	private MissionData missionData;

	// [MOBILE] References
	public GameObject mobileButtons;
	public GameObject joystickLeft;
	public GameObject joystickRight;

	public enum Joysticks {Left, Right};
	public enum JoystickLayout {Normal, Flipped};
	public enum JoystickSets {Normal, Car, Plane, Turret};

	public GameObject jumpButton;
	public GameObject crouchButton;
	public GameObject primaryFireButton;
	public GameObject secondaryFireButton;
	public GameObject driftButton;
	public GameObject pauseButton;
	public GameObject hammerTimeButton;
	public GameObject cycleWeaponButton;

	void Awake ()
	{
		// We need to know stuff about this mission.
		missionData = MissionManager.missionData;

		// We need references to atlasses since we'll be changing sprites on the fly.
		hammer2IconsAtlas = AtlasManager.hammer2IconsAtlas;
		hammer2InterfaceAtlas = AtlasManager.hammer2InterfaceAtlas;
		hammer2TargetsAtlas = AtlasManager.hammer2TargetsAtlas;
		hammer2CrosshairsAtlas = AtlasManager.hammer2CrosshairsAtlas;

		// Getting some private variable to use later in the update function.
		weaponIconImage = weapon.transform.Find("WeaponIcon").GetComponent<Image>();
		weaponTween = weapon.GetComponent<XTweenScale>();
		weaponAmmo = weapon.transform.Find("WeaponAmmo").GetComponent<Text>();
		weaponInfinityIcon = weapon.transform.Find("InfinityIcon").gameObject;

		explosiveIconImage = explosive.transform.Find("ExplosiveIcon").GetComponent<Image>();
		explosiveTween = explosive.GetComponent<XTweenScale>();
		explosiveAmmo = explosive.transform.Find("ExplosiveAmmo").GetComponent<Text>();
		explosiveInfinityIcon = explosive.transform.Find("InfinityIcon").gameObject;

		foreach (Transform t in gadgets.transform)
		{
			t.localScale = new Vector3(0,0,0);
			t.gameObject.SetActive(false);
		}

		crosshairImage = crosshair.GetComponent<Image>();
		crosshairTweenScale = crosshair.GetComponent<XTweenScale>();
		crosshairTweenAlpha = crosshair.GetComponent<XTweenAlpha>();

		targetIcon = target.GetComponent<Image>();
		Debug.Log("[GamePanel] InterfacePanel attempting to find target icon: " + missionData.target);
		targetIcon.sprite = hammer2TargetsAtlas.Get(missionData.target);
		targetValue = target.transform.Find("TargetValue").GetComponent<Text>();
		targetTween = target.GetComponent<XTweenScale>();

		killStreakAmount = 0;
		killStreakTweenScale = killStreak.GetComponent<XTweenScale>();
		killStreakTweenAlpha = killStreak.GetComponent<XTweenAlpha>();
		killStreakText = killStreak.transform.Find("Text").GetComponent<Text>();
		killStreakText.text = "";

		playerIcon = player.transform.Find("Icon").GetComponent<Image>();

		healthTween = health.transform.Find("BarFill").GetComponent<XTweenColor>();
		healthScale = new Vector3(1.0f, 1.0f, 1.0f);
		healthWarning = false;

		bulletTimeTween = bulletTime.transform.Find("BarFill").GetComponent<XTweenColor>();
		bulletTimeScale = new Vector3(1.0f, 1.0f, 1.0f);
		bulletTimeWarning = false;

		hitImage = hit.GetComponent<Image>();
		hitImage.enabled = false;

		scoreValueText = score.transform.Find("ScoreValue").GetComponent<Text>();
		scoreTween = score.transform.Find("ScoreValue").GetComponent<XTweenScale>();

		cashValueText = cash.transform.Find("CashValue").GetComponent<Text>();
//		cashTween = cash.GetComponent<XTweenAlpha>();
		cashTweenScale = cash.GetComponent<XTweenScale>();
		cashMultiplier = (ShopItemManager.IsBought("ShopItem9")) ? 2 : 1;

		messageText = message.GetComponent<Text>();

		timeValue = time.transform.Find("Value").gameObject;
		timeValueText = timeValue.GetComponent<Text>();

		inVehicle = false;
		inVehicleName = "";
		inVehicleRange = false;

		// Hide everything so we can start with a clean slate.
		foreach (Transform t in transform) t.gameObject.SetActive(false);

	}

	void OnEnable() {
		// Added this to make sure elements are on if they hadn't been activated yet.
		if (gamePanelSequenceInProgress)
		{	
			StopAllCoroutines();
			GamePanelObjectsSetActive(true);
			gamePanelSequenceInProgress = false;
			if (MissionManager.missionInProgress == false) Debug.Log("Reverting to game but mission has not been started yet!");
		}
	}

	// Use this for initialization
	void Start () {
		StartCoroutine(GamePanelSequence());
	}
	
	private IEnumerator GamePanelSequence()
	{
		Debug.Log("[GamePanel] GamePanelSequence started.");
		gamePanelSequenceInProgress = true;

		// Hide and lock the cursor
		CursorManager.SetCursor(false);

		Data.pausingAllowed = false;
		oldWeapon = currentWeapon = Scripts.hammer.currentWeapon.ToString();

		//Scripts.audioManager.StopAllSFX(); // (PA) Temp disabled.
		Scripts.audioManager.StopAllMusic();

		Scripts.audioManager.PlaySFX("Misc/AmbientTraffic", 0.4f, -1);

		if (!GameData.skipGetReady) // Added this so you don't have to wait for testing.
		{
			getReady.GetComponent<Text>().text = "GET READY !";
			Scripts.audioManager.PlaySFX("Interface/GetReadyJingle");
			yield return new WaitForSeconds(1.0f);
			mission.SetActive(true);
			mission.transform.Find("Header").GetComponent<Text>().text = XLocalization.Get("MissionText") + " " + GameData.mission.ToString() + ": " + XLocalization.Get("Mission" + GameData.mission.ToString() + "HeaderText");
			mission.transform.Find("Description").GetComponent<Text>().text = XLocalization.Get("Mission" + GameData.mission.ToString() + "DescriptionText");
			progress.SetActive(true);
			CheckProgress(progress);
			yield return new WaitForSeconds(3.5f);
			mission.SetActive(false);
			progress.SetActive(false);
			yield return new WaitForSeconds(0.5f);
			Scripts.audioManager.PlaySFX("Interface/Unlocked");
			getReady.SetActive(true);
			yield return new WaitForSeconds(2.0f);
			Scripts.audioManager.PlaySFX("Interface/Warning");
			getReady.SetActive(false);
			go.SetActive(true);
			Scripts.audioManager.PlayMusic("GameMusic",1.0f, -1);
			MissionManager.StartMission();
			yield return new WaitForSeconds(3.0f);
			go.SetActive(false);
		}
		else Scripts.audioManager.PlayMusic("GameMusic",1.0f, -1);

		GamePanelObjectsSetActive(true); // Shows the rest of the elements.

		if (GameData.skipGetReady) MissionManager.StartMission();

		Debug.Log("[GamePanel] GamePanelSequence ended.");
		gamePanelSequenceInProgress = false;
	}

	private void GamePanelObjectsSetActive(bool state)
	{
		weapon.SetActive(state);
		crosshair.SetActive(state);

		gadgets.SetActive(state);
		controls.SetActive(state);
		pickUps.SetActive(state);
		score.SetActive(state);
		cash.SetActive(state);
		message.SetActive(state);
		target.SetActive(state);
		time.SetActive(state);

		player.SetActive(state);
		bulletTime.SetActive(state);
		health.SetActive(state);
		hit.SetActive(state);
		minimap.SetActive(state);
		brandingButton.SetActive(state);

		// [MOBILE] Activate the joysticks (layout to normal) and the additional buttons
		if (GameData.mobile){
			mobileButtons.SetActive(true); // set the parent of all buttons
			SetJoystick(Joysticks.Left, state); // left joystick
			SetJoystick(Joysticks.Right, state); // right joystick
			SetJoystickLayout(JoystickLayout.Normal); // normal joystick layout
			jumpButton.SetActive(true); // jump button 
			crouchButton.SetActive(true); // crouch
			pauseButton.SetActive (true); // pause 
			controls.SetActive(false); // disable control text
		}
	}

	/// <summary>
	/// Updates the joystick set.
	/// These are 'quick' sets for easy control
	/// </summary>
	/// <param name="aSet">A set.</param>
	public void UpdateJoystickSet(JoystickSets aSet)
	{
		switch(aSet){
		case JoystickSets.Normal:
			SetJoystick(Joysticks.Left, true);
			SetJoystick(Joysticks.Right, true);
			SetJoystickLayout(JoystickLayout.Normal);
			break;		
		
		case JoystickSets.Car:
		case JoystickSets.Plane:
			SetJoystick(Joysticks.Right, false);
			break;
		
		case JoystickSets.Turret:
			SetJoystick(Joysticks.Left, false);
			SetJoystickLayout(JoystickLayout.Flipped);
			break;		
		}
	}

	/// <summary>
	/// Set the visibility state of a joystick
	/// </summary>
	/// <param name="joystick">Joystick.</param>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public void SetJoystick(Joysticks joystick, bool aState)
	{
		switch(joystick){
			case Joysticks.Left: joystickLeft.SetActive(aState); break;
			case Joysticks.Right: joystickRight.SetActive(aState); break;
		}
	}

	/// <summary>
	/// Set the layout of the joystick
	/// </summary>
	/// <param name="layout">Layout.</param>
	public void SetJoystickLayout(JoystickLayout layout)
	{
		// I try to void switching them directly so it doesnt get messedup
		RectTransform rectTransform;
		switch(layout){
		case JoystickLayout.Normal:
			// Left
			rectTransform= joystickLeft.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(Mathf.Abs(rectTransform.anchoredPosition.x), rectTransform.anchoredPosition.y);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);
			joystickLeft.GetComponent<MobileJoystick>().SetStartPosition();
			// Right
			rectTransform = joystickRight.GetComponent<RectTransform>();
			rectTransform.anchoredPosition =  new Vector2(-Mathf.Abs(rectTransform.anchoredPosition.x), rectTransform.anchoredPosition.y);
			rectTransform.anchorMin = new Vector2(1, 0);
			rectTransform.anchorMax = new Vector2(1, 0);
			joystickRight.GetComponent<MobileJoystick>().SetStartPosition();
			break;

		case JoystickLayout.Flipped:
			// Left
			rectTransform = joystickLeft.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(-Mathf.Abs(rectTransform.anchoredPosition.x), rectTransform.anchoredPosition.y);
			rectTransform.anchorMin = new Vector2(1, 0);
			rectTransform.anchorMax = new Vector2(1, 0);
			joystickLeft.GetComponent<MobileJoystick>().SetStartPosition();
			// Right
			rectTransform = joystickRight.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(Mathf.Abs(rectTransform.anchoredPosition.x), rectTransform.anchoredPosition.y);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);
			joystickRight.GetComponent<MobileJoystick>().SetStartPosition();
			break;
		}
	}

	// Update is called once per frame
	void Update () {

		// Important updating of all meters and UI stuff.
		if (!gamePanelSequenceInProgress && MissionManager.missionInProgress)
		{
			UpdateWeapon();
			
			UpdateCrosshair();
			
			UpdateExplosive();
			UpdateGadgets();
			
			UpdateTarget();
			UpdateKillStreak();
			UpdateScore();
			UpdateCash(); // Could maybe not be called every frame.

			UpdatePlayer();

			UpdateHealth();

			UpdateMessage();
			UpdateTime();
			UpdateVehicle(); // Should not be called during get ready!

			UpdateBulletTime(); // Placed last since it might deactivate some other elements!

		}

	}

	// Checking progress on challenges, displayed on the mission / get ready screen.
	private void CheckProgress(GameObject progress)
	{
		GameObject targetTime = progress.transform.Find("TargetTime").gameObject;
		GameObject targetTimeText = targetTime.transform.Find("Text").gameObject;
		GameObject targetScore = progress.transform.Find("TargetScore").gameObject;
		GameObject targetScoreText = targetScore.transform.Find("Text").gameObject;
		GameObject hiddenPackage = progress.transform.Find("HiddenPackage").gameObject;
		GameObject hiddenPackageText = hiddenPackage.transform.Find("Text").gameObject;

		MissionData missionProgressData = MissionManager.GetMissionByID(GameData.mission);

		bool hiddenPackageFound = (missionProgressData != null && missionProgressData.completed) ? missionProgressData.hiddenPackage : false;
		hiddenPackageText.GetComponent<Text>().text = (hiddenPackageFound) ? XLocalization.Get("FindPackageSuccesText") : XLocalization.Get("FindPackageText");
		hiddenPackageText.GetComponent<Text>().color = (hiddenPackageFound) ? Color.white : Color.grey;
		hiddenPackage.transform.Find("Icon").gameObject.SetActive(hiddenPackageFound);
		hiddenPackage.transform.Find("Circle").gameObject.SetActive(!hiddenPackageFound);
		
		bool targetTimeMet = (missionProgressData != null && missionProgressData.completed) ? (missionProgressData.totalTime <= missionProgressData.targetTime) : false;
		targetTimeText.GetComponent<Text>().text = (targetTimeMet) ? XLocalization.Get("BeatTimeSuccesText") : XLocalization.Get("BeatTimeText") + " " + GenericFunctionsScript.ConvertTimeToStringMSS(missionData.targetTime);
		targetTimeText.GetComponent<Text>().color = (targetTimeMet)? Color.white : Color.grey;
		targetTime.transform.Find("Icon").gameObject.SetActive(targetTimeMet);
		targetTime.transform.Find("Circle").gameObject.SetActive(!targetTimeMet);
		
		bool targetScoreMet = (missionProgressData != null && missionProgressData.completed) ? (missionProgressData.totalScore >= missionProgressData.targetScore) : false;
		targetScoreText.GetComponent<Text>().text = (targetScoreMet) ? XLocalization.Get("BeatScoreSuccesText") : XLocalization.Get("BeatScoreText") + " " + missionData.targetScore.ToString();
		targetScoreText.GetComponent<Text>().color = (targetScoreMet)? Color.white : Color.grey;
		targetScore.transform.Find("Icon").gameObject.SetActive(targetScoreMet);
		targetScore.transform.Find("Circle").gameObject.SetActive(!targetScoreMet);
	}

	private void UpdateTarget()
	{
		targetsLeft = missionData.targetsLeft;
		targetValue.text = "x " + targetsLeft.ToString();
		if (targetsLeft != oldTargetsLeft) TargetAnimation();
		oldTargetsLeft = targetsLeft;
	}

	private void UpdateKillStreak()
	{
		if (killStreakTimer>0) killStreakTimer -= Time.deltaTime;
		else if (killStreak.activeSelf)
		{
			if (killStreakAmount == 1) PickUp("OneKill"); // Sending it anyway, although no bonus is awarded.
			if (killStreakAmount == 2) PickUp("DoubleKill");
			if (killStreakAmount == 3) PickUp("TripleKill");
			if (killStreakAmount >= 4) PickUp("MultiKill");
			if (killStreakAmount >= 10) PickUp("UltraKill");
			if (killStreakAmount >= 25) PickUp("GodLikeKill");

			killStreakAmount = 0;
			killStreak.SetActive(false);
		}
	}

	public void AddKillStreakKill()
	{
		killStreakAmount++;

		if (!Scripts.hammer.isInBulletTime) killStreak.SetActive(true); // Don't want the killstreak in bullettime I think.
		else return;
		killStreakText.text ="x" + killStreakAmount.ToString();
		killStreakTweenScale.enabled = false;
		killStreakTweenScale.enabled = true;
		killStreakTweenAlpha.enabled = false;
		killStreakTweenAlpha.enabled = true;

		killStreakTimer = 3.0f; // (DG) Should correspond with the duration of the TweenAlpha
	}

	private void UpdateWeapon()
	{
		currentWeapon = Scripts.hammer.currentWeapon.ToString();
		weaponIconName = currentWeapon;
		weaponIconImage.sprite = hammer2IconsAtlas.Get(weaponIconName);
		weaponIconImage.SetNativeSize();
		if (Scripts.hammer.weapon.unlimitedAmmo)
		{
			weaponInfinityIcon.SetActive(true);
			weaponAmmo.gameObject.SetActive(false);
		}
		else
		{
			weaponInfinityIcon.SetActive(false);
			weaponAmmo.gameObject.SetActive(true);
			weaponAmmo.text = Scripts.hammer.weapon.ammo.ToString();
		}

		if (currentWeapon != oldWeapon)
		{
			WeaponAnimation();
			CrosshairAnimation();
		}
		oldWeapon = currentWeapon;

		// [MOBILE] Hide/Show the cycle weapon button
		if (GameData.mobile){
			if (Scripts.hammer.amountOfWeapons > 1 && !inVehicle) cycleWeaponButton.SetActive(true);
			else cycleWeaponButton.SetActive(false);
		}
	}

	private void UpdateCrosshair()
	{
		// Get the crosshair name for the weapon.
		string crosshairBaseName = ""; // Default no crosshair for now.

		if (inVehicle)
		{
			switch (inVehicleName)
			{
				case "Turret":
				case "MissileTurret":
				case "Tank":
				case "MammothTank":
				case "Helicopter":
				case "Apache":
				case "F22": crosshairBaseName =  "MiniGun";
				break;
			}
		}
		else
		{
			switch (currentWeapon)
			{
				case "DesertEagle": case "Magnum": crosshairBaseName =  "Default"; break;
				case "AK47": crosshairBaseName =  "SubMachineGun"; break;
				case "Remington": crosshairBaseName =  "Shotgun"; break;
				case "Rpg": crosshairBaseName =  "RocketLauncher"; break;
			}
		}

		if (crosshairBaseName == "")
		{
			crosshair.SetActive(false);
			return;
		}
		else crosshair.SetActive(true);

		// Determine what we're aiming at.
		GameObject crosshairObject = null;
		string crosshairObjectLayer = "";
		string crosshairSuffix = "Off";
		Color crosshairColor = Color.white;
		currentObjectUnderMouse = GameData.currentObjectUnderMouse;

		if(currentObjectUnderMouse != null)
		{
			crosshairObject = GameData.currentObjectUnderMouse;
			crosshairObjectLayer = LayerMask.LayerToName(crosshairObject.layer);
			if (crosshairObjectLayer == "Units" || crosshairObjectLayer == "Enemies")
			{
				crosshairSuffix = "On";
				crosshairColor = Color.yellow;
			}
		}
		// Set the image and the color;
		crosshairImage.sprite = hammer2CrosshairsAtlas.Get(crosshairBaseName+crosshairSuffix);
		crosshairImage.color = crosshairColor;
	}

	private void UpdateExplosive()
	{
		if (Scripts.hammer.currentExplosive.ToString() != "None")
		{
			explosiveIconName = Scripts.hammer.currentExplosive.ToString();
			explosiveIconImage.sprite = hammer2IconsAtlas.Get(explosiveIconName);
			explosiveIconImage.SetNativeSize();
			if (Scripts.hammer.explosive.explosiveData.unlimitedAmmo)
			{
				explosiveInfinityIcon.SetActive(true);
				explosiveAmmo.gameObject.SetActive(false);
			}
			else if (Scripts.hammer.explosive.explosiveData.ammo > 0)
			{
				explosiveInfinityIcon.SetActive(false);
				explosiveAmmo.gameObject.SetActive(true);
				explosiveAmmo.text = Scripts.hammer.explosive.explosiveData.ammo.ToString();
			}
			else if (Scripts.hammer.explosive.explosiveData.ammo == 0) explosive.SetActive(false);
		}
	}

	private void UpdateGadgets()
	{
		foreach (Gadget aGadget in Scripts.hammer.gadgets)
		{
			Transform gadgetTransform = gadgets.transform.Find(aGadget.type);
			if (gadgetTransform != null && gadgetTransform.gameObject.activeSelf == false)gadgetTransform.gameObject.SetActive(true);
		}
	}

	private void UpdateScore()
	{
		scoreValueText.text = missionData.score.ToString();
	}

	private void ScoreAnimation()
	{
		scoreTween.enabled = false;
		scoreTween.enabled = true;
	}

	private void UpdateCash()
	{
		cashValueText.text = "$" + GenericFunctionsScript.AddSeparatorInInt(GameData.cash, ",");
	}

	private void CashAnimation()
	{
		cashTweenScale.enabled = false;
		cashTweenScale.enabled = true;
	}

	private void WeaponAnimation()
	{
		weaponTween.enabled = false;
		weaponTween.enabled = true;
	}

	private void CrosshairAnimation()
	{
		crosshairTweenAlpha.enabled = false;
		crosshairTweenAlpha.enabled = true;
		crosshairTweenScale.enabled = false;
		crosshairTweenScale.enabled = true;
	}


	private void ExplosiveAnimation()
	{
		explosive.SetActive(true);
		explosiveTween.enabled = false;
		explosiveTween.enabled = true;
	}

	private void GadgetAnimation()
	{
		// Disabled now since there's a tween on each gadget seperately.
//		gadgetTween.enabled = false;
//		gadgetTween.enabled = true;
	}

	
	private void TargetAnimation()
	{
		targetTween.enabled = false;
		targetTween.enabled = true;
	}

	private void UpdatePlayer()
	{
		if (Scripts.hammer.vehicleData.isInVehicle)
		{
			string iconName = Scripts.hammer.vehicleData.name;
			if (iconName.Contains("Traffic")) iconName = "Traffic";
			playerIcon.sprite = hammer2TargetsAtlas.Get(iconName);
		}
		else playerIcon.sprite = hammer2TargetsAtlas.Get("Hammer");
	}

	private void UpdateHealth()
	{
		// Update the healthbar.
		healthAsPercentage = Scripts.hammer.characterData.health * 0.01f;
		healthScale.x = healthAsPercentage;
		health.transform.Find("BarFill").localScale = healthScale;

		// Activate healthwarning.
		if (healthAsPercentage < 0.34f && !healthWarning)
		{
			healthWarning = true;
			ActivateHit(-1);
			Scripts.audioManager.PlaySFX("Interface/LowHealth", 1.0f, -1);
			healthTween.enabled = false;
			healthTween.enabled = true;
			healthTween.startColor = Color.red;
			healthTween.playStyle = XTweener.Style.Loop;
		}
		if (healthAsPercentage > 0.34f && healthWarning)
		{
			healthWarning = false;
			ActivateHit(0); // stopping the loop effect.
			Scripts.audioManager.StopSFX("LowHealth");
			healthTween.playStyle = XTweener.Style.Once;
			healthTween.startColor = Color.white;
			healthTween.enabled = false;
			healthTween.transform.GetComponent<Image>().color = healthTween.endColor;
		}
	}

	private void UpdateBulletTime()
	{
		// Update the bullet time bar.
		bulletTimeAsPercentage = Scripts.hammer.characterData.bulletTimeAsPercentage;
		bulletTimeScale.x = bulletTimeAsPercentage;
		bulletTime.transform.Find("BarFill").localScale = bulletTimeScale;
		
		// Activate bulletTimeWarning when in use.
		if (Scripts.hammer.isInBulletTime && !bulletTimeWarning)
		{
			bulletTimeWarning = true;
			Scripts.audioManager.SetAllMusicVolume(0.1f);
			bulletTimeTween.enabled = false;
			bulletTimeTween.enabled = true;
//			bulletTimeTween.startColor = new Color(93.0f/255.0f, 136.0f/255.0f, 248.0f/255.0f, 255.0f);
			bulletTimeTween.playStyle = XTweener.Style.Loop;

			GamePanelObjectsSetActive(false); // Shows the rest of the elements.
			player.SetActive(true);
			gadgets.SetActive(false);
			killStreak.SetActive(false);
		}

		// DEactivate bulletTimeWarning when going out of bullettime.
		if (!Scripts.hammer.isInBulletTime && bulletTimeWarning)
		{
			bulletTimeWarning = false;
			Scripts.audioManager.SetAllMusicVolume(Data.musicVolume);
			bulletTimeTween.playStyle = XTweener.Style.Once;
//			bulletTimeTween.startColor = Color.white;
			bulletTimeTween.enabled = false;
			bulletTimeTween.transform.GetComponent<Image>().color = bulletTimeTween.endColor;

			GamePanelObjectsSetActive(true); // Shows the rest of the elements.
			gadgets.SetActive(true);
			killStreak.SetActive(true);
		}
	}

	private void HealthAnimation()
	{
		healthTween.enabled = false;
		healthTween.enabled = true;
	}


	private void UpdateMessage()
	{
		// This is the text displayed bottom center on the screen

		messageText.text = "";
		string message = "";
		string messageHeader = "";
		string messageDescription = "";

		if (inVehicle)
		{
			switch (inVehicleName)
			{
			case "Turret":
			case "MissileTurret":
			case "Tank":
			case "MammothTank":
			case "Helicopter":
			case "Apache":
			case "F22":	
			messageHeader = XLocalization.Get(inVehicleName + "HeaderText");
			if (messageHeader == inVehicleName + "HeaderText") messageHeader = ""; // If it's not found, empty it.
			messageDescription = XLocalization.Get(inVehicleName + "DescriptionText");
			if (messageDescription == inVehicleName + "DescriptionText") messageDescription = ""; // If it's not found, empty it.
			message = messageHeader + "\n" + messageDescription;
			messageText.text = message;
			break;
			}
		}
	}

	private void UpdateTime()
	{
		// I'm guessing the main reason that the mission time was originally an int was for highscore purposes.
		timeValueText.text = GenericFunctionsScript.ConvertTimeToStringMSS(missionData.time);
	}

	private void UpdateVehicle()
	{
		inVehicleRange = Scripts.hammer.vehicleData.canEnterVehicle; // can we enter vehicle?
		inVehicle = Scripts.hammer.vehicleData.isInVehicle; // are we IN a vehicle
		if (inVehicle){ // we are in a vehicle (get name, show exit button)
			inVehicleName = Scripts.hammer.vehicleData.name;			
			vehicle.SetActive(true);
			vehicle.transform.Find("VehicleExit").gameObject.SetActive(true);
			vehicle.transform.Find("VehicleEnter").gameObject.SetActive(false);
		}else if (inVehicleRange){ // we can enter a vehicle (show enter button)
			vehicle.SetActive(true);
			vehicle.transform.Find("VehicleExit").gameObject.SetActive(false);
			vehicle.transform.Find("VehicleEnter").gameObject.SetActive(true);
		}else vehicle.SetActive(false); // nothing here don't show anything related to enter/exit vehicle.

		bulletTime.SetActive(!inVehicle); // Show/hide HammerTime bar
		health.SetActive(!inVehicle); // Show/hide health bar

		weapon.SetActive(!inVehicle); // Show/hide weapon
		explosive.SetActive(inVehicle ? false : !(Scripts.hammer.explosive.explosiveData.ammo == 0)); // Show/hide weapon
		gadgets.SetActive(!inVehicle);// Show/hide gadgets

		// [MOBILE] This is a pretty big exception.
		// Here we show/hide the additional mobile control buttons.
		// However, there are additional buttons for vehicles as well. So we need to take care of that as well
		if (GameData.mobile){
			if (inVehicle){
				switch (inVehicleName){ // Show/Hide primary, secondary, jump(up), crouch(down) or drift button.
					case "Turret": primaryFireButton.SetActive(true); crouchButton.SetActive(false); jumpButton.SetActive(false); break;
					case "MissileTurret": primaryFireButton.SetActive(true); crouchButton.SetActive(false); jumpButton.SetActive(false); break;
					case "Tank": primaryFireButton.SetActive(true); crouchButton.SetActive(false); jumpButton.SetActive(false); break;
					case "MammothTank": primaryFireButton.SetActive(true); secondaryFireButton.SetActive(true); crouchButton.SetActive(false); jumpButton.SetActive(false); break;
					case "Helicopter": primaryFireButton.SetActive(true); crouchButton.SetActive(true); jumpButton.SetActive(true); break;
					case "Apache": primaryFireButton.SetActive(true); secondaryFireButton.SetActive(true); crouchButton.SetActive(true); jumpButton.SetActive(true); break;
					case "F22": primaryFireButton.SetActive(true); secondaryFireButton.SetActive(true); crouchButton.SetActive(true); jumpButton.SetActive(true); break;
					default: driftButton.SetActive(true); crouchButton.SetActive(false); jumpButton.SetActive(false); break;
				}
				hammerTimeButton.SetActive(false); // never show hammer time button in vehicle.
			}
			else{ // Default buttons for third person control
				primaryFireButton.SetActive(false);
				secondaryFireButton.SetActive(false);
				driftButton.SetActive(false);
				crouchButton.SetActive(false);
				jumpButton.SetActive(true);
				hammerTimeButton.SetActive(true);
			}
		}
	}

	// PickUp
	// Mainly called by the pickupmanager.
	// Can / is also used to display stuff in the center of the screen.
	// The killstreaks for example.
	public void PickUp(string pickUp) {	PickUp (pickUp, 0); }
	public void PickUp(string pickUpName, int score)
	{
		//Debug.Log("PickUp called: " + pickUpName + ", score: " + score.ToString());
		// This function spawns a pickup that can have a header text, description text, icon and sound
		// It turns out to be a little bit more flexible than creating lots of prefabs.

		// Get some data from the shared date text file
		Dictionary<string, DicEntry> pickUpData = Data.Shared["PickUps"].d[pickUpName].d;

		// in some cases, we will not spawn a pickup element, but maybe just play a sound
		bool spawn = true;
		
		// set up text variables. we need to make sure that the string does not change if the key does not exist.
		string pickUpHeaderText  = XLocalization.Get("PickUp" + pickUpName + "HeaderText");
		string pickUpDescriptionText = XLocalization.Get("PickUp" + pickUpName + "DescriptionText");
		
		// in default cases, we will not play a sound if the shared data says None.
		string pickUpSound = pickUpData["sound"].s;
		
		// do we need an icon?
		string icon = "None";
		
		// destroy the old pickup
		GameObject[] pickUpDynamics;
		pickUpDynamics = GameObject.FindGameObjectsWithTag("PickUpDynamic");
		foreach (GameObject dynamic in pickUpDynamics) Destroy (dynamic);
		
		// define some temporary variables for accessing the properties later on.
		GameObject clone;
		Transform _t;
		
		// setting some default colours for the texts
		Color _c = new Color(1.0f,1.0f,1.0f,1.0f);
		Color _cStroke = new Color(1.0f,1.0f,1.0f,0.0f);
		Color _cDesc = new Color(1.0f,1.0f,1.0f,1.0f);
		Color _cDescStroke = new Color(1.0f,1.0f,1.0f,0.0f);
		
		// creating a palette, this could be moved somewhere else I guess.
		Color colorOrange = new Color(253.0f/255.0f,82.0f/255.0f,21.0f/255.0f,255.0f/255.0f);
		Color colorGreen = new Color(0.0f/255.0f,197.0f/255.0f, 85.0f/255.0f,255.0f/255.0f);
		Color colorBlue = new Color(50.0f/255.0f,100.0f/255.0f,170.0f/255.0f,255.0f/255.0f);
		Color colorYellow = new Color(255.0f/255.0f,210.0f/255.0f,84.0f/255.0f,255.0f/255.0f);
		Color colorRed = new Color(188.0f/255.0f,28.0f/255.0f,28.0f/255.0f,255.0f/255.0f);
		Color colorPink = new Color(255.0f/255.0f,85.0f/255.0f,213.0f/255.0f,255.0f/255.0f);
		Color colorPurple = new Color(177.0f/255.0f,35.0f/255.0f,172.0f/255.0f,255.0f/255.0f);
		
		// setting the pickup specific properties, or override stuff from shared data.
		switch (pickUpName)
		{
			// Score
		case "Score": pickUpHeaderText = ""; pickUpDescriptionText = "+" + score.ToString(); missionData.score += score; ScoreAnimation(); break;

			// Health
		case "HealthSmall": _c = colorRed;_cStroke = Color.black; HealthAnimation(); break;
		case "HealthBig": _c = colorRed;_cStroke = Color.black; HealthAnimation(); break;

			// Cash
		case "CashSmall":
		case "CashMedium":
		case "CashBig":
		case "CashBriefcase":
			_c = colorGreen;_cStroke = Color.black;
			pickUpDescriptionText += " $ " + (pickUpData["value"].i * cashMultiplier).ToString(); // Add the amount to the description text.
			AddCash(pickUpData["value"].i);
			break;

		case "Silver": _c = Color.gray;_cStroke = Color.white;  break;
		case "Gold": _c = colorYellow;_cStroke = Color.white; break;
		case "Ruby": _c = Color.red;_cStroke = colorRed; break;
		case "Diamond": _c = colorBlue;_cStroke = Color.white; break;

			// Primary
		case "Weapon":
		case "AK47":
		case "Remington":
		case "Rpg":
		case "DesertEagle":
			_c = colorBlue;_cStroke = Color.white; WeaponAnimation();
			missionData.score += pickUpData["value"].i;
			break;
			
			// Secondary
		case "Explosive":
			_c = colorRed;_cStroke = Color.black; ExplosiveAnimation();
			missionData.score += pickUpData["value"].i;
			break;

			// Gadgets here
		case "Gadget":
		case "Pigeon":
		case "RapidFire":
		case "TimeFreeze":
		case "CashForKills":
		case "Magnet":
			_c = colorOrange;_cStroke = colorYellow; GadgetAnimation();
			missionData.score += pickUpData["value"].i; // You actually get points for picking them up.
			break;
		
		case "GadgetBonus":
			_c = colorOrange;_cStroke = colorYellow;
			pickUpDescriptionText += " +" + pickUpData["value"].i.ToString(); // Add the amount to the description text.
			missionData.score += pickUpData["value"].i;
			ScoreAnimation();
			break;

			// Hidden package and secret stuff.
		case "HiddenPackage":
			_c = colorYellow;_cStroke = colorPurple; _cDesc = Color.white; _cDescStroke = Color.black;
			missionData.hiddenPackage = true;
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.POSTMAN, 1);
			break;
		case "ColdOne": _c = colorGreen;_cStroke = Color.white;
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.THEPROFESSIONAL, 1);
			break;

			// Bonus for kill streaks
		case "OneKill": spawn = false; break;
		case "DoubleKill": _c = Color.white;_cStroke = Color.grey; _cDesc = Color.white; icon = "DoubleKill"; break;
		case "TripleKill": _c = colorOrange;_cStroke = colorRed; _cDesc = Color.white; icon = "TripleKill"; break;
		case "MultiKill":  _c = colorRed;_cStroke = colorPink; _cDesc = Color.white; icon = "MultiKill"; break;
		case "UltraKill":  _c = colorPink;_cStroke = colorPurple; _cDesc = Color.white; icon = "UltraKill"; break;
		case "GodLikeKill":  _c = colorPurple;_cStroke = Color.black; _cDesc = Color.white; icon = "GodLikeKill"; break;
			
			// Other bonuses here
		case "Headshot": _c = Color.black;_cStroke = colorRed; _cDesc = Color.white; _cDescStroke = Color.black; pickUpSound = "Interface/PickUpHeadshot"; break;		
		}

		if (pickUpName == "Silver" || pickUpName == "Gold" || pickUpName == "Ruby" || pickUpName == "Diamond")
		{
			pickUpDescriptionText += " $ " + (pickUpData["value"].i * cashMultiplier).ToString(); // Add the amount to the description text.
			AddCash(pickUpData["value"].i);
		}

		if (pickUpName.Contains("Kill"))
		{
			pickUpDescriptionText += " +" + pickUpData["value"].i.ToString(); // Add the amount to the description text.
			missionData.score += pickUpData["value"].i;
			ScoreAnimation();
		}

		// Play pickup sound (also if there will be no pickup element.
		if (pickUpSound != "None") Scripts.audioManager.PlaySFX(pickUpSound);

		if(spawn && pickUps.gameObject.activeSelf == true)
		{
			// Create clone	
			clone = Instantiate(pickUpPrefab, transform.position, transform.rotation) as GameObject;
			_t = clone.GetComponent<Transform>();
			_t.SetParent(pickUps.transform, false); // changed due to warning.
//			_t.parent = pickUps.transform;
			clone.transform.localPosition = new Vector3 (0.0f,64.0f,0.0f);
			clone.transform.localScale = new Vector3 (1.0f,1.0f,1.0f);

			 // Set the icon. Now only used for kill bonusses.
			 if (icon != "None")
			{
				clone.transform.Find("Icon").gameObject.SetActive(true);
				Image iconImage = clone.transform.Find("Icon").GetComponent<Image>();
				iconImage.sprite = hammer2InterfaceAtlas.Get(icon);
				iconImage.SetNativeSize();
				clone.transform.localPosition = new Vector3 (0.0f,128.0f,0.0f);
				clone.transform.localScale = new Vector3 (0.001f,0.001f,0.001f);
				clone.GetComponent<XTweenPosition>().enabled = false;
				clone.GetComponent<XTweenScale>().enabled = true;
				clone.transform.Find("Icon").GetComponent<XTweenRotation>().enabled = true;
			}
			else clone.transform.Find("Icon").gameObject.SetActive(false);

			// Set specifics for clone header
			if (pickUpHeaderText != "None")
			{
				clone.transform.Find("Header").gameObject.SetActive(true);		
				Text _labelHeader = clone.transform.Find("Header").GetComponent<Text>();
				Outline _labelHeaderStroke = clone.transform.Find("Header").GetComponent<Outline>();
				_labelHeader.text = pickUpHeaderText;
				_labelHeader.color = _c;
				_labelHeaderStroke.effectColor= _cStroke;
			}
			
			// Set specifics for clone description	
			if (pickUpDescriptionText != "None")
			{
				clone.transform.Find("Description").gameObject.SetActive(true);		
				Text	_labelDescription = clone.transform.Find("Description").GetComponent<Text>();
				Outline	_labelDescriptionStroke = clone.transform.Find("Description").GetComponent<Outline>();
				_labelDescription.text = pickUpDescriptionText;
				_labelDescription.color = _cDesc;
				_labelDescriptionStroke.effectColor = _cDescStroke;
			}
			
		}
		
	}

	private void AddCash(int amount)
	{
		GameData.cash += (amount * cashMultiplier); // Actually add the cash to your inventory. Not sure if we should save here.
		CashAnimation(); // Play the animation of the interface element.
		Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.MONEYMAN, GameData.cash); // (DG) For testing only, since it's not saved yet and may be moved to results.
	}

	public void ActivateHit(){ ActivateHit(3); }
	public void ActivateHit(int amount)
	{
		//Debug.Log("ActivateHit received: " + amount.ToString() + " looping was: " + hitLooping.ToString());
		// amount 1 - 3 is the amount of the effect.
		hitAmount = amount;

		// Stop the old one, if we were not looping.
		if (hitLooping == false || hitAmount == 0){ 
			StopCoroutine("HitSequence");
			hitImage.enabled = false;
			hitLooping = false;
		}
		
		// Determine if we need to play a sound. Can also play when loop is true.
		if (hitAmount>0) Scripts.audioManager.PlaySFX("Interface/Hit"); //" + amount.ToString());
		
		// Run a new coroutine
		if (hitAmount != 0 && hitLooping == false) StartCoroutine("HitSequence", hitAmount); // if loop is true, it needs to be disabled first before starting a new one!!!
	}

	private IEnumerator HitSequence(int amount)
	{
		//Debug.Log("HitSequence started with amount: " + amount.ToString());
		if (amount == -1){
			hitLooping = true;
			hitTimes = 9999;
			amount = 3; // this is the amount for the looping effect.
		}
		else
		{
			hitLooping = false;
			hitTimes = 1;
		}
		
		int startFrame = 4 - amount;
		int endFrame = 4;
		
		for (int t=0; t<hitTimes; t++)
		{
			hitImage.enabled = true;
			for (int a=startFrame; a<endFrame; a++)
			{
				hitImage.sprite = hammer2InterfaceAtlas.Get("Hit" + a.ToString());
				yield return new WaitForSeconds(0.25f);
			}
			hitImage.enabled = false;
			yield return new WaitForSeconds(0.25f);
		}
		
		Debug.Log("HitSequence ended!");
	}

	// Hack: Needed for the land and jump particles.
	public void CreateParticleGameObject(GameObject prefab, float lifeTime)
	{
		GameObject particleGameObject = Instantiate(prefab) as GameObject;
		particleGameObject.transform.localPosition = Scripts.hammer.transform.position;
		particleGameObject.AddComponent<AutoDestruct>().time = lifeTime;
	}
}
