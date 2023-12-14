using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public enum UIState {
MainMenu,
Upgrades,
Store,
FractionSelection,
SoldierRosters,
KingdomDetails,
KingdomSelection,
Settings

}
public class MainMenuViewManager : MonoBehaviour
{

    [SerializeField]
    GameObject MainMenuPanel;
    [SerializeField]
    GameObject UpgradePanel;
    [SerializeField]
    GameObject StoresPanel;
    [SerializeField]
    GameObject FractionSelectionPanel;
    [SerializeField]
    GameObject SoldierRosterPanel;
    [SerializeField]
    GameObject SettingsPanel;
    [SerializeField]
    GameObject UnlockedSoldiersList;


    [Header("Kingdom Selection Option at the very start of the game")]
    [SerializeField]
    GameObject KingdomSelectorPanel;
    [SerializeField]
    GameObject KingdomSelectorParentPanel;


    [Header("Kingdom Selection from Soldier Selection Panel")]
    [SerializeField]
    GameObject SR_KingdomSelectorParentPanel;

    [Header("Kingdom Details")]
    [SerializeField]
    GameObject KingdomDetailsPanel;
    [SerializeField]
    TextMeshProUGUI KingdomDetailsNameText;
    [SerializeField]
    TextMeshProUGUI KingdomDetailsLoreText;
    [SerializeField]
    TextMeshProUGUI KingdomDetailsCastleCountText;
    [SerializeField]
    Image KingdomDetailsKingdomIconImage;


    [Header("Kingdom Soldiers in Soldier Rosters section")]
    [SerializeField]
    GameObject SR_AvailableSoldiersToSelectRootPanel;
    [SerializeField]
    GameObject SR_AvailableSoldiersToSelectParentPanel;
    [SerializeField]
    TextMeshProUGUI SR_CoinsCountText;
    [SerializeField]
    TextMeshProUGUI SR_GemsCountText;



    [Header("Soldiers in Battle Slots")]
    [SerializeField]
    GameObject[] SoldierForBattleSlots;


    [Header("Resources")]
    [SerializeField]
    TextMeshProUGUI CoinsCountText;
    [SerializeField]
    TextMeshProUGUI GemsCountText;



    [SerializeField]
    bool ShowKingdomSelectionOption;
    bool KingdomHasBeenPopulated;

    public delegate void UIStateChanged(UIState currentState);
    public static event UIStateChanged OnUIStateChanged;






    UIState CurrentUIState = UIState.MainMenu;







    private void OnEnable()
    {
        GameManager.OnKingdomSelected += () =>
        {
            CurrentUIState = UIState.MainMenu;
            OpenClosePanel(CurrentUIState);
        };

        KingdomSelector.OnKingdomSelectedForPreview += ShowKingdomDetails;
        KingdomSelector.OnKingdomSelectedForPreview += PopulateSoldiersInKingdomSoldiersPanel;
        Kingdom.OnSoldierSelectedForBattle += UpdateSoldierForBattleDetails;
    }

    private void OnDisable()
    {
        GameManager.OnKingdomSelected -= () =>
        {
            CurrentUIState = UIState.MainMenu;
            OpenClosePanel(CurrentUIState);
        };
        KingdomSelector.OnKingdomSelectedForPreview -= ShowKingdomDetails;
        KingdomSelector.OnKingdomSelectedForPreview -= PopulateSoldiersInKingdomSoldiersPanel;
        Kingdom.OnSoldierSelectedForBattle -= UpdateSoldierForBattleDetails;
    }

    void Start()
    {
        PopulateKindomSelectors();

        //if (!ShowKingdomSelectionOption)
        if (GameManager.HasKingdomBeenSelected())
            ShowMain();
        else
            ShowKingdomSelection();

        InvokeRepeating("UpdateUI", 0.25f, 0.25f);
    }

    void UpdateUI()
    {
        CoinsCountText.text = GameManager.GetResourcesCount().ToString("N0");
        SR_CoinsCountText.text = GameManager.GetResourcesCount().ToString("N0");

        GemsCountText.text = GameManager.GetGemsCount().ToString("N0");
        SR_GemsCountText.text = GameManager.GetGemsCount().ToString("N0");
    }

    public void ShowUpgrades()
    {
        CurrentUIState = CurrentUIState != UIState.Upgrades ? UIState.Upgrades : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }

    public void ShowStore()
    {
        CurrentUIState = CurrentUIState != UIState.Store ? UIState.Store : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);

    }
    public void ShowFractionSelection()
    {
        CurrentUIState = CurrentUIState != UIState.FractionSelection ? UIState.FractionSelection : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }

    public void ShowSoldierRosters()
    {
        CurrentUIState = CurrentUIState != UIState.SoldierRosters ? UIState.SoldierRosters : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }

    public void ShowKingdomSelection()
    {
        CurrentUIState = CurrentUIState != UIState.KingdomSelection ? UIState.KingdomSelection : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }
    public void ShowSettings()
    {
        CurrentUIState = CurrentUIState != UIState.Settings ? UIState.Settings : UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }

    public void ShowMain()
    {
        CurrentUIState = UIState.MainMenu;
        OpenClosePanel(CurrentUIState);
    }


    public void OpenClosePanel(UIState currentState)
    {
        //  Debug.Log("OpenClosePanel(): " + currentState);
        MainMenuPanel.SetActive(CurrentUIState == UIState.MainMenu);
        StoresPanel.SetActive(CurrentUIState == UIState.Store);
        UpgradePanel.SetActive(CurrentUIState == UIState.Upgrades);
        FractionSelectionPanel.SetActive(CurrentUIState == UIState.FractionSelection);
        SoldierRosterPanel.SetActive(CurrentUIState == UIState.SoldierRosters || CurrentUIState == UIState.KingdomDetails);
        KingdomDetailsPanel.SetActive(CurrentUIState == UIState.KingdomDetails && (GameManager.GetPreviewedKingdom().KingdomID != GameManager.GetPlayerKingdom().KingdomID));
        SettingsPanel.SetActive(CurrentUIState == UIState.Settings);
        KingdomSelectorPanel.SetActive(CurrentUIState == UIState.KingdomSelection);
        switch (currentState)
        {
            case UIState.MainMenu:
                break;
            case UIState.Upgrades:
                break;
            case UIState.Store:
                break;
            case UIState.FractionSelection:
                break;
            case UIState.SoldierRosters:
                PopulateSoldiersInKingdomSoldiersPanel(GameManager.GetPlayerKingdom());
                UpdateSoldierForBattleDetails(GameManager.GetPlayerKingdom());
                break;
            case UIState.KingdomDetails:
                break;

            case UIState.KingdomSelection:
                break;
            case UIState.Settings:
                break;
            default:
                break;
        }
        OnUIStateChanged?.Invoke(CurrentUIState);
    }

    void UpdateSoldierForBattleDetails(Kingdom kingdom)
    {
        Debug.LogFormat("UpdateSoldierForBattleDetails(): Slot Count: {0} while Soldier Count: {1}", SoldierForBattleSlots.Length, kingdom.SelectedSoldiersForBattle.Count);
        if (kingdom.SelectedSoldiersForBattle.Count == 0) // there are no soldiers in the kingdom army. resetting the slots.
        {
            for (int k = 0; k < SoldierForBattleSlots.Length; k++)
            {
                SoldierForBattleSlots[k].GetComponent<Image>().sprite = ResourceManager.GetSoldierSprite("Default", true);
            }
            return;
        }

        for (int i = 0; i < SoldierForBattleSlots.Length; i++)
        {
            for (int j = 0; j < kingdom.SelectedSoldiersForBattle.Count; j++)
            {
                if (i == j)
                {
                    Soldier _s = GameManager.GetSoldier(kingdom.SelectedSoldiersForBattle[j]);
                    SoldierForBattleSlots[i].GetComponent<Image>().sprite = ResourceManager.GetSoldierSprite(_s.SoldierName, true);
                    SoldierForBattleSlots[i].GetComponent<Image>().preserveAspect = true;
                }
                else
                {
                    if (i >= j)
                        SoldierForBattleSlots[i].GetComponent<Image>().sprite = ResourceManager.GetSoldierSprite("Default", true);
                }
            }
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("Farhan_mechanics");
    }

    public void BattleScene()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void PopulateSoldiersInKingdomSoldiersPanel(Kingdom kd)
    {

        foreach (Transform t in SR_AvailableSoldiersToSelectParentPanel.transform)
        {
            Destroy(t.gameObject);
        }

        List<Soldier> _soldiers = GameManager.GetAllSoldiersFromKingdom(kd);

        Debug.LogFormat("PopulateSoldiersInKingdomSoldiersPanel(): Soldiers Count{0}", _soldiers.Count);
        List<int> _populatedSoldierIDs = new List<int>();
        for (int i = 0; i < _soldiers.Count; i++)
        {
            if (_populatedSoldierIDs.Contains(_soldiers[i].SoldierID))
                return;

            GameObject soldierGO = Instantiate(GetSoldierSelectorPrefab(), SR_AvailableSoldiersToSelectParentPanel.transform);

            SoldierSelector selector = soldierGO.GetComponent<SoldierSelector>();
            _populatedSoldierIDs.Add(_soldiers[i].SoldierID);
            selector.SetSoldierDetails(_soldiers[i]);
        }
    }

    public void PopulateKindomSelectors()
    {
        if (KingdomHasBeenPopulated)
            return;

        Debug.Log("PopulateKindomSelectors(): ");
        List<Kingdom> _allKingdoms = GameManager.GetAllKingdoms();
        if (_allKingdoms.Count == 0)
            Debug.LogError("All kingdom Count is zero. Please start the game from Loading scene");

        for (int i = 0; i < _allKingdoms.Count; i++)
        {
            //if (ShowKingdomSelectionOption)
            if (!GameManager.HasKingdomBeenSelected())
            {
                GameObject _kingdomSelectorGO = Instantiate(GetKingdomSelectorPrefab(false));
                KingdomSelector _kingdomSelector = _kingdomSelectorGO.GetComponent<KingdomSelector>();

                _kingdomSelector.SetKingdomDetails(_allKingdoms[i], false);
                _kingdomSelectorGO.transform.parent = KingdomSelectorParentPanel.transform;
            }

            GameObject _kingdomSelectorinSoldierSelectionPanelGO = Instantiate(GetKingdomSelectorPrefab(true));
            KingdomSelector _kingdomSelectorinSoldierSelectionPanel = _kingdomSelectorinSoldierSelectionPanelGO.GetComponent<KingdomSelector>();

            _kingdomSelectorinSoldierSelectionPanel.SetKingdomDetails(_allKingdoms[i], false);
            _kingdomSelectorinSoldierSelectionPanel.transform.parent = SR_KingdomSelectorParentPanel.transform;
        }
        KingdomHasBeenPopulated = true;
    }

    GameObject GetKingdomSelectorPrefab(bool isSoldierSelectorTile)
    {
        GameObject _selector;
        if (isSoldierSelectorTile)
        {
            _selector = Resources.Load("Prefabs/Kingdoms/KingdomSelectorSmall") as GameObject;
        }
        else
        {
            _selector = Resources.Load("Prefabs/Kingdoms/KingdomSelector") as GameObject;
        }
        return _selector;
    }

    GameObject GetSoldierSelectorPrefab()
    {
        GameObject _selector;
        _selector = Resources.Load("Prefabs/Soldiers/SoldierSelector") as GameObject;

        return _selector;
    }

    public void ShowKingdomDetails(Kingdom _kingdom)
    {

        CurrentUIState = UIState.KingdomDetails;
        Debug.Log("Got inside ShowKingdomDetails()");
        KingdomDetailsNameText.text = "";
        KingdomDetailsLoreText.text = "";
        KingdomDetailsCastleCountText.text = "";

        KingdomDetailsNameText.text = "Kingdom Name: " + _kingdom.KingdomName;
        KingdomDetailsLoreText.text = "Kingdom Lore: " + _kingdom.KingdomLore.ToString();
        KingdomDetailsCastleCountText.text = "Number of Castles: " + _kingdom.CastleList.Count.ToString();
        KingdomDetailsKingdomIconImage.sprite = Resources.Load<Sprite>("Sprites/Tiles/KingdomIcons/" + _kingdom.KingdomName);

        OpenClosePanel(CurrentUIState);
    }

    public void SelectKingdomAsPlayerKingdom()
    {
        GameManager.SelectPlayerKingdom(GameManager.GetPreviewedKingdom().KingdomID, true);
        Debug.Log("SelectKingdomAsPlayerKingdom()");
        CurrentUIState = UIState.SoldierRosters;
        OpenClosePanel(CurrentUIState);
    }

    public void SaveDataManually()
    {

        LoadPlayerData.Instance.Save();
    }
}