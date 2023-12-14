using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek
{
    public class RelocationController : MonoBehaviour
    {
        [SerializeField] private Transform resetTransform;
        [SerializeField] GameObject player;
        [SerializeField] Camera playerHead;
        [SerializeField] private OVRInput.Controller controller;
        [SerializeField] private OVRInput.Button teleportButton;
        [SerializeField] private GameObject relocationMenu;
        public static RelocationController Instance;
        

        bool opened = false;
        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }


        public void FixedUpdate()
        {
            if (OVRInput.Get(teleportButton, controller) )
            {
                
                relocationMenu.SetActive(true);
            }
            


           
        }

        [ContextMenu("Reset Position")]
        public void ResetPosition()
        {
            var rotationAngleY = playerHead.transform.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;
            player.transform.Rotate(0f, -rotationAngleY, 0f);

            var distanceDiff = resetTransform.position - playerHead.transform.position;

            player.transform.position += distanceDiff;

            
            relocationMenu.SetActive(false);
        }
    }
}