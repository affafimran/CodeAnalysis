using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public enum CastleType {
Player,
Enemy
}


public class Castle : MonoBehaviour
{
    public static Castle Instance;
    public int CastleID { get; set; }
    public int KingdomID { get; set; }
    public float CastleHealth { get; set; }
    public float Maxhealth { get; set; }
    public List<int> SoldiersList { get; set; } = new List<int>(); // Enemy castle will select a list from gamemanagers soldiers list
    public List<int> PowersList { get; set; } = new List<int>();
    public int CastleTier { get; set; }
    public int DamageTolerance { get; set; }
    public string CastleName { get; set; }

    #region Production

    public int ProductionCapacity { get; set; }
    public float ProductionTime { get; set; }
    public float ProductionProgress { get; set; }
    public float ProductionProgressTime { get; set; }

    #endregion
    #region Events

    public delegate void OnDamageReceived();
    public static event OnDamageReceived OnReceiveDamage;

    #endregion


    public Kingdom GetKingdom(int _kingdom_id)
    {
        return GameManager.GetKingdom(_kingdom_id);
    }
    void Awake()
    {
        Instance = this;
        CastleHealth = 100;
    }
    void Start()
    {
        InvokeRepeating("ProduceResources", ProductionTime, ProductionTime);
    }

    private void Update()
    {
        ProductionProgressTime += Time.deltaTime;
        ProductionProgress = ProductionProgressTime / ProductionTime;
    }

    void ProduceResources()
    {
        GameManager.AddResources(ProductionCapacity);
        ProductionProgress = 0;
    }

    public void SummonPowerUp()
    {

    }

    public void ApplyDamage(float _damage)
    {
        float damage = _damage - (_damage * (DamageTolerance / 100));
        CastleHealth = CastleHealth - damage;
        OnReceiveDamage?.Invoke(); //Call Damage applied event

    }


    public void placeSoldier(Soldier _soldier)
    {

        //TODO: spawn a soldier and put it to fight

    }

    public void AddSoldiertoCastle(int _id)
    {

        SoldiersList.Add(_id);
     //   Debug.Log("Added Soldier to Castle: " + _id + " : SoldiersList Count is : " + SoldiersList.Count);

    }

    public GameObject SoldierGameObjectToSpawn(int _soldierID)
    {
        return SetSoldierProperty(_soldierID);
    }

    public GameObject SetSoldierProperty(int _soldierID)
    {
        Soldier soldierRef = GameManager.GetSoldier(_soldierID);
        GameObject NewSoldier =  Resources.Load<GameObject>("Prefabs/Soldiers/" + soldierRef.SoldierName );
        return NewSoldier;
    }

    public List<Soldier> GetCastleSoldiers()
    {
        List<Soldier> _soldiers = new List<Soldier>();

        for (int i = 0; i < SoldiersList.Count; i++)
        {
            _soldiers.Add(GameManager.GetSoldier(SoldiersList[i]));
        }

        return _soldiers;

    }
}
