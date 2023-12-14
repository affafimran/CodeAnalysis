using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;
using Autohand;
namespace _360Fabriek.Scenarios
{
    public class ClipboardTouchButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scenarioTitle;
        //[SerializeField] private Button scenarioButton;
        //[Space]
        //[SerializeField] public UnityEvent<ClipboardTouchButton> OnClickEvent = new UnityEvent<ClipboardTouchButton>();
        //[Space]
        [SerializeField] [ReadOnly] public new string name;
        [NaughtyAttributes.HideIf("startUnpress")]
        [SerializeField] private bool startPress = false;
        [NaughtyAttributes.HideIf("startPress")]
        [SerializeField] private bool startUnpress = false;
        [SerializeField] private HandTouchEvent touchEvent;
        [SerializeField] private Transform button;
        [SerializeField] public Vector3 pressOffset;

        public Color unpressColor = Color.white;
        public Color pressColor = Color.white;

        [Space]
        public UnityHandEvent OnPressed;
        public UnityHandEvent OnUnpressed;

        [Space]
        [SerializeField] public bool toggle = true;

        bool pressed = false;
        // Start is called before the first frame update
        void Start()
        {
           // scenarioButton.onClick.AddListener(OnButtonClick);
            
        }

        // Update is called once per frame
        void OnEnable()
        {
            touchEvent.HandStartTouchEvent += OnTouch;
           touchEvent.HandStopTouchEvent += OnUntouch;
        }
        void OnDisable()
        {
            touchEvent.HandStartTouchEvent -= OnTouch;
            touchEvent.HandStopTouchEvent -= OnUntouch;
        }

        public bool GetPressedAction()
        {
            return pressed;
        }
        public void SetScenarioName(string name)
        {
            if (scenarioTitle)
            {
                scenarioTitle.text = name;
            }

            this.name = name;
        }

        void OnTouch(Hand hand)
        {
            if (!pressed)
            {
                PressButton(hand);
            }

        }
        void OnUntouch(Hand hand)
        {
            if (pressed && !toggle)
                ReleaseButton(hand);
        }

      

        void PressButton(Hand hand)
        {
            if (!pressed)
                button.localPosition += pressOffset;
            //pressed = true;
            TouchClipboard.Instance.OnClipboardButtonClicked(this);
            OnPressed?.Invoke(hand);
            //button.GetComponent<MeshRenderer>().material.color = pressColor;
        }

        void ReleaseButton(Hand hand)
        {
            if (pressed)
                button.localPosition -= pressOffset;
            pressed = false;
            OnUnpressed?.Invoke(hand);
            button.GetComponent<MeshRenderer>().material.color = unpressColor;
        }

        //private void OnButtonClick()
        //{
        //    OnClickEvent?.Invoke(this);
        //}
    }
}