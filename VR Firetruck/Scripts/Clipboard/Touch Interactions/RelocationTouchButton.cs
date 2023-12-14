using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Autohand;


namespace _360Fabriek
{
    public class RelocationTouchButton : MonoBehaviour
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
            if (pressed)
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
            RelocationController.Instance.ResetPosition();
            button.GetComponent<MeshRenderer>().material.color = unpressColor;
        }



    }
}