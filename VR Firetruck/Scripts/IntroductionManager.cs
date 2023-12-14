using _360Fabriek.Controllers;
using _360Fabriek.Menu;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek.Introduction {
    public class IntroductionManager : MonoBehaviour {
        [SerializeField] private bool forceIntroduction;

        [Space]
        [SerializeField] private GameObject introductionMenu;

        [SerializeField] private TextMeshProUGUI stepText;
        [SerializeField] private Button skip;
        [SerializeField] private Button next;
        [SerializeField] private Button logout;

        [SerializeField] private Scenario scenario;
        [SerializeField] private Audio.AudioListener audioListener;
        [Space]
        [SerializeField] private List<IntroductionStep> introductions = new List<IntroductionStep>();

        public static IntroductionManager Instance { get; private set; }
        public bool IsIntroducing { get; private set; }

        private const string IntroductionKey = "HasCompletedIntroduction";
        private Queue<IntroductionStep> Introductions = new Queue<IntroductionStep>();

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            if (!PlayerPrefs.HasKey("LoggedIn"))
            {
                PlayerPrefs.SetInt("LoggedIn", 0);
            }
            
            Instance = this;
            IsIntroducing = true;
        }

        private void Start() {
            skip.onClick.AddListener(StopIntroduction);
            next.onClick.AddListener(ShowNextStep);
            logout.onClick.AddListener(LogoutUser);
            if(PlayerPrefs.GetInt("LoggedIn")==1)
            {
                
                logout.interactable = true;
            }
            
            if (PlayerPrefs.GetInt(IntroductionKey, 0) == 0 || forceIntroduction) {
                StartIntroduction();
                return;
            }

            IsIntroducing = false;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                StartIntroduction();
            }

            if (!IsIntroducing) {
                return;
            }

            if (Input.GetMouseButtonDown(0)) {
                ShowNextStep();
            }

            if (Input.GetMouseButtonDown(1)) {
                StopIntroduction();
            }
        }

        private void StartIntroduction() {
            IsIntroducing = true;
            PlayerPrefs.SetInt(IntroductionKey, 1);

            FillIntroductionQueue();
            EnableDisableIntroductionEffects();
        }
        private void LogoutUser()
        {
            PlayerPrefs.SetInt("LoggedIn", 0);
            logout.interactable = false;
            
        }
        private void ShowNextStep() {
            if (Introductions.Count > 0) {
                IntroductionStep step = Introductions.Dequeue();

                stepText.text = step.IntroductionText;

                if (step.TryStartTraining) {
                    ScenarioManager.Instance.TrySetScenario(scenario);
                }

                if (step.Voiceline) {
                    audioListener.SetAudio(step.Voiceline);
                }

                return;
            }

            StopIntroduction();
        }

        private void StopIntroduction() {
            IsIntroducing = false;
            EnableDisableIntroductionEffects();
        }

        private void FillIntroductionQueue() {
            Introductions = new Queue<IntroductionStep>();

            for (int i = 0; i < introductions.Count; i++) {
                Introductions.Enqueue(introductions[i]);
            }
        }

        private void EnableDisableIntroductionEffects() {
            introductionMenu.SetActive(IsIntroducing);
            if (PlayerPrefs.GetInt("LoggedIn") == 0)
            {
                if (AuthenticationManager.Instance)
                {
                    AuthenticationManager.Instance.SetActiveSelectionMenu(!IsIntroducing);
                }
            }
            else
            {
                if (VehicleSelector.Instance)
                {
                    VehicleSelector.Instance.SetActiveSelectionMenu(!IsIntroducing);
                }
            }

            if (IsIntroducing) {
                ShowNextStep();
            }
        }

        public void SetActiveSelectionMenu(bool on)
        {
            introductionMenu.gameObject.SetActive(on);
            if (PlayerPrefs.GetInt("LoggedIn") == 1)
            {
                logout.interactable = true;
            }
            else
            {
                logout.interactable = false;
            }
        }
        [System.Serializable]
        private class IntroductionStep {
            [SerializeField] private bool tryStartTraining;
            [SerializeField, TextArea(minLines: 3, maxLines: 50)] private string introductionText;
            [SerializeField] private AudioClip voiceline;

            public string IntroductionText => introductionText;
            public bool TryStartTraining => tryStartTraining;
            public AudioClip Voiceline => voiceline;
        }
    }
}