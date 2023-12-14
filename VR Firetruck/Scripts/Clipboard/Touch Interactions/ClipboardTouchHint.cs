using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using Autohand;
using _360Fabriek.Controllers;
using static _360Fabriek.Controllers.ScenarioManager;

namespace _360Fabriek.Scenarios
{
    public class ClipboardTouchHint : MonoBehaviour
    {
        [Space]
        [SerializeField] [ReadOnly] public new string name;
        [NaughtyAttributes.HideIf("startUnpress")]
        [SerializeField] private bool startPress = false;
        [NaughtyAttributes.HideIf("startPress")]
        [SerializeField] private bool startUnpress = false;
        [SerializeField] private HandTouchEvent touchEvent;
        [SerializeField] private Transform button;
        [SerializeField] public Vector3 pressOffset;
        [Space]
        [SerializeField] private Image hintIcon;
        public Color unpressColor = Color.white;
        public Color pressColor = Color.white;

        [Space]
        public UnityHandEvent OnPressed;
        public UnityHandEvent OnUnpressed;

        [Space]
        bool toggle = true;

        bool pressed = false;
        // Start is called before the first frame update
        void Start()
        {

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

      
       

        void OnTouch(Hand hand)
        {
            if (!pressed)
            {
                PressButton(hand);
            }

        }
        void OnUntouch(Hand hand)
        {
            if (pressed )
                ReleaseButton(hand);
        }



        void PressButton(Hand hand)
        {
            if (!pressed)
                button.localPosition += pressOffset;
            pressed = true;
            OnPressed?.Invoke(hand);
            button.GetComponent<MeshRenderer>().material.color = pressColor;
        }

        void ReleaseButton(Hand hand)
        {
            if (pressed)
                button.localPosition -= pressOffset;
            pressed = false;
            OnUnpressed?.Invoke(hand);
            SetHint();
            button.GetComponent<MeshRenderer>().material.color = unpressColor;
        }


        void SetHint()
        {
            toggle = !toggle;
            hintIcon.enabled = toggle;
            DifficultySetting difficultySetting = new DifficultySetting(Random.Range(2, 60), toggle, true);
            ScenarioManager.Instance.ChangeSetting(difficultySetting);
            
        }
        
    }
}