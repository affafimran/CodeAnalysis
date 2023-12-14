using UnityEngine;
using UnityEngine.Events;
using _360Fabriek.Scenarios.Actions;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios {
    public class AnimatedSlideAction : AbstractAction {
        [Header("Animated Slide Action:")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedKey = "Speed";
        [SerializeField] private int layer;
        [Space]
        [SerializeField] private VRTouchAction moveInTouchable;
        [SerializeField] private VRTouchAction moveOutTouchable;

        private float newSpeed = 0;
        private float currentSpeed = 0;
        private float normalizedTarget = 0;

        private float NormalizedProgress => animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
        private float NormalizedProgressClamped => Mathf.Clamp01(NormalizedProgress);

        protected override void InitAditional() {
            InitMoveTouchable(moveInTouchable, () => Move(-3), () => Move(0));
            InitMoveTouchable(moveOutTouchable, () => Move(3), () => Move(0));

            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }
        }

        private void OnSetScenario(Scenario _) {
            if (animator) {
                animator.SetFloat(speedKey, 0f);
                animator.Play(0, layer, 0f);
            }
        }

        private void InitMoveTouchable(VRTouchAction touch, UnityAction move, UnityAction stop) {
            touch.Init();
            touch.OnTouchStart.AddListener(move);
            touch.OnTouchStop.AddListener(stop);
        }

        protected override void OnActivate(ActionArg arg) {
            animator.Play(0, layer, NormalizedProgress);
            SetNormalizedTarget();

            moveInTouchable.Activate();
            moveOutTouchable.Activate();

            newSpeed = animator.GetFloat(speedKey);
        }

        protected override void OnFinish(ActionArg arg) {
            moveInTouchable.Deactivate();
            moveOutTouchable.Deactivate();
            newSpeed = 0;
        }

        private void SetNormalizedTarget() {
            if (NormalizedProgressClamped == 0) {

                normalizedTarget = 1;
                return;
            }

            normalizedTarget = 0;
        }

        private void Move(int speed) {
            newSpeed = speed;
        }

        private void FixedUpdate() {
            if (Status == State.Active) {
                TrySetAnimationFloat();
            }
        }

        private void TrySetAnimationFloat() {
            float min = 0f;
            float max = 1f;

            if (newSpeed != 0) {
                if (NormalizedProgress < min && newSpeed < 0) {
                    newSpeed = 0;
                }

                if (NormalizedProgress > max && newSpeed > 0) {
                    newSpeed = 0;
                }

                if (NormalizedProgressClamped == normalizedTarget) {

                    moveInTouchable.Finish();
                    moveOutTouchable.Finish();

                    Finish();

                    return;
                }
            }

            if (newSpeed == currentSpeed) {
                return;
            }

            ApplySpeedValue();
        }

        private void ApplySpeedValue() {
            currentSpeed = newSpeed;
            print("Apply " + currentSpeed);
            animator.SetFloat(speedKey, currentSpeed);
        }

#if UNITY_EDITOR

        private void Update() {
            if (Status == State.Active) {
                if (Input.GetKeyDown(KeyCode.W)) {
                    moveOutTouchable.OnTouchStart?.Invoke();
                    Move(-1);
                }

                if (Input.GetKeyDown(KeyCode.S)) {
                    moveInTouchable.OnTouchStart?.Invoke();
                    Move(1);
                }

                if (Input.GetKeyUp(KeyCode.W)) {
                    moveOutTouchable.OnTouchStop?.Invoke();
                    Move(0);
                }

                if (Input.GetKeyUp(KeyCode.S)) {
                    moveInTouchable.OnTouchStop?.Invoke();
                    Move(0);
                }
            }
        }
#endif
    }
}