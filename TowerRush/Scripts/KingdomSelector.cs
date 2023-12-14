using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class KingdomSelector : MonoBehaviour
{


    public TextMeshProUGUI KingdomNameText;
    public Image Kingdomicon;

    public GameObject SelectedImage;
    public GameObject LockedImage;

    bool IsSoldierRosterTile;
    string KingdomName;
    int KingdomID;
    Button selectorBtn;


    public delegate void KingdomSelectedforPreview(Kingdom kingdom);
    public static event KingdomSelectedforPreview OnKingdomSelectedForPreview;


    private void OnEnable()
    {
        OnKingdomSelectedForPreview += UpdateSelectorUI;
        MainMenuViewManager.OnUIStateChanged += UpdateUI;
    }
    private void OnDisable()
    {
        OnKingdomSelectedForPreview -= UpdateSelectorUI;
        MainMenuViewManager.OnUIStateChanged += UpdateUI;
    }

    private void UpdateUI(UIState currentState)
    {
        if (currentState == UIState.SoldierRosters)
            SelectedImage.SetActive(KingdomID == GameManager.GetPlayerKingdom().KingdomID);
    }


    private void UpdateSelectorUI(Kingdom kingdom)
    {
        SelectedImage.SetActive(KingdomID == GameManager.GetPreviewedKingdom().KingdomID);
    }


    private void Awake()
    {
        selectorBtn = GetComponentInChildren<Button>();
    }
    public void SelectKindgom()
    {
        GameManager.SelectPlayerKingdom(KingdomID, false);
    }

    public void SetKingdomDetails(Kingdom kingdom, bool _isSoldierRosterTile)
    {
        KingdomID = kingdom.KingdomID;
        KingdomName = kingdom.KingdomName;
        IsSoldierRosterTile = _isSoldierRosterTile;
        Kingdomicon.sprite = ResourceManager.GetKingdomSprite(KingdomName, IsSoldierRosterTile);
        Kingdomicon.preserveAspect = true;
        
        if (KingdomNameText)
            KingdomNameText.text = KingdomName;
        selectorBtn.interactable = !kingdom.KingdomLocked;
        LockedImage.SetActive(kingdom.KingdomLocked);
    }


    public void ShowKingdomDetails()
    {
        Debug.Log("Trying to show kingdom details");
        GameManager.SetPreviewedKingdom(KingdomID);
        InvokeKingdomSelectedEvent(GameManager.GetPreviewedKingdom());
    }

    public static void InvokeKingdomSelectedEvent(Kingdom kingdom) {
        OnKingdomSelectedForPreview?.Invoke(GameManager.GetPreviewedKingdom());
    }
}