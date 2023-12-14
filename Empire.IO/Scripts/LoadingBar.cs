using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
	[SerializeField]
	private Image fillBar;

	private float timer;

	private void Update()
	{
		timer += Time.deltaTime;
		fillBar.fillAmount = timer / 3f;
	}
}
