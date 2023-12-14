using _360Fabriek.Controllers;
using _360Fabriek.Utility;
using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class BarSegments : MonoBehaviour {
        [SerializeField] private GameObject[] segments;

        private void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }

            ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            SetBarTargetInt(0);
        }

        private void OnSetScenario(Scenario scenario) {
            SetBarTargetInt(0);
        }

        public void SetBarTargetFloat(float target) {
            int part = Mathf.RoundToInt(segments.Length / 100f * target - .5f);

            for (int i = 0; i < segments.Length; i++) {
                segments[i].SetActive(i <= part);
            }
        }

        public void SetBarTargetInt(int target) {
            for (int index = 0; index < segments.Length; index++) {
                bool isWithinTarget = index < target;
                StaticUtilities.TrySetActive(segments[index], isWithinTarget);
            }
        }
    }
}