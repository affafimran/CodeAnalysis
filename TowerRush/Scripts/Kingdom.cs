using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingdom : MonoBehaviour
{
    public int KingdomID { get; set; }
    public List<int> CastleList { get; set; } = new List<int>();
    public float ConqueredRegionProgress { get; set; }
    public string KingdomName { get; set; }
    public string KingdomLore { get; set; }
    public bool KingdomLocked;
    public List<int> SelectedSoldiersForBattle { get; set; } = new List<int>();

    public delegate void KingdomUnlocked();
    public event KingdomUnlocked OnKingdomUnlocked;

    public delegate void SoldierSelectedForBattle(Kingdom Kingdom);
    public static event SoldierSelectedForBattle OnSoldierSelectedForBattle;


    public void AddCastleToKingdom(int _id)
    {
        CastleList.Add(_id);
        // Debug.Log("Kingdom.cs: Added castle to kingdom : " + _id);
    }

    private void Start()
    {
        LoadPlayerData.OnSavedDataLoaded += PopulateDefaultSoldierForBattle;
    }

    private void OnDisable()
    {
        LoadPlayerData.OnSavedDataLoaded -= PopulateDefaultSoldierForBattle;

    }

    public void UnlockKingdom()
    {
        KingdomLocked = false;
        OnKingdomUnlocked?.Invoke();
    }

    public void AddSoldierToBattleTeam(int soldierID)
    {
        if (SelectedSoldiersForBattle.Contains(soldierID) || SelectedSoldiersForBattle.Count >= 6)
        {
            Debug.LogFormat("Can't add a soldier to battle list. either it is alreayd available or the limit has been reached");
            return;
        }
        Debug.LogFormat("Soldier {0} Added To ToBattle", soldierID);
        SelectedSoldiersForBattle.Add(soldierID);
        OnSoldierSelectedForBattle?.Invoke(this);
    }

    public void RemoveSoldierFromBattleTeam(int soldierID)
    {
        for (int i = 0; i < SelectedSoldiersForBattle.Count; i++)
        {
            if (soldierID == SelectedSoldiersForBattle[i])
            {
                SelectedSoldiersForBattle.Remove(soldierID);
                Debug.LogFormat("Soldier {0} Removed To Battle", soldierID);

            }
        }
        OnSoldierSelectedForBattle?.Invoke(this);
    }


    public void PopulateDefaultSoldierForBattle()
    {
        List<Soldier> _soldiers = GameManager.GetAllSoldiersFromKingdom(this);

        List<int> SelectedSoldiersForBattle = new List<int>();
        Debug.LogFormat("PopulateDefaultSoldierForBattle(): ", _soldiers.Count);
        for (int i = 0; i < _soldiers.Count; i++)
        {
            if (SelectedSoldiersForBattle.Contains(_soldiers[i].SoldierID))
                return;
            if (!_soldiers[i].SoldierLocked)
                AddSoldierToBattleTeam(_soldiers[i].SoldierID);
        }
    }
}