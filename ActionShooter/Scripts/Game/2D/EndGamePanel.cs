using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// EndGamePanel.
/// Handles the displaying of credits etc.
/// </summary>

public class EndGamePanel : MonoBehaviour {

	public GameObject background;
	public GameObject brandingButton;
	public GameObject header;
	public GameObject content;
	public GameObject comment;
	public GameObject endGameBackButton;

	void Start ()
	{
		Scripts.audioManager.StopAllSFX();
		Scripts.audioManager.StopAllMusic();

		foreach (Transform t in this.transform) t.gameObject.SetActive(false);

		StartCoroutine("EndGameSequence");
	}

	private IEnumerator EndGameSequence()
	{
		Debug.Log("[EndGamePanel] EndGameSequence started.");

		background.SetActive(true);

		Scripts.interfaceScript.Fade("FadeInMenu");
		Scripts.audioManager.PlayMusic("EndGame",1.0f, -1);

		yield return new WaitForSeconds(3.0f);

		header.SetActive(true);

		yield return new WaitForSeconds(3.0f);

		content.SetActive(true);

		yield return new WaitForSeconds(3.0f);

		StartCoroutine("CommentSequence");

		yield return new WaitForSeconds(6.0f);
		
		brandingButton.SetActive(true);

		endGameBackButton.SetActive(true);

		Debug.Log("[EndGamePanel] EndGameSequence started.");

	}

	// Changes some of the text over time for fun.
	private IEnumerator CommentSequence()
	{
		XTweenAlpha myTween = comment.GetComponent<XTweenAlpha>();
		Text myText = comment.GetComponent<Text>();

		int i = 1;

		for (i=1; i<6; i++)
		{
			comment.SetActive(true);
			myText.text = XLocalization.Get("EndGameComment" + i.ToString() + "Text");
			myTween.GetComponent<XTweenAlpha>().enabled = true;

			yield return new WaitForSeconds(12.0f);

			if (i==5) i = 0;
		}
	}

}
