using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Castle_View : MonoBehaviour
{



    public Image CastleBackground;
    public Image HealthBar;
    public Castle Castle;
    public Image ProductionProgressBar;

    void OnEnable()
    {
        Castle.OnReceiveDamage += CustomUpdateUI;
    }

    void OnDisable()
    {
        Castle.OnReceiveDamage -= CustomUpdateUI;
    }

    void Awake()
    {
        Castle = GetComponent<Castle>();
    }

    void Start()
    {
        LoadCastleSprite();
    }

    void LoadCastleSprite()
    {
        Sprite _sprite = Resources.Load<Sprite>(string.Format("Sprites/Backgrounds/Kingdom/{0}/Castle_{1}", Castle.KingdomID, Castle.CastleID));

        if (!_sprite)
        {
            Debug.Log("Error loading castle sprite.. probably due to wrong folder structure");
            return;
        }
        CastleBackground.sprite = _sprite;
    }


    private void Update()
    {
        ProductionProgressBar.fillAmount = Castle.ProductionProgress / 100;

    }

    void CustomUpdateUI()
    {
        HealthBar.fillAmount = Castle.CastleHealth / Castle.Maxhealth;
    }
}