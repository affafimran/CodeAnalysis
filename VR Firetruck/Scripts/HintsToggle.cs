using _360Fabriek.Controllers;
using UnityEngine;
using UnityEngine.UI;
using DifficultySetting = _360Fabriek.Controllers.ScenarioManager.DifficultySetting;

namespace _360Fabriek.Scenarios {
    public class HintsToggle : MonoBehaviour {
        [SerializeField] private Toggle toggle;

        private bool value;

        private void Start() {
            toggle.onValueChanged.AddListener(OnValueChange);
        }

#if UNITY_EDITOR
        private void Update() {
            if (Input.GetKeyDown(KeyCode.A)) {
                Debug.LogError("key pressed");
                OnValueChange(!value);
            }
        }
#endif

        private void OnValueChange(bool on) {
            Debug.LogError("Value change initiated");
            value = on;
            DifficultySetting difficultySetting = new DifficultySetting(Random.Range(2,60), on, true);
            ScenarioManager.Instance.ChangeSetting(difficultySetting);
        }
    }
}