using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingdom_View : MonoBehaviour
{
    public Kingdom kingdom;
    public GameObject LockedImage;



    private void OnEnable()
    {
        kingdom.OnKingdomUnlocked += UpdateUI;
    }


    private void OnDisable()
    {
        kingdom.OnKingdomUnlocked += UpdateUI;
    }




    void UpdateUI() {
        LockedImage.SetActive(kingdom.KingdomLocked);
    }
}
