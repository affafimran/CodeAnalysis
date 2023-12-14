using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float speed = 5f;

	public float damage;

	public bool isAreaDamage;

	public bool playerBullet;

	[SerializeField]
	private GameObject effectPrefab;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Vector3.up * speed * Time.deltaTime);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (isAreaDamage)
		{
			Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, 0.7f);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].GetComponent<Enemy>() != null)
				{
					collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);
				}
			}
			Object.Instantiate(effectPrefab).transform.position = base.transform.position;
		}
		else if (collision.gameObject.tag == "Enemy")
		{
			Enemy component = collision.gameObject.GetComponent<Enemy>();
			component.TakeDamage(damage);
			if (playerBullet)
			{
				component.Slow();
			}
			if (effectPrefab != null)
			{
				Object.Instantiate(effectPrefab).transform.position = base.transform.position;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
	}
}
