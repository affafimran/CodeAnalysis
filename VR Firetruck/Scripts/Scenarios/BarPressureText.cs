using _360Fabriek.Scenarios.Listeners;
using TMPro;
using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class BarPressureText : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private NeedleListener needle;

        private void Awake() {
            text.text = "0";
        }

        private void Update() {
            float value = needle.Value;
            text.text = float.NaN == value ? "0" : value.ToString();
        }
    }
}