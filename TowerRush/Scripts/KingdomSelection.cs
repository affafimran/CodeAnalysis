using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingdomSelection : MonoBehaviour
{
    public GameObject playerCastle, enemyCastle;
    public Castle playerCastleID, enemyCastleID;
    void Start()
    {
        List<Kingdom> kingdoms = GameManager.GetAllKingdoms();
        
        int playerKingdom = Random.Range(0, kingdoms.Count-1);
        int randNum = Random.Range(0, kingdoms[playerKingdom].CastleList.Count - 1);
        playerCastleID = GameManager.GetCastle(kingdoms[playerKingdom].CastleList[randNum]);
        
        
        int enemyKingdom = Random.Range(0, kingdoms.Count - 1);
        randNum = Random.Range(0, kingdoms[enemyKingdom].CastleList.Count - 1);
        enemyCastleID = GameManager.GetCastle(kingdoms[enemyKingdom].CastleList[randNum]);
        SetCastleProperties();


    }

    public void SetCastleProperties()
    {
        playerCastle.GetComponent<Castle>().CastleID= playerCastleID.CastleID;
        playerCastle.GetComponent<Castle>().CastleName = playerCastleID.CastleName;
        playerCastle.GetComponent<Castle>().ProductionCapacity= playerCastleID.ProductionCapacity;
        playerCastle.GetComponent<Castle>().ProductionTime= playerCastleID.ProductionTime;
        playerCastle.GetComponent<Castle>().DamageTolerance= playerCastleID.DamageTolerance;
        playerCastle.GetComponent<Castle>().SoldiersList= playerCastleID.SoldiersList;

        //Setting enemy castle properties
        enemyCastle.GetComponent<Castle>().CastleID = enemyCastleID.CastleID;
        enemyCastle.GetComponent<Castle>().CastleName = enemyCastleID.CastleName;
        enemyCastle.GetComponent<Castle>().ProductionCapacity = enemyCastleID.ProductionCapacity;
        enemyCastle.GetComponent<Castle>().ProductionTime = enemyCastleID.ProductionTime;
        enemyCastle.GetComponent<Castle>().DamageTolerance = enemyCastleID.DamageTolerance;
        enemyCastle.GetComponent<Castle>().SoldiersList = enemyCastleID.SoldiersList;
    }
}
