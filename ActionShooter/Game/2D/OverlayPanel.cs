using UnityEngine;
using System.Collections;

/// <summary>
/// OverlayPanel.
/// Handles thing on top of the game screen, other than fades and achievements.
/// Could also be used for vignetting etc.
/// Now it's used for the bars you see in an in-game cinematic.
/// Which aren't that great because they're scale is wrong on some resolutions.
/// </summary> 

public class OverlayPanel : MonoBehaviour {

	public GameObject barsIn;
	public GameObject barsInTop;
	public GameObject barsInBottom;
	public GameObject barsOut;
	public GameObject barsOutTop;
	public GameObject barsOutBottom;

	public void ActivateOverlay(string type)
	{
		switch (type)
		{
		case "BarsIn":
			barsIn.SetActive(true);
			foreach (XTweener xt in barsIn.GetComponentsInChildren<XTweener>()) xt.enabled = true;
			break;
		case "BarsOut":
			barsIn.SetActive (false);
			barsOut.SetActive(true);
			foreach (XTweener xt in barsOut.GetComponentsInChildren<XTweener>()) xt.enabled = true;
			break;
		default: Debug.Log("ActivateOverlay with unknown type called: " + type); break;
		}
	}
}
