using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{

    public static Sprite GetSoldierSprite(string soldierName, bool isMiniTile)
    {
        Sprite _soldierSprite;

        string path = isMiniTile ? "Sprites/Tiles/SoldierIconsSmall/" + soldierName : "Sprites/Tiles/SoldierIcons/" + soldierName;
        _soldierSprite = Resources.Load<Sprite>(path);

        if (_soldierSprite == null)
            Debug.Log("Sprite not found at: " + path);
        return _soldierSprite;
    }



    public static Sprite GetKingdomSprite(string KingdomName, bool isMiniTile)
    {

        Sprite _kingdomSprite;
        string path;

        path = isMiniTile == true ? "Sprites/Tiles/KingdomIconsSmall/" + KingdomName : "Sprites/Tiles/KingdomIcons/" + KingdomName;
        _kingdomSprite = Resources.Load<Sprite>(path);

        if (_kingdomSprite == null)
            Debug.Log("Sprite not found");
        return _kingdomSprite;
    }
}
