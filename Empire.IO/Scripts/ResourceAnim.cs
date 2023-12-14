using System.Collections;
using UnityEngine;

public class ResourceAnim : MonoBehaviour
{
	private SpriteRenderer sr;

	public int value;

	[SerializeField]
	private TextMesh text;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		text.text = "+" + value;
		StartCoroutine(Anim());
	}

	private IEnumerator Anim()
	{
		float time = 1f;
		float t = 0f;
		while (t < time)
		{
			t += Time.deltaTime;
			base.transform.localPosition = Vector3.up * Mathf.Lerp(0.5f, 1.5f, t / time);
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t / time);
			text.color = new Color(1f, 1f, 1f, 1f - t / time);
			yield return null;
		}
	}
}
