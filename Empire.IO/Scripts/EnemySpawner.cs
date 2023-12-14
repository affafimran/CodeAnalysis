using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
	public static EnemySpawner _instance;

	public EnemyType[] enemyTypes;

	public List<Enemy> enemies;

	public float timeBetweenSpawns;

	public int enemiesToSpawn;

	public int destroyedEnemies;

	[SerializeField]
	private Text enemyCount;

	[SerializeField]
	private Transform[] spawnPoints;

	public Coroutine spawnCoroutine;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		enemies = new List<Enemy>();
		enemyCount.gameObject.SetActive(value: false);
	}

	private void Update()
	{
	}

	public void StartWave()
	{
		spawnCoroutine = StartCoroutine(StartWaveCoroutine());
	}

	private IEnumerator StartWaveCoroutine()
	{
		int waveNum = DayNightManager._instance.dayNum;
		if (waveNum == 1)
		{
			enemiesToSpawn = 15;
		}
		else
		{
			enemiesToSpawn = 20 + Mathf.Min(waveNum + waveNum * 2, 150);
		}
		destroyedEnemies = 0;
		enemyCount.text = "0/" + enemiesToSpawn;
		enemyCount.gameObject.SetActive(value: true);
		int maxEnemyType = Mathf.Min(2 + waveNum / 3, enemyTypes.Length);
		timeBetweenSpawns = 0.6f - (float)maxEnemyType * 0.03f;
		for (int i = 0; i < enemiesToSpawn; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(enemyTypes[GetRandom(maxEnemyType)].prefab, spawnPoints[Random.Range(0, spawnPoints.Length)]);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.GetComponent<Enemy>().hp = GetHp(waveNum, 1f);
			enemies.Add(gameObject.GetComponent<Enemy>());
			yield return new WaitForSeconds(timeBetweenSpawns);
		}
		yield return null;
	}

	private int GetRandom(int max)
	{
		int num = 0;
		for (int i = 0; i < max; i++)
		{
			num += enemyTypes[i].chance;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		for (int j = 0; j < max; j++)
		{
			if (enemyTypes[j].start <= num2 && enemyTypes[j].start + enemyTypes[j].chance > num2)
			{
				return j;
			}
		}
		return 0;
	}

	private int GetHp(int waveNum, float multiplier)
	{
		return Mathf.Min(1000, (int)((float)(10 * waveNum) * (0.8f + (float)waveNum / 20f) * multiplier));
	}

	public void EnemyDestroyed(Enemy e)
	{
		enemies.Remove(e);
		destroyedEnemies++;
		enemyCount.text = destroyedEnemies + "/" + enemiesToSpawn;
		if (enemiesToSpawn <= destroyedEnemies && enemies.Count == 0)
		{
			DayNightManager._instance.timeMultiplier = 20f;
			enemyCount.gameObject.SetActive(value: false);
			StartCoroutine(WaveManager._instance.Victory());
		}
	}
}
