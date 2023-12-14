using UnityEngine;
using UnityEngine.UI;
using Autohand;
using _360Fabriek.UI;
using _360Fabriek.Utility;
using System.Collections;

using Step = _360Fabriek.Scenario.Step;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios {
    public class Clipboard : MonoBehaviour {
        [Header("Clipboard Grabbing: ")]
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private bool constrain = true;

        [Header("Clipboard Components: ")]
        [SerializeField] private Vehicle vehicle;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform stepsHolder;
        [SerializeField] private RectTransform scenarioHolder;
        [SerializeField] private RectTransform navigationHolder;

        [Header("(Optional) Restart")]
        [Tooltip("This is only used in scenes marked as tutorial scenes")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Scenario restartScenario;

        [Header("Clipboard Placable Prefabs: ")]
        [SerializeField] private ClipboardButton clipboardButtonPrefab;
        [SerializeField] private ClipboardStep clipboardStepPrefab;
        [SerializeField] private ClipboardNavigation clipboardNavigationPrefab;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private ClipboardNavigation navigationInstance;
        private RigidbodyConstraints originalConstrains;
        private AudioClip currentVoiceline;

        private readonly KeyCode[] keyCodes = {
            KeyCode.Alpha0,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
        };

        public static Clipboard Instance { get; private set; }

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;

            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }

            originalConstrains = rigidbody.constraints;
            TrySetContraints(RigidbodyConstraints.FreezeAll);

            grabbable.OnGrabEvent += TrySetContraints;
            grabbable.OnReleaseEvent += ResetLocation;

            ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            ScenarioManager.Instance.OnNextStep.AddListener(AddStep);

            if (clipboardNavigationPrefab) {
                navigationInstance = Instantiate(clipboardNavigationPrefab, navigationHolder);

                if (scenarioHolder) {
                    ScenarioManager.Instance.OnScenarioFinish.AddListener((Scenario _) => { navigationInstance.openScenarioHolder.onClick?.Invoke(); });
                    navigationInstance.openScenarioHolder.onClick.AddListener(() => { SetActiveScrollContent(scenarioHolder); });
                }

                navigationInstance.replayVoiceline.onClick.AddListener(OnVoicelineButtonClicked);
                navigationInstance.openStepHolder.onClick.AddListener(() => { SetActiveScrollContent(stepsHolder); });
            }

            if (restartButton) {
                bool isTutorial = ScenarioManager.Instance.IsTutorial;

                restartButton.gameObject.SetActive(isTutorial);
                restartButton.enabled = isTutorial;

                if (isTutorial) {
                    restartButton.onClick.AddListener(() => StartScenario(restartScenario));
                }
            }

            if (vehicle && scenarioHolder) {
                SetActiveScrollContent(scenarioHolder);

                for (int i = 0; i < vehicle.Scenarios.Count; i++) {
                    Scenario scenario = vehicle.Scenarios[i];

                    ClipboardButton clipboardButton = Instantiate(clipboardButtonPrefab, scenarioHolder);

                    clipboardButton.SetScenarioName(scenario.Name);
                    clipboardButton.OnClickEvent.AddListener(OnClipboardButtonClicked);
                }
            }
        }

        private void StartScenario(Scenario scenario) {
            if (ScenarioManager.Instance && scenario) {
                ScenarioManager.Instance.SetScenario(scenario);
            }
        }

        private void OnClipboardButtonClicked(ClipboardButton arg) {
            vehicle.SetScenarioByName(arg.name);

            if (navigationInstance) {
                navigationInstance.ShiftHolder(false, true);
                navigationInstance.SetInteractability(navigationInstance.openStepHolder, true);
            }
        }

        private void OnVoicelineButtonClicked() {
            if (currentVoiceline && ScenarioManager.Instance && ScenarioManager.Instance.AudioListener) {
                ScenarioManager.Instance.AudioListener.SetAudio(currentVoiceline);
            }
        }

        private void SetActiveScrollContent(RectTransform holder) {
            StaticUtilities.TrySetActive(scrollRect.content.gameObject, false);

            holder.gameObject.SetActive(true);
            scrollRect.content = holder;
        }

        private void OnSetScenario(Scenario scenario) {
            DestroyChildren(stepsHolder);
            SetActiveScrollContent(stepsHolder);
        }

        public void AddStep(Step step) {
            if (step.Action.ShowInClipboard) {
                ClipboardStep stepObject = Instantiate(clipboardStepPrefab, stepsHolder);
                int index = stepsHolder.childCount;

                currentVoiceline = step.Voiceline;

                stepObject.Init(index.ToString(), step.Name);
                StartCoroutine(ResetScrollHeight());
            }
        }

        private void TrySetContraints(Hand hand, Grabbable grabbable) {
            TrySetContraints(originalConstrains);
        }

        private void TrySetContraints(RigidbodyConstraints constraints) {
            if (constrain) {
                StaticUtilities.TrySetRigidbodyConstraints(rigidbody, constraints);
            }
        }

        private void ResetLocation(Hand hand, Grabbable grabbable) {
            if (grabbable.IsHeld()) {
                return;
            }

            TrySetContraints(RigidbodyConstraints.FreezeAll);

            grabbable.body.velocity = new Vector3();
            grabbable.body.angularVelocity = new Vector3();

            transform.SetPositionAndRotation(startPosition, startRotation);
        }

        private void DestroyChildren(Transform parent) {
            foreach (Transform child in parent) {
                Destroy(child.gameObject);
            }
        }

        private IEnumerator ResetScrollHeight() {
            yield return new WaitForSeconds(.2f);
            scrollRect.verticalScrollbar.value = 0f;
        }

#if UNITY_EDITOR
        private void Update()
        {
            Scroll();
            CheckForEditorScenarioSelection();
        }

        private void Scroll()
        {
            float scroll = Input.GetAxisRaw("Vertical");

            if (scroll != 0)
            {
                scrollRect.verticalNormalizedPosition += scroll * scrollRect.scrollSensitivity * Time.deltaTime;
            }
        }

        private void CheckForEditorScenarioSelection()
        {
            if (vehicle)
            {
                int input = -1;

                for (int i = 0; i < keyCodes.Length; i++)
                {
                    if (Input.GetKeyDown(keyCodes[i]))
                    {
                        input = i;

                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            input += 10;
                        }
                        break;
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnVoicelineButtonClicked();
                }

                if (input >= 0)
                {
                    if (vehicle.TrySetScenarioByIndex(input))
                    {
                        if (navigationInstance)
                        {
                            navigationInstance.ShiftHolder(false, true);
                            navigationInstance.SetInteractability(navigationInstance.openStepHolder, true);
                        }

                        scenarioHolder.gameObject.SetActive(false);
                    }
                }
            }
        }
#endif
    }
}