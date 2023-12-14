using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static List<Kingdom> KingdomsList = new List<Kingdom>();
    static List<Soldier> SoldiersList = new List<Soldier>();
    static List<Castle> CastlesList = new List<Castle>();
   // static Dictionary<string, List<Castle>> CastlesInKingdomList = new Dictionary<string, List<Castle>>();
    //static Dictionary<string, List<Soldier>> SoldiersInCastleList = new Dictionary<string, List<Soldier>>();
   static Kingdom Selectedkingdom;
    static Kingdom PreviewedKingdom;
   static bool KingdomHasBeenSelected;

    List<PowerUp> PowerupsList = new List<PowerUp>();
    static float resourcesCount = 500;
    static float gemsCount = 500;
    public delegate void OnResourcesAddition();
    public static event OnResourcesAddition OnResourcesAdded;
    public delegate void OnKingdomSelection();
    public static event OnKingdomSelection OnKingdomSelected;

    public delegate void onUpgradeSoldier(Soldier soldier);
    public static event onUpgradeSoldier OnSoldierUpgraded;


    private void OnEnable()
    {
        //LoadGameData.OnLoadDataComplete += PlaceCastlesAndSoldiers;
        DontDestroyOnLoad(this);
    }

    private void OnDisable()
    {
        //LoadGameData.OnLoadDataComplete -= PlaceCastlesAndSoldiers;
    }

    public static Kingdom GetKingdom(int kingdom_id)
    {
        Kingdom _kingdom = KingdomsList.Find(x => x.KingdomID == kingdom_id);
        Debug.Log("Getkingdom(): " + _kingdom.KingdomID);
        return _kingdom;
    }


    public static Kingdom GetKingdomForSoldier(int soldier_ID)
    {

        Castle castle = GetCastleForSoldier(soldier_ID);

        Kingdom _kingdom = KingdomsList.Find(x => x.CastleList.Contains(castle.CastleID));
        return _kingdom;
    }

    public static Castle GetCastle(int castle_id)
    {
        Castle _castle = CastlesList.Find(x => x.CastleID == castle_id);
        return _castle;
    }

    public static Soldier GetSoldier(int soldier_id)
    {
        Soldier _soldier = SoldiersList.Find(x => x.SoldierID == soldier_id);
        return _soldier;
    }


    public static void AddResources(int amountToAdd)
    {
        resourcesCount += amountToAdd;
        OnResourcesAdded?.Invoke();
    }

    bool CanPlaceSoldier(int _resourceCost)
    {
        return _resourceCost < resourcesCount;
    }

    public static bool CanUpgradeSoldier(float _resourceCost)
    {
        return _resourceCost < resourcesCount;
    }

    public static bool CanUnlockSoldier(float _gemsCount)
    {
        return _gemsCount < resourcesCount;
    }

    public static void AddKingdom(Kingdom _kingdom)
    {
        KingdomsList.Add(_kingdom);
        //Debug.Log("Added a kingdom: to Gamemanager Pool" + _kingdom.KingdomID);
    }

    public static void AddCastle(Castle _castle)
    {
        //Debug.Log("Added a castle: to Gamemanager Pool: " + _castle.CastleName);
        CastlesList.Add(_castle);
    }


    public static void AddSoldier(Soldier _soldier)
    {
        SoldiersList.Add(_soldier);
        //Debug.Log("Added a Soldier: to Gamemanager Pool: "+_soldier.SoldierID);
    }

    public static float GetResourcesCount()
    {
        return resourcesCount;
    }

    public static float GetGemsCount()
    {
        return gemsCount;
    }


    public static void SelectPlayerKingdom(int _kingdomID, bool selectedFromPreview)
    {

        Selectedkingdom = GetKingdom(_kingdomID);
        Debug.Log("Selected player Kingdom: " + Selectedkingdom.KingdomName + " Selected kingdom: " + Selectedkingdom.KingdomID);
        KingdomHasBeenSelected = true;
        if (!selectedFromPreview)
            OnKingdomSelected?.Invoke();
    }

    public static bool HasKingdomBeenSelected() {
        Debug.Log("HasKingdomBeenSelected: " + KingdomHasBeenSelected);
        return KingdomHasBeenSelected;
    }

    public static Kingdom GetPlayerKingdom()
    {
        if (Selectedkingdom == null)
        {
            Debug.Log("GetPlayerKingdom(): Selected Kingdom is null "+Selectedkingdom.KingdomID);
        }
        else
        {
            Debug.Log("GetPlayerKingdom(): Selected Kingdom is Available" + Selectedkingdom.KingdomID);
        }
        return Selectedkingdom;
    }

    public static List<Kingdom> GetAllKingdoms()
    {
        return KingdomsList;
    }
    public static List<Soldier> GetAllSoldiers()
    {
        return SoldiersList;
    }


    public static List<Soldier> GetAllSoldiersFromKingdom(Kingdom kingdom)
    {
        List<Soldier> _soldiers = new List<Soldier>();
        Kingdom kd = kingdom;

        for (int i = 0; i < kd.CastleList.Count; i++)
        {
            Castle castle = GetCastle(kd.CastleList[i]);
            List<Soldier> _castleSoldiers = castle.GetCastleSoldiers();

            for (int j = 0; j < _castleSoldiers.Count; j++)
            {
                _soldiers.Add(_castleSoldiers[j]);
            }
        } 
        return _soldiers;
    }
    public static List<Castle> GetAllCastles()
    {
        return CastlesList;
    }

    string GetCastleNameForSoldier(int _id)
    {

        //        Debug.Log("GetCastleNameForSoldier(): Soldier Count " + SoldiersList.Count + " ID: " + _id + "Castle List:" + CastlesList.Count);
        Castle castle = CastlesList.Find(x => x.SoldiersList.Contains(_id));

        if (castle == null)
            Debug.Log("Can't find castle");
        string _castleName = castle.CastleName;
        return _castleName;
    }

    static Castle GetCastleForSoldier(int _id)
    {

        //        Debug.Log("GetCastleNameForSoldier(): Soldier Count " + SoldiersList.Count + " ID: " + _id + "Castle List:" + CastlesList.Count);
        Castle castle = CastlesList.Find(x => x.SoldiersList.Contains(_id));

        if (castle == null)
            Debug.Log("Can't find castle");

        return castle;
    }


    string GetKingdomNameForCastle(int _id)
    {
        //  Debug.Log("GetKingdomNameForCastle(): Castle Count " + CastlesList.Count + " ID: " + _id);

        string _kingdomName = KingdomsList.Find(x => x.CastleList.Contains(_id)).KingdomName;
        return _kingdomName;
    }

    public static Kingdom GetPreviewedKingdom()
    {
        return PreviewedKingdom;
    }

    public static void SetPreviewedKingdom(int kingdomid) {
        PreviewedKingdom = GetKingdom(kingdomid);
    }


    //public void PlaceSoldierInCastleDictionary(int _soldierID)
    //{
    //    Soldier _soldier = GetSoldier(_soldierID);
    //    List<Soldier> _soldiersList;
    //    string _castleName = GetCastleNameForSoldier(_soldierID);
    //    //Debug.Log("Trying to put a soldier in castle: " + _castleName);

    //    if (SoldiersInCastleList.ContainsKey(_castleName.ToLower()))
    //    {
    //        _soldiersList = new List<Soldier>();
    //        List<Soldier> _s = SoldiersInCastleList[_castleName.ToLower()];

    //        foreach (Soldier soldier in _s)
    //        {
    //            _soldiersList.Add(soldier);
    //        }

    //    }
    //    else
    //    {
    //        _soldiersList = new List<Soldier>();
    //    }
    //    _soldiersList.Add(_soldier);
    //    SoldiersInCastleList[_castleName.ToLower()] = _soldiersList;

    //    Debug.LogFormat("PlaceSoldierInCastleDictionary(): {0} : Soldiers Count:{1}", _castleName, _soldiersList.Count);
    //}


    //public void PlaceCastleInKingdomDictionary(int _castleID)
    //{
    //    Castle _castle = GetCastle(_castleID);
    //    List<Castle> _castlesList;
    //    string _kingdomName = GetKingdomNameForCastle(_castleID);
    //    if (CastlesInKingdomList.ContainsKey(_kingdomName.ToLower()))
    //    {
    //        _castlesList = new List<Castle>();
    //        List<Castle> _c = CastlesInKingdomList[_kingdomName.ToLower()];

    //        foreach (Castle castle in _c)
    //        {
    //            _castlesList.Add(castle);
    //        }
    //    }
    //    else
    //    {
    //        _castlesList = new List<Castle>();
    //    }

    //    _castlesList.Add(_castle);
    //    CastlesInKingdomList[_kingdomName.ToLower()] = _castlesList;
    //    Debug.LogFormat("PlaceCastleInKingdomDictionary(): Placed Castle:{0}: in Kingdom: {1} ", _castle.CastleName, _kingdomName);
    //}

    //void PlaceCastlesAndSoldiers()
    //{
    //    StartCoroutine(PlaceCastlesAndSoldierCoroutine());
    //}


    //IEnumerator PlaceCastlesAndSoldierCoroutine()
    //{
    //    for (int i = 0; i < SoldiersList.Count; i++)
    //    {
    //        Debug.Log("PlaceCastlesAndSoldiers() - Soldier: " + SoldiersList[i].SoldierID);
    //        PlaceSoldierInCastleDictionary(SoldiersList[i].SoldierID);
    //    }
    //    yield return new WaitForSeconds(0.8f);
    //    for (int i = 0; i < CastlesList.Count; i++)
    //    {
    //        PlaceCastleInKingdomDictionary(CastlesList[i].CastleID);
    //    }
    //}


    //public Dictionary<string, List<Castle>> GetAllCastlesInAllKingdom()
    //{
    //    return CastlesInKingdomList;
    //}

    //public Dictionary<string, List<Soldier>> GetAllSoldiersInAllCastles()
    //{
    //    return SoldiersInCastleList;
    //}


    public static void UpgradeSoldier(Soldier soldier) {

        if (CanUpgradeSoldier(soldier.UpgradeCost)) {
            resourcesCount -= soldier.UpgradeCost;
            soldier.Upgrade();
            OnSoldierUpgraded?.Invoke(soldier);
        }
    }
    public static void UnlockSoldier(Soldier soldier)
    {

        if (CanUnlockSoldier(20))
        {
            gemsCount -= 20;
            soldier.Unlock();
            OnSoldierUpgraded?.Invoke(soldier);
        }
    }

    public static void SetResourceCount(float count)
    {
        resourcesCount = count;
    }
    public static void SetGemsCount(float count)
    {
        gemsCount = count;
    }
}