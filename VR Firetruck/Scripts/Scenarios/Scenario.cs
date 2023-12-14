using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

namespace _360Fabriek {
    public class Scenario : MonoBehaviour {
        [Header("Scenario:")]
        [SerializeField] private bool isTutorialScenario;
        [SerializeField] private new string name;
        [SerializeField] private AudioClip scenarioIntroductionVoiceline = null;
        [Space]
        [SerializeField] [ReadOnly] private int index = -1;
        [SerializeField] private List<Step> steps = new List<Step>();

        public string Name => name;
        public AudioClip Voiceline => scenarioIntroductionVoiceline;

        private IEnumerator Start() {
            if (steps.Count > 0) {
                foreach (Step step in steps) {
                    if (step.Action && !step.Action.name.ToLower().Contains("deprecated") && step.Action.gameObject.activeSelf) {

                        step.Action.Init();
                    } else {
                        Debug.LogError($"Missing action of ({step.Name}) in Scencario ({name})");
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public Step NextStep() { return AlterIndex(1); }
        public Step RevertStep() { return AlterIndex(-1, 1); }
        public Step PreviousStep() { return AlterIndex(-1); }
        public Step CurrentStep() { return AlterIndex(0); }

        private Step AlterIndex(int alteration, int returnOffset = 0) {
            index += alteration;

            if (index < 0 || index >= steps.Count) {
                index -= alteration;
            }

            return steps[index + returnOffset];
        }

        public bool IsActive() { return StatePercentageOf(State.Active) > 0 || index != -1; }
        public bool IsInitialised() { return StatePercentageOf(State.Uninitialised) == 0 && index == -1; }

        private int StatePercentageOf(State status) {
            int found = 0;
            int percentage = 0;

            steps.ForEach(step => found += (step.Action.Status == status) ? 1 : 0);

            int stepCount = steps.Count;

            if (stepCount > 0) {
                percentage = 100 / stepCount * found;
            }

            print("Scenario Status Check => " + percentage + "% of steps is " + status);

            return percentage;
        }

        public bool ExecutedAllSteps() { return IndexPercentage() >= 100; }

        public void SkipCurrentStep() {
            CurrentStep().Action.Skip();
        }

        private int IndexPercentage() {
            int percentage = (int)Math.Round((100.0 / steps.Count) * (index + 1));

            return percentage;
        }

        public void ResetIndex() {
            index = -1;
        }

        [Serializable]
        public class Step {
            [SerializeField] private string name = "";
            [SerializeField] private AbstractAction action = null;
            [SerializeField] private AudioClip voiceline = null;

            public string Name => name;
            public AbstractAction Action => action;
            public AudioClip Voiceline => voiceline;
        }
    }
}