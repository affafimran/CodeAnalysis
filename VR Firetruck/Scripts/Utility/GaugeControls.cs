using _360Fabriek.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _360Fabriek
{
    public class GaugeControls : MonoBehaviour
    {

        public static GaugeControls Instance { get; private set; }


        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (ScenarioManager.Instance)
            {
                ScenarioManager.Instance.OnScenarioSet.AddListener(ScenarioSet);
            }
        }

        void ScenarioSet(Scenario _)
        {
            SetHDGauge(0);
            SetLDGauge(0);
        }

        

        public void SetHDGauge(float value)
        {
            PlayerPrefs.SetFloat("Gauge1", value);
        }
        public void SetLDGauge(float value)
        {
            PlayerPrefs.SetFloat("Gauge2", value);
        }

        public float GetHDGauge()
        {
           return PlayerPrefs.GetFloat("Gauge1");
        }
        public float GetLDGauge()
        {
            return PlayerPrefs.GetFloat("Gauge2");
        }

    }

}