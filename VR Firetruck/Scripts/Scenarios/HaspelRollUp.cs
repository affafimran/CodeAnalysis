using UnityEngine;
using System.Collections;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios.Actions {
    public class HaspelRollUp : VRTouchAction {
        [SerializeField] private AnimatorData[] animatorDatas;

        protected override void Start() {
            base.Start();

            foreach (AnimatorData data in animatorDatas) {
                data.Init(this);
            }

            ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
        }

        protected override void OnActivate(ActionArg arg) {
            base.OnActivate(arg);

            StartCoroutine(CheckAnimationCompletion());
        }

        protected override void OnDeactivate(ActionArg arg) {
            base.OnDeactivate(arg);

            StopAllCoroutines();
        }

        private void OnSetScenario(Scenario _) {
            foreach(AnimatorData data in animatorDatas) {
                data.Reset();
            }
        }

        private IEnumerator CheckAnimationCompletion() {
            float normalizedAnimationProgress = animatorDatas[0].NormalizedStartProgress;
            float normalizedTarget;

            if (normalizedAnimationProgress == 1) {
                normalizedTarget = 0;

                while(normalizedAnimationProgress > normalizedTarget) {
                    normalizedAnimationProgress = animatorDatas[0].NormalizedProgress;
                    yield return null;
                }
            } else {
                normalizedTarget = 1;

                while(normalizedAnimationProgress < normalizedTarget) {
                    normalizedAnimationProgress = animatorDatas[0].NormalizedProgress;
                    yield return null;
                }
            }

            Finish();
        }

        [System.Serializable]
        private class AnimatorData {
            [SerializeField] private Animator animator;
            [SerializeField] private int layer;
            [SerializeField] private string speedKey;

            [Range(-1f, 1f)]
            [SerializeField] private float speed;
            [Range(0f, 1f)]
            [SerializeField] private float normalizedStartPosition;

            public float NormalizedStartProgress => normalizedStartPosition;
            public float NormalizedProgress => animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;

            private bool hasStartedPlaying;

            public void Init(VRTouchAction action) {
                SetSpeed(0);

                action.OnTouchStart.AddListener(Play);
                action.OnTouchStop.AddListener(Stop);
            }

            public void Play() {
                SetSpeed(speed);

                if (!hasStartedPlaying) {
                    hasStartedPlaying = true;
                    animator.Play(0, layer, normalizedStartPosition);
                }
            }

            public void Reset() {
                hasStartedPlaying = false;
            }

            public void Stop() {
                SetSpeed(0);
            }

            private void SetSpeed(float speed) {
                animator.SetFloat(speedKey, speed);
            }
        }
    }
}