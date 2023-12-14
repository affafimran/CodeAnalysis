using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek
{
    public class PassThroughManager : MonoBehaviour
    {
        [SerializeField] private PassThroughObjects[] gameObjects;
        [SerializeField] private Material skybox;
        [SerializeField] private bool checkAtStart;
        [SerializeField] private OVRManager ovrManager;
        //public Text dummy;
        public static PassThroughManager Instance { get; private set; }
        private void Start()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            
            if (checkAtStart)
            {
                if (PlayerPrefs.GetString("Mode").Equals("VR"))
                {
                    SetVRObjects();
                }
                else if (PlayerPrefs.GetString("Mode").Equals("PT"))
                {
                    SetPassthroughObjects();
                }
            }
            
        }

        
        public void SetVRObjects()
        {
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            ovrManager.isInsightPassthroughEnabled = false;
            foreach (PassThroughObjects passThroughObject in gameObjects)
            {
                passThroughObject.SetVRMaterial();
            }
        }

        public void SetPassthroughObjects()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            ovrManager.isInsightPassthroughEnabled = true;
            foreach (PassThroughObjects passThroughObject in gameObjects)
            {
                passThroughObject.SetPassThroughMaterial();
            }
        }

    }


    [System.Serializable]
    public class PassThroughObjects
    {
        [SerializeField] private MeshRenderer[] gameObjects;
        [SerializeField] private Material[] objectMaterials;
        [SerializeField] private Material[] passThroughMaterial;

        public void SetPassThroughMaterial()
        {
            
            foreach (MeshRenderer gameObject in gameObjects)
            {
                gameObject.materials = passThroughMaterial;
            }
        }

        public void SetVRMaterial()
        {
            
            foreach (MeshRenderer gameObject in gameObjects)
            {
                gameObject.materials = objectMaterials;
            }
        }
    }
}