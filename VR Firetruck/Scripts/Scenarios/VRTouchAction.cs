using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using _360Fabriek.Utility;
using NaughtyAttributes;
using Autohand;
using _360Fabriek.Controllers;


namespace _360Fabriek.Scenarios.Actions {

    public class VRTouchAction : AbstractAction {

        [ShowIf(nameof(showEvents))] public UnityEvent OnTouchStart = new UnityEvent();
        [ShowIf(nameof(showEvents))] public UnityEvent OnTouchStop = new UnityEvent();

        [Space(5), Header("VR Touch Action:")]
        [SerializeField] private float interactRange = .02f;
        [SerializeField] private bool finishOutsideOfRange = false;
        [SerializeField] private Transform button;
        [SerializeField] private bool finishOnTouch = true;
        [SerializeField] private GameObject highlight;

        private HandControllerLink touchingHand;
        private bool wasTouchedLastFrame;
        
        private void OnTouchStart_Internal() {
            if (touchingHand) {
                float duration = .1f;
                float strength = 1f;

                touchingHand.TryHapticImpulse(duration, strength);
            }

            if (finishOnTouch) {

                StaticUtilities.TrySetActive(highlight, false);
                Finish();
            }
        }

        protected override void OnActivate(ActionArg arg) {
            StartCoroutine(RangeChecker());
            StaticUtilities.TrySetActive(highlight, true);
        }

        protected override void OnDeactivate(ActionArg arg) {
            StaticUtilities.TrySetActive(highlight, false);
        }

        protected override void OnStepRepetition(RepetitionArg arg) {
            StaticUtilities.TrySetActive(highlight, true);
        }

        protected virtual void Start() {
            OnTouchStart.AddListener(OnTouchStart_Internal);

            StaticUtilities.TrySetActive(highlight, false);
            if (ScenarioManager.Instance)
            {
                ScenarioManager.Instance.OnScenarioFinish.AddListener(OnSetScenario);
            }
        }

        private void OnSetScenario(Scenario _)
        {
            SetAnimation();
        }

        private IEnumerator RangeChecker() {
            while (Status == State.Active) {
                if (Player.Instance) {
                    bool isTouching = TouchLogic();
                    wasTouchedLastFrame = isTouching;
                }

                yield return new WaitForSeconds(.2f);
            }
        }

        private bool TouchLogic() {
            if (!Player.Instance) {
                return false;
            }

            bool isTouching = false;
            HandControllerLink hand = null;

            if (Application.isEditor) {
                Transform editorIndex = Player.Instance.EditorIndex;

                isTouching = CheckRange(editorIndex);
            } else {
                Transform leftIndex = Player.Instance.LeftIndex;
                Transform rightIndex = Player.Instance.RightIndex;

                if (CheckRange(leftIndex)) {
                    isTouching = true;
                    hand = HandControllerLink.handLeft;
                } else if (CheckRange(rightIndex)) {
                    isTouching = true;
                    hand = HandControllerLink.handRight;
                }

                touchingHand = hand;
            }

            if (isTouching != wasTouchedLastFrame) {
                if (isTouching) {
                    OnTouchStart?.Invoke();
                } else {
                    OnTouchStop?.Invoke();
                }
            }

            return isTouching;
        }

        private bool CheckRange(Transform index) {
            if (!index) {
                return false;
            }

            float range = Vector3.Distance(index.position, button.position);
            return finishOutsideOfRange ? range > interactRange : range < interactRange;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (button && debug) {
                Gizmos.DrawWireSphere(button.position, interactRange);
            }
        }
#endif
    }
}