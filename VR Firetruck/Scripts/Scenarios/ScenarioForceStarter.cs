using UnityEngine;
using System.Collections;
using _360Fabriek.Controllers;

namespace _360Fabriek {
    public class ScenarioForceStarter : MonoBehaviour {
        [SerializeField] private Scenario scenario;

        private IEnumerator Start() {
            if (ScenarioManager.Instance && scenario) {
                while (!scenario.IsInitialised()) {
                    yield return null;
                }

                ScenarioManager.Instance.SetScenario(this.scenario);
            }
        }
    }
}