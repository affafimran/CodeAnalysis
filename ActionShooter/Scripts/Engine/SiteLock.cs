// Stijn 20/09/2013 - SiteLock, would be nice to implement Reg.Exp. support here (from shareddata but needs proper parsing)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SiteLock : MonoBehaviour
{
	private static bool pForceSiteLock = false;  // only used to test the sitelock screen within Unity

	private static Dictionary<string, DicEntry> pSiteLockData = null;
	public static bool IsSiteLocked()
	{
		if (pForceSiteLock)
		{
			pSiteLockData = Data.Shared["SiteLock"].d;
			return true;  // only used to test the sitelock screen within Unity
		}

		if (Data.platform == "PC")
		{  // Site lock only makes sense for PC (webplayer)
			if (!Data.Shared.ContainsKey("SiteLock")) return false;  // no sitelock data at all
			pSiteLockData = Data.Shared["SiteLock"].d;

			if (pSiteLockData.ContainsKey(Data.branding))
			{  // This type of branding has a sitelock
				// if run local then we'll always allow it
				if (Application.absoluteURL.StartsWith("file://")) return false;

				// Get the domain name, that is:
				// in http://abc.123.com/somemap/index.html the domain is abc.123.com
				// first strip protocol off: http://
				string tDomain = Application.absoluteURL;
				int tSlashSlashIndex = tDomain.IndexOf("//", 0, 8);  // check if "//" occurs within 8 characters of the absoluteURL
				if (tSlashSlashIndex >= 0)  // strip protocol off
					tDomain = tDomain.Substring(tSlashSlashIndex+2);
				// now strip off everything after the domain
				int tSlashIndex = tDomain.IndexOf("/");
				if (tSlashIndex >= 0)  // would be weird if this slash would not be found
					tDomain = tDomain.Substring(0, tSlashIndex);
				tDomain = tDomain.ToLower();  // not sure if this is needed, but doing it

				// TEST:
				// Debug.LogWarning("Application.absoluteURL: " + Application.absoluteURL);
				// Debug.LogWarning("tSlashSlashIndex: " + tSlashSlashIndex);
				// Debug.LogWarning("tSlashIndex: " + tSlashIndex);
				// Debug.LogWarning("FinalDomain: " + tDomain);

				// first check the always allow domains
				foreach (DicEntry tDicEntry in pSiteLockData["AlwaysAllow"].l)
					if (tDomain.EndsWith(tDicEntry.s.Replace("\\", "/")))
						return false;

				// check if still sitelocked (we're not in the always allowed domains
				foreach (DicEntry tDicEntry in pSiteLockData[Data.branding].l)
					if (tDomain.EndsWith(tDicEntry.s.Replace("\\", "/")))
						return false;

				return true;  // here it's locked
			}
		}
		return false;  // here no sitelock is in effect
	}

	// when aForced is true, we redirect, otherwise we may not redirect depending on property ShowLockScreen
	// returns whether we redirected
	public static bool Redirect() { return Redirect(true); }
	public static bool Redirect(bool aForced)
	{
		if (Data.Shared == null) return false;  // can happen if we start in the SiteLock scene
		if (pSiteLockData.ContainsKey(Data.branding + "Redirect"))
		{
			Dictionary<string, DicEntry> tRedirectData = pSiteLockData[Data.branding + "Redirect"].d;
			if (aForced || !tRedirectData["ShowLockScreen"].b)
			{
				string tJavaCall = "window.open(\"" + tRedirectData["UrlPage"].s.Replace("\\", "/") + "\", \"_top\")";
				Application.ExternalEval(tJavaCall);
				return true;
			}
		} else
		{
			Debug.LogError("Missing \"Redirect\" property!");
		}
		return false;
	}

	// When starting in the SiteLock screen we want to go to the sitelock but Globals and SharedData have not been loaded
	// This will make sure it goes to start up but returns to the SiteLock scene so it can be tested
	void Awake()
	{
		if (pForceSiteLock) return;

		if (GameObject.Find("StartUp") == null)
		{  // could not find StartUp object, this means we started from the SiteLock scene
			pForceSiteLock = true;
			Debug.LogWarning("Started in SiteLock scene: you are testing the sitelock");
			Application.LoadLevel("StartUp");
		}
	}

	// This script should only be instantiated in the SiteLockScreen
	void Start()
	{
		// SiteLock redirect will immediately redirect if there is no screen to be shown for this branding
		gameObject.SetActive(SiteLock.Redirect(false));  // only show when we didn't redirect
		Languages.Init();
	}

//	// TODO TO BE COMMENTED AND REPLACED BY PROPER SITELOCK SCREEN (But not in this script!)
	// void OnGUI()
	// {
		// GUI.TextArea(new Rect(Screen.width*0.5f-80, Screen.height*0.5f-10, 160, 20), "LOCKED PLEASE VISIT:");
		// if (GUI.Button(new Rect(Screen.width*0.5f-80, Screen.height*0.5f+15, 160, 50), "HERE TO PLAY"))
			// SiteLock.Redirect();
	// }
}
