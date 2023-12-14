using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public float hp;

	public float maxHp;

	public float hpMultiplier = 1f;

	public float dmgMultiplier = 1f;

	[SerializeField]
	private Image hpBar;

	[SerializeField]
	private float speed;

	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private Transform skinTransform;

	public Animator animator;

	[SerializeField]
	private GameObject[] deathPrefabs;

	private float timer;

	private float collisionStayTimer;

	public float damage;

	private Transform canvasTransform;

	public bool isAnimated;

	private Rigidbody2D rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		hp *= hpMultiplier;
		hp *= 1f - (float)Upgrades._instance.zombieHp.bonus / 100f;
		maxHp = hp;
		damage *= dmgMultiplier;
		hpBar.fillAmount = 1f;
		hpBar.transform.parent.gameObject.SetActive(value: false);
		maxSpeed = speed;
		if (!isAnimated)
		{
			StartCoroutine(MoveRotation());
		}
		UpdateTarget();
		canvasTransform = GetComponentInChildren<Canvas>().transform;
		if (isAnimated)
		{
			animator.Play("Move");
		}
	}

	private void Update()
	{
		rb.velocity = Vector2.zero;
		base.transform.Translate(speed * Time.deltaTime * Vector3.right);
		timer += Time.deltaTime;
		collisionStayTimer -= Time.deltaTime;
		if (timer > 1f)
		{
			UpdateTarget();
			timer = 0f;
		}
		canvasTransform.eulerAngles = Vector3.zero;
	}

	public void TakeDamage(float dmg)
	{
		hpBar.transform.parent.gameObject.SetActive(value: true);
		hp -= dmg;
		hpBar.fillAmount = hp / maxHp;
		if (hp <= 0f)
		{
			EnemySpawner._instance.EnemyDestroyed(this);
			Object.Instantiate(deathPrefabs[Random.Range(0, deathPrefabs.Length)]).transform.position = base.transform.position;
			CurrencyManager._instance.AddEssence(1);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void UpdateTarget()
	{
		float num = float.PositiveInfinity;
		Building building = null;
		foreach (Building allPlacedBuilding in GameManager._instance.allPlacedBuildings)
		{
			float num2 = Vector3.Distance(base.transform.position, allPlacedBuilding.transform.position);
			if (num2 < num)
			{
				num = num2;
				building = allPlacedBuilding;
			}
		}
		base.transform.localEulerAngles = new Vector3(0f, 0f, 180f + PlayerMovement.AngleBetweenTwoPoints(base.transform.position, building.transform.position));
		if (isAnimated)
		{
			skinTransform.localEulerAngles = -base.transform.localEulerAngles;
			if (base.transform.localEulerAngles.z > 90f && base.transform.localEulerAngles.z < 270f)
			{
				skinTransform.localScale = new Vector3(0f - Mathf.Abs(skinTransform.localScale.x), skinTransform.localScale.y, 0f);
			}
			else
			{
				skinTransform.localScale = new Vector3(Mathf.Abs(skinTransform.localScale.x), skinTransform.localScale.y, 0f);
			}
		}
	}

	private IEnumerator MoveRotation()
	{
		int multiplier = 1;
		while (true)
		{
			if (skinTransform.localEulerAngles.z > 15f)
			{
				multiplier = -1;
			}
			if (skinTransform.localEulerAngles.z > 345f)
			{
				multiplier = 1;
			}
			skinTransform.localEulerAngles = new Vector3(0f, 0f, skinTransform.localEulerAngles.z + Time.deltaTime * 20f * (float)multiplier);
			yield return null;
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Building")
		{
			if (isAnimated)
			{
				animator.Play("Attack");
			}
			if (collisionStayTimer <= 0f)
			{
				collisionStayTimer = 0.5f;
				if (collision.gameObject.GetComponent<Building>() != null)
				{
					collision.gameObject.GetComponent<Building>().TakeDamage(damage);
				}
				else
				{
					collision.transform.parent.gameObject.GetComponent<Building>().TakeDamage(damage);
				}
			}
		}
		if (collisionStayTimer <= 0f && collision.gameObject.tag == "Soldier")
		{
			animator.Play("Attack");
			collisionStayTimer = 0.5f;
			if (collision.gameObject.GetComponent<PlayerNPC>() != null)
			{
				collision.gameObject.GetComponent<PlayerNPC>().TakeDamage(damage);
			}
		}
		if (collisionStayTimer <= 0f && collision.gameObject.tag == "Player")
		{
			animator.Play("Attack");
			collisionStayTimer = 0.5f;
			if (collision.gameObject.GetComponent<PlayerHp>() != null)
			{
				collision.gameObject.GetComponent<PlayerHp>().TakeDamage(damage);
			}
		}
	}

	public IEnumerator Burn()
	{
		while (hp > 0f)
		{
			TakeDamage(maxHp / 100f);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void Slow()
	{
		speed = maxSpeed * (1f - (float)Upgrades._instance.slow.bonus / 100f);
	}
}
