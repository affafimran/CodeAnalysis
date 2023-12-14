using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
	[SerializeField]
	private float fireRate = 1.5f;

	[SerializeField]
	private float damage;

	private float timer;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private Transform fireTransform;

	private void Start()
	{
	}

	private void Update()
	{
		if (!GameManager._instance.isPaused)
		{
			timer += Time.deltaTime;
			if (Input.GetMouseButton(0) && 1f / (fireRate * (1f + (float)Upgrades._instance.atkSpeed.bonus / 100f)) < timer)
			{
				timer = 0f;
				Shoot();
			}
		}
	}

	private void Shoot()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(bulletPrefab);
		gameObject.transform.position = fireTransform.position;
		gameObject.transform.localEulerAngles = base.transform.localEulerAngles - Vector3.forward * 90f;
		gameObject.GetComponent<Bullet>().damage = damage * (float)Upgrades._instance.playerDmg.bonus;
		gameObject.GetComponent<Bullet>().playerBullet = true;
	}
}
