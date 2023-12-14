using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageHandler : MonoBehaviour
{
	public static MessageHandler _instance;

	public Text messageText;

	public Image backgroundImage;

	private string prevText = "";

	private void Awake()
	{
		_instance = this;
	}

	public void ShowMessage(string text, float duration, Color c)
	{
		if (!(prevText == text))
		{
			prevText = text;
			messageText.text = text;
			StartCoroutine(MessageCoroutine(text, duration, c));
		}
	}

	private IEnumerator MessageCoroutine(string text, float duration, Color c)
	{
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 3f;
			messageText.color = new Color(c.r, c.g, c.b, t);
			backgroundImage.color = new Color(0f, 0f, 0f, t / 1.2f);
			yield return null;
		}
		yield return new WaitForSeconds(duration);
		while (t > 0f)
		{
			t -= Time.deltaTime * 3f;
			messageText.color = new Color(c.r, c.g, c.b, t);
			backgroundImage.color = new Color(0f, 0f, 0f, t / 1.2f);
			yield return null;
		}
		messageText.color = new Color(1f, 1f, 1f, 0f);
		backgroundImage.color = new Color(0f, 0f, 0f, 0f);
		prevText = "";
	}
}
