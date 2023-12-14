using UnityEngine;
using UnityEngine.UI;

public class PlayerNPC : MonoBehaviour
{
	public float hp;

	public float maxHp;

	public float damage;

	[SerializeField]
	private Image hpBar;

	public SpawnBuilding spawnBuilding;

	private void Start()
	{
		maxHp = hp;
		hpBar.fillAmount = 1f;
	}

	private void Update()
	{
	}

	public void TakeDamage(float dmg)
	{
		hpBar.transform.parent.gameObject.SetActive(value: true);
		hp -= dmg;
		hpBar.fillAmount = hp / maxHp;
		if (hp <= 0f)
		{
			spawnBuilding.RemoveSoldier(base.gameObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
