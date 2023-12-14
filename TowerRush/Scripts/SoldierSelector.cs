using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoldierSelector : MonoBehaviour
{

    public TextMeshProUGUI SoldierNameText;
    public TextMeshProUGUI SoldierAttackPointsText;
    public TextMeshProUGUI SoldierDamageTolerancePointsText;
    public TextMeshProUGUI SoldierSelectedText;
    public TextMeshProUGUI SoldierLevelText;
    public TextMeshProUGUI UpgradeCostText;

    public GameObject UpgradeBtn;
    public GameObject EquipBtn;
    public GameObject UnlockBtn;
    public GameObject LockedImage;
    public Image SoldierIcon;
    string soldierName;
    float damageToleration;
    float hitPoints;
    int soldierID;
    bool soldierSelected;



    private void OnEnable()
    {
        GameManager.OnSoldierUpgraded += UpdateSoldierSelectorUI;
    }

    private void OnDisable()
    {
        GameManager.OnSoldierUpgraded -= UpdateSoldierSelectorUI;
    }


    public void SelectSoldier()
    {
        if (!soldierSelected)
        {
            GameManager.GetPlayerKingdom().AddSoldierToBattleTeam(soldierID);
            soldierSelected = true;
        }
        else
        {
            GameManager.GetPlayerKingdom().RemoveSoldierFromBattleTeam(soldierID);
            soldierSelected = false;
        }
        SoldierSelectedText.text = soldierSelected ? "UnEquip" : "Equip";
        Debug.Log("Soldier Selected: " + soldierSelected);
    }

    public void SetSoldierDetails(Soldier soldier)
    {
        soldierID = soldier.SoldierID;
        soldierName = soldier.SoldierName;
        damageToleration = soldier.DamageTolerance;
        hitPoints = soldier.HitPoints;

        UpdateSoldierSelectorUI(soldier);
    }

    private void UpdateSoldierSelectorUI(Soldier soldier)
    {
        if (soldier.SoldierID != soldierID)
            return;

        SoldierIcon.sprite = ResourceManager.GetSoldierSprite(soldierName, false);
        SoldierIcon.preserveAspect = true;
        SoldierNameText.text = soldierName;
        SoldierAttackPointsText.text = hitPoints.ToString();
        SoldierDamageTolerancePointsText.text = damageToleration.ToString("N0");
        SoldierLevelText.text = soldier.SoldierLevel.ToString("N0");
        UpgradeCostText.text = soldier.UpgradeCost.ToString("N0");
        LockedImage.SetActive(soldier.SoldierLocked);
        UnlockBtn.SetActive(soldier.SoldierLocked);
        EquipBtn.SetActive(!soldier.SoldierLocked);
        UpgradeBtn.SetActive(!soldier.SoldierLocked && GameManager.CanUpgradeSoldier(soldier.UpgradeCost));
    }

    public void ShowKingdomDetails()
    {
        Debug.Log("Trying to show kingdom details");
        //OnKingdomSelectedForPreview?.Invoke(GameManager.GetKingdom(KingdomID));
    }


    public void UpgradeSoldier()
    {
        GameManager.UpgradeSoldier(GameManager.GetSoldier(soldierID));
    }

    public void UnlockSoldier() {
        GameManager.UnlockSoldier(GameManager.GetSoldier(soldierID));

    }
}
