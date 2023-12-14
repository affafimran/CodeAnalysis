using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SoldierRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public class Soldier : MonoBehaviour
{
    int soldierID;
    int castleID;
    float health;
    int xpPoints;
    int soldierLevel;
    int summonCost;
    float upgradeCost;
    string soldierType;
    float hitPoints;
    string factionName;
    string soldierName;
    float damageTolerance;
    bool _soldierLocked;

    SoldierRarity soldierRarity;
    public int CastleID { get => castleID; set => castleID = value; }
    public float Health { get => health; set => health = value; }
    public int XpPoints { get => xpPoints; set => xpPoints = value; }
    public int SoldierLevel { get => soldierLevel; set => soldierLevel = value; }
    public int SummonCost { get => summonCost; set => summonCost = value; }
    public float UpgradeCost { get => upgradeCost; set => upgradeCost = value; }
    public string SoldierType { get => soldierType; set => soldierType = value; }
    public float HitPoints { get => hitPoints; set => hitPoints = value; }
    public string FactionName { get => factionName; set => factionName = value; }
    public string SoldierName { get => soldierName; set => soldierName = value; }
    public int SoldierID { get => soldierID; set => soldierID = value; }

    public float DamageTolerance { get => damageTolerance; set => damageTolerance = value; }
    public SoldierRarity SoldierRarity { get => soldierRarity; set => soldierRarity = value; }
    public bool SoldierLocked { get => _soldierLocked; set => _soldierLocked = value; }

    private void Awake()
    {
        Health = 100;
    }


    public void ApplyDamage(float HP)
    {
        Health = Health - (HP - (HP * (DamageTolerance / 100)));
        print("Soldier Health" + Health);
    }


    public void Upgrade()
    {
        SoldierLevel += 1;
        UpgradeCost *= 1.17f;
        HitPoints *= 1.1f;
        damageTolerance *= 1.1f;
    }
    public void Unlock()
    {
        SoldierLocked = false;
    }
}
