using _360Fabriek.Controllers;
using _360Fabriek.Scenarios.Listeners;
using Autohand;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _360Fabriek.Scenarios
{
    public class ClipboardTouchNavigation : MonoBehaviour
    {
        [SerializeField] private RectTransform ScenarioHolderNavigation;
        [SerializeField] private RectTransform StepHolderNavigation;
        [Space]
        [SerializeField] public Button replayVoiceline;
        [SerializeField] public Button openStepHolder;
        [SerializeField] public Button openScenarioHolder;
        [SerializeField] public Button openRemise;


        [NaughtyAttributes.HideIf("startUnpress")]
        [SerializeField] private bool startPress = false;
        [NaughtyAttributes.HideIf("startPress")]
        [SerializeField] private bool startUnpress = false;
        [SerializeField] public Vector3 pressOffset;
        [Space]
        [SerializeField] public HandTouchEvent touchEventOpenStepHolder;
        [SerializeField] public Transform buttonOpenStepHolder;
        [Space]
        [SerializeField] public HandTouchEvent touchEventReplayVoiceline;
        [SerializeField] public Transform buttonReplayVoiceline;
        [Space]
        [SerializeField] public HandTouchEvent touchEventOpenScenarioHolder;
        [SerializeField] public Transform buttonOpenScenarioHolder;
        [Space]
        [SerializeField] public HandTouchEvent touchEventOpenRemise;
        [SerializeField] public Transform buttonOpenRemise;

        //[SerializeField] private TMP_Text textField;

        private bool buttonPressed = false;
        private bool scenarioButtons = true;

        [Space]
        public UnityHandEvent OnPressed;
        public UnityHandEvent OnUnpressed;

        [Space]
        [SerializeField] public bool toggle = true;

        bool pressed = false;

        public Color unpressColor = Color.white;
        public Color pressColor = Color.white;

        //Mono Behavior Callbacks
        #region
        private void Start()
        {
            if (openRemise)
            {
                openRemise.onClick.AddListener(ToRemise);
            }
            openStepHolder.onClick.AddListener(() => { ShiftHolder(false, true); });
            openScenarioHolder.onClick.AddListener(() => { ShiftHolder(true, false); });
            ShiftHolder(true, false);
        }

        void OnEnable()
        {
            touchEventOpenStepHolder.HandStartTouchEvent += OnTouchStepHolder;
            touchEventOpenScenarioHolder.HandStartTouchEvent += OnTouchScenarioHolder;
            touchEventOpenRemise.HandStartTouchEvent += OnTouchRemise;
            touchEventOpenStepHolder.HandStopTouchEvent += OnUntouchStepHolder;
            touchEventOpenScenarioHolder.HandStopTouchEvent += OnUntouchScenarioHolder;
            touchEventOpenRemise.HandStopTouchEvent += OnUntouchRemise;
            touchEventReplayVoiceline.HandStartTouchEvent += OnTouchReplayVoice;
            touchEventReplayVoiceline.HandStopTouchEvent += OnUntouchReplayVoice;
        }
        void OnDisable()
        {
            touchEventOpenStepHolder.HandStartTouchEvent -= OnTouchStepHolder;
            touchEventOpenScenarioHolder.HandStartTouchEvent -= OnTouchScenarioHolder;
            touchEventOpenRemise.HandStartTouchEvent -= OnTouchRemise;
            touchEventOpenStepHolder.HandStopTouchEvent -= OnUntouchStepHolder;
            touchEventOpenScenarioHolder.HandStopTouchEvent -= OnUntouchScenarioHolder;
            touchEventOpenRemise.HandStopTouchEvent -= OnUntouchRemise;
            touchEventReplayVoiceline.HandStartTouchEvent -= OnTouchReplayVoice;
            touchEventReplayVoiceline.HandStopTouchEvent -= OnUntouchReplayVoice;
        }

        #endregion

        //Press Event Register
        #region 
        void OnTouchStepHolder(Hand hand)
        {
            if (!pressed)
            {
                //    ShiftHolder(false, true);
                //    TouchClipboard.Instance.SetTransforms(true, false);
                //    TouchClipboard.Instance.StartScenario(ScenarioManager.Instance.currentActiveScenario);
                PressButton(hand, buttonOpenStepHolder);
            }
        }
        void OnTouchScenarioHolder(Hand hand)
        {
            if (!pressed)
            {
                //ShiftHolder(true, false);
                //TouchClipboard.Instance.SetTransforms(false, true);
                //TouchClipboard.Instance.ResetScenario();
                //TouchClipboard.Instance.OnSetScenario(ScenarioManager.Instance.currentActiveScenario);
                PressButton(hand, buttonOpenScenarioHolder);
            }
        }
        void OnTouchRemise(Hand hand)
        {
            if (!pressed)
            {
                //ToRemise();
                PressButton(hand, buttonOpenRemise);
            }
        }

        void OnTouchReplayVoice(Hand hand)
        {
            PressButton(hand, buttonReplayVoiceline);
        }



        #endregion

        //Release Event Register
        #region
        void OnUntouchStepHolder(Hand hand)
        {
            if (pressed )
            {
                ShiftHolder(false, true);
                TouchClipboard.Instance.SetTransforms(true, false);
                TouchClipboard.Instance.StartScenario(ScenarioManager.Instance.currentActiveScenario);
                ReleaseButton(hand, buttonOpenStepHolder);
            }
                
        }
        void OnUntouchScenarioHolder(Hand hand)
        {
            if (pressed)
            {
                ShiftHolder(true, false);
                TouchClipboard.Instance.SetTransforms(false, true);
                TouchClipboard.Instance.ResetScenario();
                TouchClipboard.Instance.OnSetScenario(ScenarioManager.Instance.currentActiveScenario);
                ReleaseButton(hand, buttonOpenScenarioHolder);
            }
                
        }
        void OnUntouchRemise(Hand hand)
        {
            if (pressed)
            {
                ToRemise();
                ReleaseButton(hand, buttonOpenRemise);
            }
                
        }

        void OnUntouchReplayVoice(Hand hand)
        {
            ReleaseButton(hand, buttonReplayVoiceline);
            TouchClipboard.Instance.OnVoicelineButtonClicked();
        }
        #endregion


        //Button Press/Release effect
        #region
        void PressButton(Hand hand, Transform btn)
        {
            if (!pressed)
                btn.localPosition += pressOffset;
            pressed = true;
            OnPressed?.Invoke(hand);
            btn.GetComponent<MeshRenderer>().material.color = pressColor;
        }

        void ReleaseButton(Hand hand, Transform btn)
        {
            if (pressed)
                btn.localPosition -= pressOffset;
            pressed = false;
            OnUnpressed?.Invoke(hand);
            btn.GetComponent<MeshRenderer>().material.color = unpressColor;
        }
        #endregion

        //Button Actions
        #region

        public void ShiftHolder(bool hasScenario, bool hasStep)
        {
            ScenarioHolderNavigation.gameObject.SetActive(hasScenario);
            StepHolderNavigation.gameObject.SetActive(hasStep);
            if (hasScenario)
            {
                touchEventOpenRemise.GetComponent<BoxCollider>().enabled = false;
                touchEventOpenStepHolder.GetComponent<BoxCollider>().enabled = false;
                Invoke("EnableColliderScenario", 0.2f);
            }
            else if (hasStep)
            {
                touchEventOpenScenarioHolder.GetComponent<BoxCollider>().enabled = false;
                touchEventReplayVoiceline.GetComponent<BoxCollider>().enabled = false;
                Invoke("EnableColliderSteps", 0.2f);
            }
        }

        public void EnableColliderScenario()
        {
            touchEventOpenRemise.GetComponent<BoxCollider>().enabled = true;
            touchEventOpenStepHolder.GetComponent<BoxCollider>().enabled = true;
        }

        public void EnableColliderSteps()
        {
            touchEventOpenScenarioHolder.GetComponent<BoxCollider>().enabled = true;
            touchEventReplayVoiceline.GetComponent<BoxCollider>().enabled = true;
        }

        public void ToRemise()
        {
            ApplicationController.Instance.LoadToScene(SceneLoadDirection.ToMenu);
        }

        public void SetInteractability(Button button, bool isInteractable)
        {
            button.interactable = isInteractable;
        }

        public void SetButtonPress(bool option)
        {
            buttonPressed = option;
        }

        public bool GetButtonHolder()
        {
            return scenarioButtons;
        }
        #endregion
    }

}