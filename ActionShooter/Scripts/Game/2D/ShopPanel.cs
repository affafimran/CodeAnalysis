using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// ShopPanel.
/// Builds the shop item buttons.
/// Show the correct information on each item when clicking on them.
/// </summary> 

public class ShopPanel : MonoBehaviour {

	public GameObject background;
	public GameObject brandingButton;
	public GameObject header;
	public GameObject shopItemPrefab; // IMPORTANT: This is the base object used for instancing!
	public GameObject shopItems; // Parent object for the items. Not the manager.
	public GameObject shopBackButton;
	public GameObject information;
	public GameObject informationHeader;
	public GameObject informationDescription;
	private string informationShopItem; // This one is important since this is the one we want to buy!
	public GameObject buyButton;
	public GameObject buyButtonText;
	public GameObject owned;
	public GameObject notEnoughCash;
	public GameObject cash;
	public GameObject cashValue;

	private Atlas hammer2ShopItemsAtlas;

	void Awake()
	{
		hammer2ShopItemsAtlas = AtlasManager.hammer2ShopItemsAtlas;
	}

	void OnEnable()
	{
		foreach(Transform t in transform) t.gameObject.SetActive(false);

		StartCoroutine(ShopPanelSequence());
	}

	void Update ()
	{
		// This is here in update since sometimes the cash amount is influenced elsewehere.
		cashValue.GetComponent<Text>().text = "$" + GenericFunctionsScript.AddSeparatorInInt(GameData.cash, ",");

		#if UNITY_ANDROID
		if (Input.GetKeyUp(KeyCode.Escape)) Scripts.interfaceScript.OnButton(shopBackButton);
		#endif

	}

	// Buttons are referred here by the InterfaceScript.
	public void OnButton(string aButtonName)
	{
		switch (aButtonName)
		{
		case "BuyButton":
		if (ShopItemManager.Buy(informationShopItem))
		{
			Scripts.audioManager.PlaySFX("Interface/Buy");
			BuildShopItems();
			UpdateInformation(informationShopItem);
		}
		else
		{
			Scripts.audioManager.PlaySFX("Interface/Unavailable");
			notEnoughCash.SetActive(true);
			Invoke("DeactivateCashWarning", 1.0f);
		}

		break;
		
		default: // in this case default means a shop item button
			foreach (Transform t in shopItems.transform) t.Find("Selector").gameObject.SetActive(false);
			GameObject.Find(aButtonName).transform.Find("Selector").gameObject.SetActive(true);
			GameObject.Find(aButtonName).transform.Find("New").gameObject.SetActive(false);
			UpdateInformation(aButtonName);
			Scripts.audioManager.PlaySFX("Interface/Equip");
		break;
		}
	}

	private IEnumerator ShopPanelSequence()
	{
		Debug.Log("ShopPanelSequence started!");
		
		background.SetActive(true);
		header.SetActive(true);

		yield return new WaitForSeconds(0.1f);

		shopItems.SetActive(true);
		BuildShopItems();

		yield return new WaitForSeconds(0.1f);

		information.SetActive(true);

		UpdateInformation("ShopItem1");

		yield return new WaitForSeconds(0.1f);

		cash.SetActive(true);
		UpdateCash();

		yield return new WaitForSeconds(0.1f);

		brandingButton.SetActive(true);
		shopBackButton.SetActive(true);

		Debug.Log("ShopPanelSequence done!");
	}

	public void BuildShopItems()
	{
		// remove the old ones.
		foreach (Transform shopItem in shopItems.transform) Destroy (shopItem.gameObject);
		
		// Setup some temporary variables for the building
		GameObject _clone;
		Transform _t;
		Vector3 _pos = new Vector3();
		
		int columnSpacing = 128;
		//		int rowSpacing = 128;
		
		bool alreadyBought;
		bool canAfford;
		
		for (int i = 0; i<15; i++)
			
		{
			if (i <= 4) { _pos.x = -256.0f + (i * columnSpacing); _pos.y = 146.0f;}
			if (i > 4 && i <= 9) { _pos.x = -256.0f + ((i-5) * columnSpacing); _pos.y = 42.0f;}
			if (i > 9 && i <= 14) { _pos.x = -256.0f + ((i-10) * columnSpacing); _pos.y = -58.0f;}

			// Create the clones and position them.
			_clone = Instantiate(shopItemPrefab, transform.position, transform.rotation) as GameObject;
			_t = _clone.GetComponent<Transform>();
			_t.SetParent(shopItems.transform);
			_t.localScale = new Vector3 (1,1,1);
			_t.localPosition = _pos;

			_clone.name = "ShopItem" + (i+1).ToString();
			_clone.GetComponent<Image>().sprite = hammer2ShopItemsAtlas.Get("ShopItem" + (i+1).ToString());
			
			alreadyBought = ShopItemManager.IsBought(_clone.name);
			canAfford = ShopItemManager.CanAfford(_clone.name);

			_clone.transform.Find("Check").gameObject.SetActive(alreadyBought);
			_clone.transform.Find("New").gameObject.SetActive(canAfford && !alreadyBought);

			_clone.transform.Find("Selector").gameObject.SetActive(false);
		}
	}

	public void UpdateInformation(string shopItem)
	{
		informationShopItem = shopItem;
		informationHeader.GetComponent<Text>().text = XLocalization.Get(shopItem + "HeaderText");
		informationDescription.GetComponent<Text>().text = XLocalization.Get(shopItem + "DescriptionText");

		buyButtonText.GetComponent<Text>().text =  XLocalization.Get("BuyText") + " " + ShopItemManager.PriceAsString(shopItem);

		if (!ShopItemManager.IsBought(shopItem)) // If you don't have it....
		{
			buyButton.SetActive(true);
			owned.SetActive(false);
		}
		else
		{
			buyButton.SetActive(false);
			owned.SetActive(true);
		}
		notEnoughCash.SetActive(false);

		if (GameData.mobile && shopItem == "ShopItem5") buyButton.SetActive(false);

	}

	public void UpdateCash()
	{
		cashValue.GetComponent<Text>().text = "$" + GenericFunctionsScript.AddSeparatorInInt(GameData.cash, ",");
	}

	private void DeactivateCashWarning()
	{
		notEnoughCash.SetActive(false);
	}
}
