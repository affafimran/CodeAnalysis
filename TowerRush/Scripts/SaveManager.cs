using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[System.Serializable]
public class SaveManager
{


    #region gamemanager

    public List<SaveSystemKingdom> KingdomList = new List<SaveSystemKingdom>();
    public List<SaveSystemSoldier> AllSoldiersList = new List<SaveSystemSoldier>();
    public List<SaveSystemCastle> castleList = new List<SaveSystemCastle>();
    public List<SaveSystemPowerup> powerList = new List<SaveSystemPowerup>();
    public SaveSystemKingdom SelectedKingdom;

    public float ResourcesCount;
    public float GemsCount;
    #endregion


    public SaveManager()
    {

        ResourcesCount = GameManager.GetResourcesCount();
        GemsCount = GameManager.GetGemsCount();
        #region Saving Kingdoms
        List<Kingdom> _allKingdoms = GameManager.GetAllKingdoms();
        for (int i = 0; i < _allKingdoms.Count; i++)
        {
            Kingdom kd = _allKingdoms[i]; // just keeping a reference to store currently processed kingdom

            SaveSystemKingdom _saveKingdom = new SaveSystemKingdom();
            _saveKingdom.KingdomID = kd.KingdomID;
            _saveKingdom.ConqueredRegionProgress = kd.ConqueredRegionProgress;
            _saveKingdom.KingdomName = kd.KingdomName;
            _saveKingdom.KingdomLore = kd.KingdomLore;
            _saveKingdom.KingdomLocked = kd.KingdomLocked;
            _saveKingdom.CastleList = new List<int>();

            List<int> _cList = kd.CastleList;
            for (int j = 0; j < _cList.Count; j++)
            {
                _saveKingdom.CastleList.Add(_cList[j]);
            }
            _saveKingdom.SelectedSoldiersForBattle = new List<int>();

            List<int> _selectedSoldiersForBattleList = kd.SelectedSoldiersForBattle;

            for (int j = 0; j < _selectedSoldiersForBattleList.Count; j++)
            {
                _saveKingdom.SelectedSoldiersForBattle.Add(_selectedSoldiersForBattleList[j]);
            }

            KingdomList.Add(_saveKingdom);
        }

        #endregion

        #region Saving Soldiers
        List<Soldier> _allSoldiers = GameManager.GetAllSoldiers();
        for (int i = 0; i < _allSoldiers.Count; i++)
        {
            SaveSystemSoldier _saveSoldier = new SaveSystemSoldier();
            Soldier _s = _allSoldiers[i];

            _saveSoldier.CastleID = _s.CastleID;
            _saveSoldier.XpPoints = _s.XpPoints;
            _saveSoldier.SoldierLevel = _s.SoldierLevel;
            _saveSoldier.SummonCost = _s.SummonCost;
            _saveSoldier.UpgradeCost = _s.UpgradeCost;
            _saveSoldier.SoldierType = _s.SoldierType;
            _saveSoldier.HitPoints = _s.HitPoints;
            _saveSoldier.FactionName = _s.FactionName;
            _saveSoldier.SoldierName = _s.SoldierName;
            _saveSoldier.SoldierID = _s.SoldierID;
            _saveSoldier.DamageTolerance = _s.DamageTolerance;
            _saveSoldier.SoldierLocked = _s.SoldierLocked.ToString();
            _saveSoldier.SoldierRarity = _s.SoldierRarity.ToString();

            AllSoldiersList.Add(_saveSoldier);

        }

        #endregion

        #region Saving Castles

        List<Castle> _allCastleList = GameManager.GetAllCastles();

        for (int i = 0; i < _allCastleList.Count; i++)
        {
            Castle _c = _allCastleList[i];

            SaveSystemCastle _saveCastle = new SaveSystemCastle();

            _saveCastle.CastleID = _c.CastleID;
            _saveCastle.KingdomID = _c.KingdomID;
            _saveCastle.CastleTier = _c.CastleTier;
            _saveCastle.ProductionCapacity = _c.ProductionCapacity;
            _saveCastle.ProductionTime = _c.ProductionTime;


            List<int> _castleSoldiers = _c.SoldiersList;
            _saveCastle.SoldiersList = new List<int>();
            for (int j = 0; j < _castleSoldiers.Count; j++)
            {
                _saveCastle.SoldiersList.Add(_castleSoldiers[j]);
            }
            castleList.Add(_saveCastle);
        }

        #endregion



        #region Saving Player Kingdom

        SelectedKingdom = new SaveSystemKingdom();
        Kingdom _pKingdom = GameManager.GetPlayerKingdom();

        //if (_pKingdom == null) // TODO: Do some research on this.. _pKingdom is null while the ID is still present!!!!!
        //{
        //    Debug.Log("Selected kingdom is Empty: " + _pKingdom.KingdomID);
        //    return;
        //}

        SelectedKingdom.KingdomID = _pKingdom.KingdomID;
        SelectedKingdom.KingdomLocked = _pKingdom.KingdomLocked;
        SelectedKingdom.KingdomName = _pKingdom.KingdomName;
        SelectedKingdom.KingdomLore = _pKingdom.KingdomLore;

        SelectedKingdom.SelectedSoldiersForBattle = new List<int>();
        List<int> _kingdomBattleSoldiers = _pKingdom.SelectedSoldiersForBattle;

        for (int i = 0; i < _kingdomBattleSoldiers.Count; i++)
        {
            SelectedKingdom.SelectedSoldiersForBattle.Add(_kingdomBattleSoldiers[i]);
        }

        #endregion

    }


    [System.Serializable]
    public class SaveSystemSoldier
    {

        #region soldiers
        public int CastleID;
        public int XpPoints;
        public int SoldierLevel;
        public int SummonCost;
        public float UpgradeCost;
        public string SoldierType;
        public float HitPoints;
        public string FactionName;
        public string SoldierName;
        public int SoldierID;
        public string SoldierRarity;
        public string SoldierLocked;
        public float DamageTolerance;

        #endregion

    }




    [System.Serializable]
    public class SaveSystemKingdom
    {

        #region kingdom

        public int KingdomID;
        public List<int> CastleList;
        public float ConqueredRegionProgress;
        public string KingdomName;
        public string KingdomLore;
        public bool KingdomLocked;
        public List<int> SelectedSoldiersForBattle;

        #endregion
    }



    [System.Serializable]
    public class SaveSystemCastle
    {


        public int CastleID;
        public int KingdomID;
        public List<int> SoldiersList;
        public List<int> PowersList;
        public int CastleTier;


        #region Production
        public int ProductionCapacity;
        public float ProductionTime;
        #endregion
    }


    [System.Serializable]
    public class SaveSystemPowerup
    {


    }
}
