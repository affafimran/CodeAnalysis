using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CheatManager
{
	/// <summary>
	/// Keeps track of cheats activation.
	/// This code used to be inside the interfacescript before it got too bulky.
	/// This script is now created in the Scripts script!
	/// The UpdateInput function is called from the LevelScript.
	/// Still need a good way to use cheats without a keyboard...
	/// </summary>

	private string pCheatText = "";

	public void Update()
	{
		if (Data.scene == "Menu") // The cheats can only be enabled during the menu.
		{
			if (Input.anyKey)
			{
				foreach (char tC in Input.inputString)
				{
					if (tC == "\n"[0] || tC == "\r"[0])
					{
						// check if pCheatText is correct
						if (pCheatText == GameData.cheatText)
						{
							Data.cheats = true;

							if (GameData.cheatText.IndexOf("m") > -1) // the infamouse "if the cheat contains an m" - hack
							{
								Data.muteAllSound = false;
								Data.sfx = true;
								Scripts.audioManager.MuteMusic(false);
								Data.music = true;
								Scripts.audioManager.MuteMusic(false);
							}

							Scripts.audioManager.PlaySFX("Interface/StopHammerTime");
							Debug.Log("[CheatManager] Cheats ENABLED!");
						}
						pCheatText = "";  // empty it
					} else
					{
						pCheatText += tC;
					}
				}
				if (Input.inputString.Length > 14) pCheatText = "";  // empty it
			}
		}


		if (Data.cheats)
		{
			// These cheats are always available, and hopefully won't cause problems.
			if (Input.GetKeyUp(KeyCode.I))
			{
				// Makes the interface appear and disappear. Handy for capturing.
				Scripts.interfaceScript.transform.GetComponent<Canvas>().enabled = !Scripts.interfaceScript.transform.GetComponent<Canvas>().enabled;
				Debug.Log("[CheatManager] I - Interface toggle");
			}

			// These cheats should only be available during the game!
			if (Scripts.interfaceScript.gamePanel.activeSelf)
			{
				if (Input.GetKeyUp(KeyCode.G))
				{
					// Makes the player invincible.
					GameData.godMode = !GameData.godMode;
					Scripts.hammer.characterData.godMode = GameData.godMode;
					Debug.Log("[CheatManager] G - Godmode toggle");
				}
				
				if (Input.GetKeyUp(KeyCode.K))
				{
					MissionManager.CompleteMission();
					Debug.Log("[CheatManager] K - Killed it! Victory!");
				}
				
				if (Input.GetKeyUp(KeyCode.L))
				{
					MissionManager.FailMission();
					Debug.Log("[CheatManager] L - Lose mission");
				}

				if (Input.GetKeyUp(KeyCode.R))
				{
					Scripts.hammer.weapon.rof = Scripts.hammer.weapon.sourceRof * 0.5f;
					Debug.Log("[CheatManager] R - Double rate of fire");
				}

				if (Input.GetKeyUp(KeyCode.O))
				{
					// Makes the player character appear and disappears. Also, handy for capturing.
					Scripts.hammer.gameObject.transform.Find("Hammer").GetComponent<SkinnedMeshRenderer>().enabled =  !Scripts.hammer.gameObject.transform.Find("Hammer").GetComponent<SkinnedMeshRenderer>().enabled;
					Scripts.hammer.gameObject.transform.Find("Hammer Bip").gameObject.SetActive(!Scripts.hammer.gameObject.transform.Find("Hammer Bip").gameObject.activeInHierarchy);
					Debug.Log("[CheatManager] O - Oh my! Where is the Hammer?");
				}

				// PickUp relates cheats are all mostly located on the numpad, according to the UI element screen layout ;)
				Vector3 pickUpPos = new Vector3(0,0,0);
				if (!Scripts.hammer.characterData.playerDead) pickUpPos = Scripts.hammer.transform.position + Scripts.hammer.transform.forward*5.0f;
				
				if (Input.GetKeyUp(KeyCode.Keypad0)) PickUpManager.AddPickUp("HealthBig", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.KeypadPeriod)) PickUpManager.AddPickUp("Explosive", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.KeypadEnter)) PickUpManager.AddPickUp("Weapon", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.KeypadPlus)) PickUpManager.AddPickUp("Gadget", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.KeypadMinus)) PickUpManager.AddPickUp("CashBriefcase", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.KeypadMultiply)) Scripts.interfaceScript.gamePanelScript.PickUp("Score", 10000);
				if (Input.GetKeyUp(KeyCode.KeypadDivide)) MissionManager.missionData.time -= 10000;

				if (Input.GetKeyUp(KeyCode.Keypad1)) PickUpManager.AddPickUp("CashSmall", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad2)) PickUpManager.AddPickUp("CashMedium", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad3)) PickUpManager.AddPickUp("CashBig", pickUpPos, true);

				if (Input.GetKeyUp(KeyCode.Keypad4)) PickUpManager.AddPickUp("Silver", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad5)) PickUpManager.AddPickUp("Gold", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad6)) PickUpManager.AddPickUp("Ruby", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad7)) PickUpManager.AddPickUp("Diamond", pickUpPos, true);

				if (Input.GetKeyUp(KeyCode.Keypad8)) PickUpManager.AddPickUp("ColdOne", pickUpPos, true);
				if (Input.GetKeyUp(KeyCode.Keypad9)) PickUpManager.AddPickUp("HiddenPackage", pickUpPos, true);

				return;
			}

			
			// Cheat that are available when you're NOT in the game itself, but in the menu.
			if (Input.GetKeyUp(KeyCode.T))
			{
				Scripts.medalsManager.CheatNearlyGetAllMedals();
				if (Scripts.interfaceScript.achievementsOverviewPanel.activeSelf) Scripts.interfaceScript.achievementsOverviewPanel.GetComponent<AchievementsOverviewPanel>().ReBuildAchievements();
				Debug.Log("[Cheats] T - Nearly get all medals");
			}
			
			if (Input.GetKeyUp(KeyCode.U))
			{
				GameData.unlockedMissions = 30; // hardcoded amount now.
				if (Scripts.interfaceScript.missionSelectPanel.activeSelf) Scripts.interfaceScript.missionSelectPanel.GetComponent<MissionSelectPanel>().ReBuildMissions();
				Debug.Log("[Cheats] U - Unlock 30 missions");
			}
			
			if (Input.GetKeyUp(KeyCode.C))
			{
				GameData.cash += 100000; // hardcoded amount now.
				if (Scripts.interfaceScript.shopPanel.activeSelf) Scripts.interfaceScript.shopPanel.GetComponent<ShopPanel>().BuildShopItems();
				Debug.Log("[Cheats] C - Give 100000 cash");
			}

		}
	}
}