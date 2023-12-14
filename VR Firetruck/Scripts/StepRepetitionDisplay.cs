using TMPro;
using UnityEngine;
using _360Fabriek.Utility;
using ActionArg = _360Fabriek.AbstractAction.ActionArg;
using RepetitionArg = _360Fabriek.AbstractAction.RepetitionArg;

namespace _360Fabriek.Scenarios.UI {
    public class StepRepetitionDisplay : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private AbstractAction connectedAction;

        private void Start() {
            connectedAction.OnStepRepetitionEvent.AddListener((o) => UpdateUI(o, true));
            connectedAction.ActivateEvent.AddListener(OnActivate);
            connectedAction.DeactivateEvent.AddListener(OnDeactivate);
            UpdateUI(null, false);
        }

        private void OnActivate(ActionArg _) {
            UpdateUI(new RepetitionArg(connectedAction.RepetitionsNeeded + 1), true);
        }

        private void OnDeactivate(ActionArg _) {
            UpdateUI(null, false);
        }

        private void UpdateUI(RepetitionArg arg, bool on) {
            StaticUtilities.TrySetActive(text.gameObject, on);

            if (on) {
                text.text = (arg.RepetitionsLeft).ToString();
            }
        }
    }
}