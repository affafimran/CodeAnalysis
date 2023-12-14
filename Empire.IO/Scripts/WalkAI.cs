using System.Collections;
using UnityEngine;

public class WalkAI : MonoBehaviour
{
	private float timer;

	private Transform target;

	private Transform home;

	public float speed = 1.5f;

	private float currentSpeed;

	private float dmgTimer;

	private float damage;

	private float rotationOffset;

	[SerializeField]
	private Transform bodyTransform;

	private void Start()
	{
		currentSpeed = speed;
		damage = GetComponent<PlayerNPC>().damage;
		home = base.transform.parent;
	}

	private void Update()
	{
		timer += Time.deltaTime;
		dmgTimer += Time.deltaTime;
		if (timer > 2f && (target == null || target == home))
		{
			timer = 0f;
			UpdateTarget();
		}
		if (!(target == null) && !(Vector2.Distance(base.transform.position, target.position) < 0.025f))
		{
			bodyTransform.localEulerAngles = Vector3.forward * rotationOffset;
			base.transform.localEulerAngles = Vector3.forward * (-90f + PlayerMovement.AngleBetweenTwoPoints(base.transform.position, target.position));
			base.transform.Translate(Vector3.down * currentSpeed * Time.deltaTime);
		}
	}

	private void UpdateTarget()
	{
		float num = float.PositiveInfinity;
		Enemy enemy = null;
		foreach (Enemy enemy2 in EnemySpawner._instance.enemies)
		{
			float num2 = Vector3.Distance(base.transform.position, enemy2.transform.position);
			if (num2 < num && num2 < 10f)
			{
				num = num2;
				enemy = enemy2;
			}
		}
		if ((bool)enemy)
		{
			if (enemy.transform != target)
			{
				target = enemy.transform;
				base.transform.localEulerAngles = new Vector3(0f, 0f, -90f + PlayerMovement.AngleBetweenTwoPoints(base.transform.position, target.position));
			}
		}
		else
		{
			target = home;
		}
		currentSpeed = speed;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Enemy")
		{
			currentSpeed = 0f;
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Enemy" && dmgTimer > 1.2f)
		{
			dmgTimer = 0f;
			collision.gameObject.GetComponent<Enemy>().TakeDamage(damage * 2f);
			StartCoroutine(AttackAnim());
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Enemy")
		{
			currentSpeed = speed;
		}
	}

	private IEnumerator AttackAnim()
	{
		float t3 = 0f;
		while (t3 < 0.3f)
		{
			t3 += Time.deltaTime;
			rotationOffset = Mathf.Lerp(0f, 10f, t3 / 0.35f);
			yield return null;
		}
		t3 = 0f;
		while (t3 < 0.6f)
		{
			t3 += Time.deltaTime;
			rotationOffset = Mathf.Lerp(10f, -30f, t3 / 0.6f);
			yield return null;
		}
		t3 = 0f;
		while (t3 < 0.3f)
		{
			t3 += Time.deltaTime;
			rotationOffset = Mathf.Lerp(-30f, 0f, t3 / 0.3f);
			yield return null;
		}
		rotationOffset = 0f;
	}
}
