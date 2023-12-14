using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSpawner : MonoBehaviour
{
    public int soldierID;
    GameObject soldierPrefab = null;

    public void SpawnOnClick()
    {
        soldierPrefab = Castle.Instance.SoldierGameObjectToSpawn(soldierID);
        Soldier soldierRef = GameManager.GetSoldier(soldierID);
        Transform transfom = WaypointHandler.Instance.returnPosition();
        GameObject unit = Instantiate(soldierPrefab, transfom.position, transfom.rotation);
        unit.layer = LayerMask.NameToLayer("Player");
        unit.GetComponent<Soldier>().SoldierID = soldierRef.SoldierID;
        unit.GetComponent<Soldier>().CastleID = soldierRef.CastleID;
        unit.GetComponent<Soldier>().Health = soldierRef.Health;
        unit.GetComponent<Soldier>().XpPoints = soldierRef.XpPoints;
        unit.GetComponent<Soldier>().SoldierLevel = soldierRef.SoldierLevel;
        unit.GetComponent<Soldier>().SummonCost = soldierRef.SummonCost;
        unit.GetComponent<Soldier>().UpgradeCost = soldierRef.UpgradeCost;
        unit.GetComponent<Soldier>().SoldierType = soldierRef.SoldierType;
        unit.GetComponent<Soldier>().HitPoints = soldierRef.HitPoints;
        unit.GetComponent<Soldier>().FactionName = soldierRef.FactionName;
        unit.GetComponent<Soldier>().SoldierName = soldierRef.SoldierName;
        unit.GetComponent<Soldier>().SoldierRarity = soldierRef.SoldierRarity;
        unit.GetComponent<Soldier>().DamageTolerance = soldierRef.DamageTolerance;
    }
}