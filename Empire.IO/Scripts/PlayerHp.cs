using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
	[SerializeField]
	private Image hpBar;

	[SerializeField]
	private Transform hpBg;

	public float hp;

	public float maxHp;

	[SerializeField]
	private GameObject deathPrefab;

	private void Start()
	{
		hp = maxHp;
	}

	private void Update()
	{
		if (hp < maxHp)
		{
			hp += Time.deltaTime / 2f;
		}
	}

	public void TakeDamage(float dmg)
	{
		hpBar.transform.parent.gameObject.SetActive(value: true);
		hp -= dmg;
		hpBar.fillAmount = hp / maxHp;
		if (hp <= 0f)
		{
			Object.Instantiate(deathPrefab).transform.position = base.transform.position;
			base.gameObject.SetActive(value: false);
			GameManager._instance.GameOver();
		}
	}

	public void SetHpBarPosition()
	{
		hpBg.localEulerAngles = Vector3.back * base.transform.eulerAngles.z;
		hpBg.position = base.transform.localPosition + Vector3.up * 0.5f;
	}
}
