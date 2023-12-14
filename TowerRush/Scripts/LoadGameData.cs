using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LoadGameData : MonoBehaviour
{

    public delegate void LoadDataComplete();
    public static event LoadDataComplete OnLoadDataComplete;

    string GameDataText;

    public string GameDataFile = "gamedata";
    private void Start()
    {
        LoadGameDataFromXML();
    }

    public void LoadGameDataFromXML()
    {
        GameDataText = System.IO.File.ReadAllText("Assets/Resources/GameData/" + GameDataFile + ".xml");

        // GameData = Resources.Load("GameData/" + GameDataFile + ".xml") as TextAsset;

        XmlDocument xmlDoc = new XmlDocument();
        Debug.Log(GameDataText);
        xmlDoc.LoadXml(GameDataText);

        Debug.Log(xmlDoc);

        LoadKingdoms(xmlDoc);
        LoadCastles(xmlDoc);
        LoadSoldiers(xmlDoc);


        OnLoadDataComplete?.Invoke();
    }


    void LoadSoldiers(XmlDocument xmlDoc)
    {
        // Look through our XML nodes to get a list of all the soldiers
        XmlNodeList SoldiersList = xmlDoc.GetElementsByTagName("Soldier");

        // loop through all the soldier nodes
        foreach (XmlNode SoldierInfo in SoldiersList)
        {
            LoadSoldierNodes(SoldierInfo);
        }
    }

    void LoadKingdoms(XmlDocument xmlDoc)
    {
        // Look through our XML nodes to get a list of all the kingdoms
        XmlNodeList KingdomsList = xmlDoc.GetElementsByTagName("Kingdom");

        // loop through all the kingdom nodes
        foreach (XmlNode KingdomInfo in KingdomsList)
        {

            LoadKingdomNodes(KingdomInfo);
        }
    }

    void LoadCastles(XmlDocument xmlDoc)
    {

        XmlNodeList CastlesList = xmlDoc.GetElementsByTagName("Castle");


        foreach (XmlNode CastleInfo in CastlesList)
        {
            //Load Castle Nodes for each Castle
            LoadCastleNodes(CastleInfo);
        }
    }


    void LoadKingdomNodes(XmlNode KingdomInfo)
    {
        //Debug.Log("LoadKingdomNodes()");
        XmlNodeList KingdomNodes = KingdomInfo.ChildNodes;

        //GameObject NewKingdom = LoadKingdomPrefab(KingdomNodes);

        Kingdom kingdomOBJ = new Kingdom();

        foreach (XmlNode KingdomNode in KingdomNodes)
        {
            SetKingdomObj(kingdomOBJ, KingdomNode);
        }


        GameManager.AddKingdom(kingdomOBJ);
    }


    private void SetKingdomObj(Kingdom kingdomOBJ, XmlNode kingdomNode)
    {
        //Debug.Log("SetKingdomObj()");
        // This will match the XML node 'id' for the soldier
        if (kingdomNode.Name == "id")
            kingdomOBJ.KingdomID = int.Parse(kingdomNode.InnerText);

        if (kingdomNode.Name == "kingdomName")
            kingdomOBJ.KingdomName = kingdomNode.InnerText;

        if (kingdomNode.Name == "KingdomLore")
            kingdomOBJ.KingdomLore = kingdomNode.InnerText;

        if (kingdomNode.Name == "KingdomLocked")
            kingdomOBJ.KingdomLocked = Convert.ToBoolean(kingdomNode.InnerText.ToLower());

        if (kingdomNode.Name == "CastleList")
            LoadKingdomCastles(kingdomNode, kingdomOBJ);
    }

    private void LoadKingdomCastles(XmlNode kingdomNode, Kingdom kingdomOBJ)
    {
        XmlNodeList _castleIDs = kingdomNode.ChildNodes;

        foreach (XmlNode _castleNode in _castleIDs)
        {
            if (_castleNode.Name == "id")
            {
                int _castleID = int.Parse(_castleNode.InnerText);
                kingdomOBJ.AddCastleToKingdom(_castleID);

            }
        }
    }

    private void LoadCastleNodes(XmlNode CastleInfo)
    {
      //  Debug.Log("LoadKingdomNodes()");
        XmlNodeList CastlesNodes = CastleInfo.ChildNodes;

        //GameObject NewCastle = LoadCastlePrefab(CastleInfo);

        Castle castleObj = new Castle();

        foreach (XmlNode CastleNode in CastlesNodes)
        {
            SetCastleObj(castleObj, CastleNode);
        }

        GameManager.AddCastle(castleObj);
    }


    private void SetCastleObj(Castle castleObj, XmlNode castleNode)
    {
     //   Debug.Log("SetCastleDetails()");
        if (castleNode.Name == "id")
            castleObj.CastleID = int.Parse(castleNode.InnerText);

        if (castleNode.Name == "ProductionCapacity")
            castleObj.ProductionCapacity = int.Parse(castleNode.InnerText);

        if (castleNode.Name == "ProductionTime")
            castleObj.ProductionTime = int.Parse(castleNode.InnerText);

        if (castleNode.Name == "CastleName")
            castleObj.CastleName = castleNode.InnerText;


        if (castleNode.Name == "SoldierList")
            LoadCastleSoldiers(castleNode, castleObj);
        
        if (castleNode.Name == "DamageTolerance")
            castleObj.DamageTolerance = int.Parse(castleNode.InnerText);

    }

    private void LoadCastleSoldiers(XmlNode castleNode, Castle castleObj)
    {


        XmlNodeList SoldierNodes = castleNode.ChildNodes;
        castleObj.SoldiersList = new List<int>();
        foreach (XmlNode SoldierNode in SoldierNodes)
        {
            if (SoldierNode.Name == "ID")
            {
                //Debug.Log("LoadCastleSoldiers(): " + castleObj.CastleName);
                int _soldierID = int.Parse(SoldierNode.InnerText);
                castleObj.AddSoldiertoCastle(_soldierID);

            }
        }

    }

    void LoadSoldierNodes(XmlNode SoldierInfo)
    {

        XmlNodeList SoldierNodes = SoldierInfo.ChildNodes;

        //GameObject NewSoldier = LoadSoldierPrefab(SoldierNodes);

        Soldier soldierOBJ = new Soldier();

        foreach (XmlNode SoldierNode in SoldierNodes)
        {
            SetSoldierObj(soldierOBJ, SoldierNode);
        }


        GameManager.AddSoldier(soldierOBJ);
    }

    void SetSoldierObj(Soldier soldierObj, XmlNode SoldierNode)
    {
        // This will match the XML node 'id' for the soldier
        if (SoldierNode.Name == "id")
            soldierObj.SoldierID = int.Parse(SoldierNode.InnerText);

        if (SoldierNode.Name == "SoldierName")
            soldierObj.SoldierName = SoldierNode.InnerText;

        if (SoldierNode.Name == "SummonCost")
            soldierObj.SummonCost = int.Parse(SoldierNode.InnerText);

        if (SoldierNode.Name == "SoldierType")
            soldierObj.SoldierType = SoldierNode.InnerText;

        if (SoldierNode.Name == "Rarity")
            soldierObj.SoldierRarity = (SoldierRarity)Enum.Parse(typeof(SoldierRarity), SoldierNode.InnerText);

        if (SoldierNode.Name == "FactionName")
            soldierObj.FactionName = (SoldierNode.InnerText);

        if (SoldierNode.Name == "UpgradeCost")
            soldierObj.UpgradeCost = int.Parse(SoldierNode.InnerText);

        if (SoldierNode.Name == "hitPoints")
            soldierObj.HitPoints = int.Parse(SoldierNode.InnerText);

        if (SoldierNode.Name == "SoldierLevel")
            soldierObj.SoldierLevel = int.Parse(SoldierNode.InnerText);

        if (SoldierNode.Name == "DamageTolerance")
            soldierObj.DamageTolerance = int.Parse(SoldierNode.InnerText);
        if (SoldierNode.Name == "SoldierLocked")
            soldierObj.SoldierLocked = Convert.ToBoolean(SoldierNode.InnerText);


    }
}