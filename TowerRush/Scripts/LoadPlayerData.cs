using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SaveManager;

public class LoadPlayerData : MonoBehaviour
{

    public static LoadPlayerData Instance;

    public delegate void SavedDataLoaded();
    public static event SavedDataLoaded OnSavedDataLoaded;



    private void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    private void OnEnable()
    {
        SaveSystem.OnSavedDataLoaded += LoadSavedData;
    }
    private void OnDisable()
    {
        Save();
        SaveSystem.OnSavedDataLoaded -= LoadSavedData;
    }


    public void PopulateData()
    {

    }

    public void Save()
    {
        SaveSystem.SaveGame();
    }

    public void CheckDataCount()
    {
        Debug.LogFormat("Soldiers: {0} - Castles: {1} - Kingdoms: {2}", GameManager.GetAllSoldiers().Count(), GameManager.GetAllCastles().Count(), GameManager.GetAllKingdoms().Count());
    }



    public void ModifyData()
    {
        List<Castle> _allCastles = GameManager.GetAllCastles();
        for (int i = 0; i < _allCastles.Count; i++)
        {
            Castle _c = _allCastles[i];

            _c.CastleTier += 2;
        }

        List<Soldier> _allSoldiers = GameManager.GetAllSoldiers();
        for (int i = 0; i < _allSoldiers.Count; i++)
        {
            Soldier _s = _allSoldiers[i];

            _s.Health += 2;
        }
        List<Kingdom> _allKingdoms = GameManager.GetAllKingdoms();
        for (int i = 0; i < _allKingdoms.Count; i++)
        {
            Kingdom _s = _allKingdoms[i];

            _s.ConqueredRegionProgress += 2;
        }
    }

    public void LoadGame()
    {
        Debug.Log("LoadPlayerData: LoadGame()");
        LoadSavedData();
    }

    void LoadSavedData()
    {
        Debug.Log("LoadPlayerData: LoadGame()");

        if (!PlayerPrefs.HasKey("GameData") || SaveSystem.SavedData == null)
        {

            Debug.LogFormat("Saved Data : Key Present: {0} -- SavedData: {1}", PlayerPrefs.GetInt("GameData"), SaveSystem.SavedData);
            return;
        }


        SaveManager savedData = SaveSystem.SavedData;


        GameManager.SetResourceCount(savedData.ResourcesCount);
        GameManager.SetGemsCount(savedData.GemsCount);

        #region Loading Kingdoms
        List<SaveSystemKingdom> _allKingdoms = savedData.KingdomList;

        for (int i = 0; i < _allKingdoms.Count; i++)
        {
            Kingdom kd = GameManager.GetKingdom(_allKingdoms[i].KingdomID); // just keeping a reference to store currently processed kingdom
            SaveSystemKingdom _saveKingdom = _allKingdoms[i];
            //  kd.KingdomID = _saveKingdom.KingdomID;
            kd.ConqueredRegionProgress = _saveKingdom.ConqueredRegionProgress;
            kd.KingdomName = _saveKingdom.KingdomName;
            kd.KingdomLore = _saveKingdom.KingdomLore;
            kd.KingdomLocked = _saveKingdom.KingdomLocked;
            kd.CastleList.Clear();
            kd.CastleList = new List<int>();

            List<int> _cList = _saveKingdom.CastleList;

            for (int j = 0; j < _cList.Count; j++)
            {
                kd.CastleList.Add(_cList[j]);
            }

            kd.SelectedSoldiersForBattle = new List<int>();

            List<int> _selectedSoldiersForBattleList = _saveKingdom.SelectedSoldiersForBattle;

            for (int j = 0; j < _selectedSoldiersForBattleList.Count; j++)
            {
                kd.AddSoldierToBattleTeam(_selectedSoldiersForBattleList[j]);
            }
            //GameManager.Instance.kingdomList.Add(_saveKingdom);
        }

        #endregion

        #region Loading Soldiers
        List<SaveSystemSoldier> _allSoldiers = savedData.AllSoldiersList;

        for (int i = 0; i < _allSoldiers.Count; i++)
        {
            SaveSystemSoldier _saveSoldier = _allSoldiers[i];
            Soldier _s = GameManager.GetSoldier(_allSoldiers[i].SoldierID);
            _s.CastleID = _saveSoldier.CastleID;
            _s.XpPoints = _saveSoldier.XpPoints;
            _s.SoldierLevel = _saveSoldier.SoldierLevel;
            _s.SummonCost = _saveSoldier.SummonCost;
            _s.UpgradeCost = _saveSoldier.UpgradeCost;
            _s.SoldierType = _saveSoldier.SoldierType;
            _s.HitPoints = _saveSoldier.HitPoints;
            _s.FactionName = _saveSoldier.FactionName;
            _s.SoldierName = _saveSoldier.SoldierName;
            _s.DamageTolerance = _saveSoldier.DamageTolerance;
            _s.SoldierID = _saveSoldier.SoldierID;
            _s.SoldierLocked = Convert.ToBoolean(_saveSoldier.SoldierLocked);
            _s.SoldierRarity = (SoldierRarity)Enum.Parse(typeof(SoldierRarity), _saveSoldier.SoldierRarity);
            //AllSoldiersList.Add(_saveSoldier);
        }

        #endregion

        #region Loading Castles

        List<SaveSystemCastle> _allCastleList = savedData.castleList;

        for (int i = 0; i < _allCastleList.Count; i++)
        {
            SaveSystemCastle _saveCastle = _allCastleList[i];

            Castle _c = GameManager.GetCastle(_allCastleList[i].CastleID);


            // _c.CastleID = _saveCastle.CastleID;
            _c.KingdomID = _saveCastle.KingdomID;
            _c.CastleTier = _saveCastle.CastleTier;
            _c.ProductionCapacity = _saveCastle.ProductionCapacity;
            _c.ProductionTime = _saveCastle.ProductionTime;



            _c.SoldiersList.Clear();
            _c.SoldiersList = new List<int>();

            List<int> _cList = _saveCastle.SoldiersList;

            for (int j = 0; j < _cList.Count; j++)
            {
                _c.SoldiersList.Add(_cList[j]);
            }
        }

        #endregion


        #region Loading Selected Kingdom
        Debug.Log("Trying to Load Selected Kindom");
        SaveSystemKingdom sKingdom = savedData.SelectedKingdom;
        if (sKingdom == null)
        {
            Debug.Log("Selected Kingdom Not found.. ");
            return;
        }
        Debug.Log("LoadPlayerData: Trying to get Kingdom: " + sKingdom.KingdomID);
        Kingdom _pKingdom = GameManager.GetKingdom(sKingdom.KingdomID);
        _pKingdom.KingdomID = sKingdom.KingdomID;
        _pKingdom.KingdomLocked = sKingdom.KingdomLocked;
        _pKingdom.KingdomName = sKingdom.KingdomName;
        _pKingdom.KingdomLore = sKingdom.KingdomLore;
        _pKingdom.SelectedSoldiersForBattle.Clear();
        _pKingdom.SelectedSoldiersForBattle = new List<int>();

        List<int> _kingdomBattleSoldiers = sKingdom.SelectedSoldiersForBattle;

        for (int i = 0; i < _kingdomBattleSoldiers.Count; i++)
        {
            _pKingdom.AddSoldierToBattleTeam(_kingdomBattleSoldiers[i]);
        }

        GameManager.SelectPlayerKingdom(_pKingdom.KingdomID, false);
        #endregion

        OnSavedDataLoaded?.Invoke();
    }
}