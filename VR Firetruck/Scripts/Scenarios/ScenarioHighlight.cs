using _360Fabriek.Controllers;
using UnityEngine;
using DifficultySetting = _360Fabriek.Controllers.ScenarioManager.DifficultySetting;

namespace _360Fabriek.Scenarios {
    public class ScenarioHighlight : MonoBehaviour {
        [SerializeField] private MonoBehaviour[] connectedBehaviours;
        [SerializeField] private Renderer[] connectedRenderers;

        private void OnEnable() {
            if (ScenarioManager.Instance) {
                SetVisibility(ScenarioManager.Instance.CurrentDifficulty);
            }
        }

        private void OnDisable() {
            SetVisibility(new DifficultySetting(0, false));
        }

        private void Start() {
            SetVisibility(ScenarioManager.Instance.CurrentDifficulty);
            ScenarioManager.Instance.OnChangeSettings.AddListener(SetVisibility);
        }

        private void SetVisibility(DifficultySetting setting) {
            foreach(Renderer renderer in connectedRenderers) {
                renderer.enabled = setting.useHighlights;
            }

            foreach(MonoBehaviour mono in connectedBehaviours) {
                mono.enabled = setting.useHighlights;
            }
        }
    }
}