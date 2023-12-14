using _360Fabriek.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _360Fabriek.Scenarios {
    public class Vehicle : MonoBehaviour {
        [SerializeField] private UnityEvent OnFinishInitialization = new UnityEvent();
        [Header("Scenarios")]
        [SerializeField] private List<Scenario> scenarios;

        public List<Scenario> Scenarios => scenarios;

        private IEnumerator Start() {
            bool isInitialized = false;

            while (!isInitialized) {
                isInitialized = true;

                for (int i = 0; i < scenarios.Count; i++) {
                    Scenario scenario = scenarios[i];

                    if (!scenario) {
                        Debug.LogError($"No scenario assigned in Vehicle: ({name}) at index [{i}]");
                        yield break;
                    }

                    if (!scenario.IsInitialised()) {
                        isInitialized = false;
                        break;
                    }
                }

                yield return new WaitForEndOfFrame();
            }

            OnFinishInitialization?.Invoke();
            print("All steps are initilized for " + name);
        }

        public Scenario GetScenarioByName(string name) {
            foreach (Scenario scenario in this.scenarios) {
                if (scenario.Name.Equals(name)) {
                    return scenario;
                }
            }

            return null;
        }

        public void SetScenarioByName(string name) {
            foreach (Scenario scenario in this.scenarios) {
                if (scenario.Name.Equals(name)) {
                    ScenarioManager.Instance.SetScenario(scenario);
                }
            }
        }

        public bool TrySetScenarioByIndex(int index) {
            if (index < scenarios.Count) {
                ScenarioManager.Instance.SetScenario(scenarios[index]);
                return true;
            }
            return false;
        }
    }
}