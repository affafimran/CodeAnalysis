using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using NaughtyAttributes;
using Autohand;
using TMPro;
using Step = _360Fabriek.Scenario.Step;
using ActionArg = _360Fabriek.AbstractAction.ActionArg;

namespace _360Fabriek.Controllers {
    public class ScenarioManager : MonoBehaviour {
        [Header("Scenario Manager Editor Reference:")] 
        [SerializeField] private Audio.AudioListener audioListener = null;
        [SerializeField] private OVRHandControllerSwapper ovrController;
        [SerializeField, ReadOnly] public Scenario currentActiveScenario = null;

        [Header("Scenario Manager Settings:")]
        [Tooltip("First item in list will be default")]
        [SerializeField] private List<DifficultySetting> difficultyPresets = new List<DifficultySetting>();
        [SerializeField] private bool deactivateActionWhenFinished = true;
        [SerializeField] private bool isTutorial;

        [Space]
        public UnityEvent<Scenario> OnScenarioSet = new UnityEvent<Scenario>();
        public UnityEvent<Scenario> OnScenarioFinish = new UnityEvent<Scenario>();
        public UnityEvent<Step> OnNextStep = new UnityEvent<Step>();
        public UnityEvent OnOpenShutters = new UnityEvent();
        public UnityEvent<DifficultySetting> OnChangeSettings = new UnityEvent<DifficultySetting>();

        [SerializeField, ReadOnly] private DifficultySetting currentDifficulty;

        public static ScenarioManager Instance { get; private set; }
        public DifficultySetting CurrentDifficulty => currentDifficulty;
        public bool IsTutorial => isTutorial;
        public Audio.AudioListener AudioListener => audioListener;

        public TMP_Text valueTest;

        [field: SerializeField, ReadOnly] public bool ShuttersAreOpen { get; private set; }

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;

            if (difficultyPresets.Count == 0) {
                DifficultySetting setting = new DifficultySetting(Random.Range(2,60), true, true);
                difficultyPresets.Add(setting);
            }
            currentDifficulty = difficultyPresets[0];
        }

        private void Start() {
            if (!ovrController) {
                ovrController = FindObjectOfType<OVRHandControllerSwapper>();
            }

            if (!audioListener) {
                GameObject go = new GameObject("Voicelines");
                audioListener = go.AddComponent<Audio.AudioListener>();
            }
        }

#if UNITY_EDITOR
        private void Update() {
            if (Input.GetMouseButtonDown(0) && currentActiveScenario) {
                currentActiveScenario.SkipCurrentStep();
            }
        }
#endif

        public void OpenShutters() {
            if (!ShuttersAreOpen) {
                OnOpenShutters?.Invoke();
                ShuttersAreOpen = true;
            }
        }

        private bool IsHandHolding(Hand _) {
            return false;
        }

        public void TrySetScenario(Scenario scenario) {
            if (!currentActiveScenario) {
                SetScenario(scenario);
            }
        }

        public void SetScenario(Scenario scenario) {
            if (IsHandHolding(ovrController.leftHand) || IsHandHolding(ovrController.rightHand)) {
                return;
            }

            if (currentActiveScenario != null) {
                if (currentActiveScenario.IsActive()) {
                    Step step = currentActiveScenario.RevertStep();

                    step.Action.FinishEvent.RemoveListener(OnStepActionFinish);
                    step.Action.Deactivate();
                }

                currentActiveScenario.ResetIndex();
            }

            TryLoadScenario(scenario);
        }

        private void TryLoadScenario(Scenario scenario) {
            if (scenario) {
                //scenario.gameObject.SetActive(true); // Won't work as some steps are overlapped and once the scenario is activated it won't recognize the completed overlapping steps. 
                currentActiveScenario = scenario;

                StartCoroutine(LoadCoroutine());
            }
        }

        public void ChangeSetting(DifficultySetting setting) {
            currentDifficulty = setting;
            OnChangeSettings?.Invoke(setting);
        }

        private IEnumerator LoadCoroutine() {
            while (!currentActiveScenario.IsInitialised()) {
                yield return new WaitForSeconds(0.5f);
            }

            OnScenarioSet?.Invoke(currentActiveScenario);

            if (audioListener && currentActiveScenario.Voiceline) {
                audioListener.SetAudio(currentActiveScenario.Voiceline);

                yield return new WaitForSeconds(currentActiveScenario.Voiceline.length);
            }

            Progress();
        }

        private void Progress() {
            if (!currentActiveScenario.ExecutedAllSteps()) {
                Step step = currentActiveScenario.NextStep();

                if (audioListener && currentDifficulty.useHighlights) {
                    audioListener.SetAudio(step.Voiceline);
                }

                step.Action.FinishEvent.AddListener(OnStepActionFinish);
                step.Action.Activate();

                OnNextStep?.Invoke(step);
            } else {
                OnScenarioFinish?.Invoke(currentActiveScenario);
            }
        }

        private void OnStepActionFinish(ActionArg arg) {
            if (deactivateActionWhenFinished) {
                arg.TriggeredAction.Deactivate();
            }

            arg.TriggeredAction.FinishEvent.RemoveListener(OnStepActionFinish);

            Progress();
        }

        

        [System.Serializable]
        public struct DifficultySetting {
            public int errorPercentage;
            public bool useHighlights;
            public bool useVoicelines;

            public DifficultySetting(int errorPercentage , bool useHighlights = true, bool useVoicelines = true) {
                this.errorPercentage = errorPercentage;
                this.useHighlights = useHighlights;
                this.useVoicelines = useVoicelines;
            }
        }
    }
}