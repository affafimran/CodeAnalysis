using UnityEngine;
using System.Collections;

public class Languages
{

	public static void Init()
	{
		// Set branding for localization, thus logo and URL.
		// For now, the language and localization file are tied to the specified branding.
		// We might change this in a future build, that the language can be set independent of the branding.
		// Beware: if you want to use localization in the startup and loading scene
		// You need to make sure it's available in the resources at that moment!

		Debug.Log("[Languages] Init called.");
		Debug.Log("[Languages] Branding is: " + Data.branding );

		string selectedLanguage = "English (US)"; // The default language.

		switch (Data.branding)
		{
			case "None": selectedLanguage = "English (US)"; break;
			case "Xform": selectedLanguage = "English (US) Xform"; break;
			case "XformMobile": selectedLanguage = "English (US) XformMobile"; break;
			case "UnityAssetStore": selectedLanguage = "English (US) UnityAssetStore"; break;
			default:
			Debug.LogWarning("[Languages] Unknown branding! Using language: English (US).");
			selectedLanguage = "English (US)";
			break;
		}

		XLocalization.language = selectedLanguage;
		Debug.Log("[Languages] Language set to: " + XLocalization.language);
	}
}

