using _360Fabriek.Scenarios.Actions;
using NaughtyAttributes;
using UnityEngine;
using _360Fabriek.Utility;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios.Listeners {
    public class HaspelListener : MonoBehaviour {
        [SerializeField] private VRTouchAction action;
        [SerializeField, Range(-1f, 1f)] private float speed;
        [SerializeField, Range(0, 1f)] private float normalizedStartPosition;
        [SerializeField] private AnimationData[] animationDatas;

        [SerializeField] private bool resetOnScenarioSet;
        [ShowIf(nameof(resetOnScenarioSet))] 
        [SerializeField] private GameObject tube;

        private void Start() {
            action.OnTouchStart.AddListener(OnTouchStart);

            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }

            if (resetOnScenarioSet) {
                SetAnimationSpeed(0);
            }
        }

        private void OnTouchStart() {
            SetAnimationSpeed(speed);
        }

        private void OnSetScenario(Scenario _) {
            SetAnimationSpeed(0);
        }

        private void SetAnimationSpeed(float speed) {
            if (resetOnScenarioSet && tube) {
                tube.SetActive(speed != 0);
            }

            foreach (AnimationData data in animationDatas) {
                data.ApplyValues(speed, normalizedStartPosition);
            }
        }
    }
}