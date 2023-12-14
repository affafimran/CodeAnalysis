using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;

namespace _360Fabriek.Scenarios {
    public class ClipboardButton : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI scenarioTitle;
        [SerializeField] private Button scenarioButton;
        [Space]
        [SerializeField] public UnityEvent<ClipboardButton> OnClickEvent = new UnityEvent<ClipboardButton>();
        [Space]
        [SerializeField] [ReadOnly] public new string name;

        private void Start() {
            scenarioButton.onClick.AddListener(OnButtonClick);
        }

        public void SetScenarioName(string name) {
            if (scenarioTitle) {
                scenarioTitle.text = name;
            }

            this.name = name;
        }

        private void OnButtonClick() {
            OnClickEvent?.Invoke(this);
        }
    }
}