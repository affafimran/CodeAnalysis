using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MessagePopup : MonoBehaviour
{

    public TextMeshProUGUI messageText;
    public GameObject closeBtn;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    internal void DisplayMessage(string message)
    {
        messageText.text = message;
    }


    internal void ClearMessage()
    {
        messageText.text = "";
    }

    public void closeBtnOnClick()
    {
        ViewManager.Instance.CloseMessage();
    }
}
