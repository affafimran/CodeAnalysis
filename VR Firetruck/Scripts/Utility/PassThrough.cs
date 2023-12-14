using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek
{
    public class PassThrough : MonoBehaviour
    {
        [SerializeField] private Button passThrough;
        [SerializeField] private Button VR;


        void Start()
        {

            if (!PlayerPrefs.HasKey("Mode"))
            {
                PlayerPrefs.SetString("Mode", "VR");
            }
            
            
            passThrough.onClick.AddListener(SetPassthroughState);
            VR.onClick.AddListener(SetVRState);
        }

        void SetVRState()
        {
            PlayerPrefs.SetString("Mode", "VR");
            PassThroughManager.Instance.SetVRObjects();
        }

        void SetPassthroughState()
        {
            PlayerPrefs.SetString("Mode", "PT");
            PassThroughManager.Instance.SetPassthroughObjects();
        }
    }

    
}