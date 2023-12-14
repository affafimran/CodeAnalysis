using _360Fabriek.Controllers;
using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class Haspel : MonoBehaviour {
        [SerializeField] private GameObject defaultStep;
        [SerializeField] private GameObject[] steps;

        private void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }

            ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            EnableStep(defaultStep);
        }

        private void OnSetScenario(Scenario _) {
            EnableStep(defaultStep);
        }

        public void EnableStep(GameObject step) {
            foreach(GameObject go in steps) {
                go.SetActive(false);
            }

            step.SetActive(true);
        }
    }
}