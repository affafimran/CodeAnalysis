using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
	public float time;

	private float timer;

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer > time)
		{
			time = 0f;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
