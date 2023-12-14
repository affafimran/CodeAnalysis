using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class Ads : MonoBehaviour
{
	[SerializeField]
	private Text crystalText;

	[SerializeField]
	private Text woodText;

	public float timeToShowAds = 180f;

	private float timer;

	private void Start()
	{
		//Advertisement.Initialize("3410347");
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer > timeToShowAds)
		{
			timer = 0f;
			//Advertisement.Show();
		}
	}

	public void OpenShop(GameObject g)
	{
		g.SetActive(value: true);
		string text3 = crystalText.text = (woodText.text = "+" + CurrencyManager.GetSuffix(DayNightManager._instance.dayNum * 150));
	}

	public void OnClickCrystal()
	{
		//ShowOptions showOptions = new ShowOptions
		//{
		//	resultCallback = CrystalCallback
		//};
		//Advertisement.Show("rewardedVideo", showOptions);
	}

	//private void CrystalCallback(ShowResult result)
	//{
	//	if (result == ShowResult.Finished)
	//	{
	//		MessageHandler._instance.ShowMessage("Reward collected", 1.5f, Color.magenta);
	//		CurrencyManager._instance.AddCrystal(DayNightManager._instance.dayNum * 150);
	//	}
	//}

	public void OnClickWood()
	{
		//ShowOptions showOptions = new ShowOptions
		//{
		//	resultCallback = WoodCallback
		//};
		//Advertisement.Show("rewardedVideo", showOptions);
	}

	//private void WoodCallback(ShowResult result)
	//{
	//	if (result == ShowResult.Finished)
	//	{
	//		MessageHandler._instance.ShowMessage("Reward collected", 1.5f, Color.green);
	//		CurrencyManager._instance.AddWood(DayNightManager._instance.dayNum * 150);
	//	}
	//}
}
