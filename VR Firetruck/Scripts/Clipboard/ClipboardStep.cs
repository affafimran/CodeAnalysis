using TMPro;
using UnityEngine;

namespace _360Fabriek.UI {
    public class ClipboardStep : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI index;
        [SerializeField] private TextMeshProUGUI stepName;

        public void Init(string index, string stepName) {
            this.index.text = index;
            this.stepName.text = stepName;
        }
    }
}