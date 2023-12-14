using UnityEngine;

public class ShooterBuilding : MonoBehaviour
{
	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private Transform firePoint;

	public int damage = 1;

	public float fireRate = 2f;

	public float rangeRadius;

	public Transform radiusTransform;

	private Transform target;

	private float shootTimer;

	public bool isWizardTower;

	[SerializeField]
	private Laser[] lasers;

	private void Start()
	{
		if (!isWizardTower)
		{
			InvokeRepeating("UpdateTarget", 0f, 0.5f);
		}
		if (radiusTransform != null)
		{
			radiusTransform.localScale = Vector3.one * rangeRadius * 2f;
		}
	}

	private void Update()
	{
		shootTimer += Time.deltaTime;
		if (isWizardTower)
		{
			Laser[] array = lasers;
			foreach (Laser laser in array)
			{
				if (laser.target == null)
				{
					laser.lr.enabled = false;
				}
				else
				{
					laser.lr.SetPosition(1, laser.target.transform.position);
				}
			}
		}
		if (!(shootTimer > 1f / fireRate))
		{
			return;
		}
		shootTimer = 0f;
		if (isWizardTower)
		{
			int num = 0;
			foreach (Enemy enemy in EnemySpawner._instance.enemies)
			{
				if (num <= 2 && Vector3.Distance(base.transform.position, enemy.transform.position) <= rangeRadius)
				{
					lasers[num].target = enemy;
					lasers[num].lr.enabled = true;
					lasers[num].lr.SetPosition(0, firePoint.position);
					num++;
				}
			}
			Laser[] array = lasers;
			foreach (Laser laser2 in array)
			{
				if (!(laser2.target == null))
				{
					laser2.target.TakeDamage((float)damage / 5f);
				}
			}
		}
		else if (target != null)
		{
			Shoot();
		}
	}

	private void UpdateTarget()
	{
		float num = float.PositiveInfinity;
		Enemy enemy = null;
		foreach (Enemy enemy2 in EnemySpawner._instance.enemies)
		{
			float num2 = Vector3.Distance(base.transform.position, enemy2.transform.position);
			if (num2 < num)
			{
				num = num2;
				enemy = enemy2;
			}
		}
		if (enemy != null && num <= rangeRadius)
		{
			target = enemy.transform;
			firePoint.transform.localEulerAngles = new Vector3(0f, 0f, 90f + PlayerMovement.AngleBetweenTwoPoints(base.transform.position, target.position));
		}
		else
		{
			target = null;
		}
	}

	private void Shoot()
	{
		Bullet component = UnityEngine.Object.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).GetComponent<Bullet>();
		if (component != null)
		{
			component.damage = damage;
		}
	}

	public void OnClick()
	{
		if (radiusTransform != null)
		{
			radiusTransform.gameObject.SetActive(value: true);
		}
	}

	public void Deselect()
	{
		if (radiusTransform != null)
		{
			radiusTransform.gameObject.SetActive(value: false);
		}
	}
}
