using _360Fabriek.Controllers;
using _360Fabriek.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek.Scenarios {
    public class ClipboardNavigation : MonoBehaviour {
        [SerializeField] private RectTransform ScenarioHolderNavigation;
        [SerializeField] private RectTransform StepHolderNavigation;
        [Space]
        [SerializeField] public Button replayVoiceline;
        [SerializeField] public Button openStepHolder;
        [SerializeField] public Button openScenarioHolder;
        [SerializeField] public Button openRemise;

        private void Start() {
            if (openRemise) {
                openRemise.onClick.AddListener(ToRemise);
            }

            openStepHolder.onClick.AddListener(() => { ShiftHolder(false, true); });
            openScenarioHolder.onClick.AddListener(() => { ShiftHolder(true, false); });

            ShiftHolder(true, false);
            SetInteractability(openStepHolder, false);
        }

        public void ShiftHolder(bool hasScenario, bool hasStep) {
            StaticUtilities.TrySetActive(ScenarioHolderNavigation.gameObject, hasScenario);
            StaticUtilities.TrySetActive(StepHolderNavigation.gameObject, hasStep);
            
        }

        public void ToRemise() {
            ApplicationController.Instance.LoadToScene(SceneLoadDirection.ToMenu);
        }

        public void SetInteractability(Button button, bool isInteractable) {
            button.interactable = isInteractable;
        }
    }
}
