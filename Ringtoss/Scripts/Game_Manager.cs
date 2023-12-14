using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour {
	AudioSource audioSource;
	public AudioClip buttonClip;
	public GameObject rightPush;
	public GameObject leftPush;
	public GameObject rightBubbles;
	public GameObject leftBubbles;
	public GameObject bubbleParticales;
	[HideInInspector]
	public int noOfRingsOnRod = 0;
	[HideInInspector]
	public int levelNo;
	public GameObject[] levels;
	public int[] ringsInLevel;
	public GameObject[] timerStars;
	public GameObject RingsParent;
	private GameObject[] ringObj;
	private Rigidbody[] rigidbody;
	private Text timer;
	private Text levelCount;
	private Text ringsCount;
	[HideInInspector]
	public float currentTime = 0;
	[HideInInspector]
	public bool isGameFinished = false;

	//FBManager fBManager;
	// Use this for initialization
	void Start () {
		if(!PlayerPrefs.HasKey("GameScore"))
			PlayerPrefs.SetInt("GameScore", 0);
		levelNo = PlayerPrefs.GetInt("LevelNo");
		for(int i=0; i<levels.Length; i++)
			levels[i].SetActive(false);
		levels[levelNo].SetActive(true);

		audioSource = gameObject.GetComponent<AudioSource>();

		//fBManager = gameObject.GetComponent<FBManager>();
		//string playerStatus = fBManager.playerDetails();
		timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>();
		levelCount = GameObject.FindGameObjectWithTag("LevelCount").GetComponent<Text>();
		levelCount.text = "" + (levelNo + 1);
		ringsCount = GameObject.FindGameObjectWithTag("RingsCount").GetComponent<Text>();
		ringsCount.text = noOfRingsOnRod + "/" + ringsInLevel[levelNo];
		RingsParent = GameObject.FindGameObjectWithTag("RingsParent");
		ringObj = new GameObject[RingsParent.transform.childCount];
		rigidbody = new Rigidbody[RingsParent.transform.childCount];
		for(int i=0; i<RingsParent.transform.childCount; i++) {
			ringObj[i] = RingsParent.transform.GetChild(i).gameObject;
			rigidbody[i] = ringObj[i].GetComponent<Rigidbody>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		currentTime += 1 * Time.deltaTime;
		if(!checkIenumerator)
 		{
			 StartCoroutine(waitForShow());
			 checkIenumerator = !checkIenumerator;
		}
		if(!isGameFinished)
			if(noOfRingsOnRod >= ringsInLevel[levelNo])
				StartCoroutine(WaitForGameEnd());
			// if(noOfRingsOnRod >= ringsInLevel[levelNo])
			// 	isGameFinished = true;
	}

	IEnumerator WaitForGameEnd() {
		yield return new WaitForSeconds(3f);
			if(noOfRingsOnRod >= ringsInLevel[levelNo])
				isGameFinished = true;
			else
				isGameFinished = false;
	}

	bool checkIenumerator = false;
	IEnumerator waitForShow() {
		yield return new WaitForSeconds(0.5f);
		float minutes = (int) currentTime/60;
		float seconds = currentTime%60;
		timer.text = minutes + ":" + Mathf.RoundToInt(seconds);
		ringsCount.text = noOfRingsOnRod + "/" + ringsInLevel[levelNo];
		checkIenumerator = !checkIenumerator;
		if(minutes == 1 && timerStars[0].activeInHierarchy)
			timerStars[0].SetActive(false);
		else if (minutes == 2 && timerStars[1].activeInHierarchy)
			timerStars[1].SetActive(false);
	}

	public void AddForce(string btnType) {
		GameObject instantiatingBubbles;
		if(btnType.Equals("right")) {
			for(int i=0; i<rigidbody.Length; i++) {
				rigidbody[i].AddExplosionForce(500000 * Time.deltaTime, rightPush.transform.position, 100);
			}
			instantiatingBubbles = Instantiate(bubbleParticales, rightBubbles.transform);
		} else {
			for(int i=0; i<rigidbody.Length; i++) {
				rigidbody[i].AddExplosionForce(500000 * Time.deltaTime, leftPush.transform.position, 100);
			}
			instantiatingBubbles = Instantiate(bubbleParticales, leftBubbles.transform);
		}
		if(PlayerPrefs.GetInt("rollingBallVolume") == 1)
			audioSource.PlayOneShot(buttonClip);
		Destroy(instantiatingBubbles, 20);
	}
}
