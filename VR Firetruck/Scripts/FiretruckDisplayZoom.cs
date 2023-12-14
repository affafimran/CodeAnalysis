using UnityEngine;
using _360Fabriek.Utility;
using Autohand;
using System.Collections;
using _360Fabriek.Controllers;

namespace _360Fabriek.UX {
    public class FiretruckDisplayZoom : MonoBehaviour {
        [SerializeField] private float interactionRange;
        [SerializeField] private GameObject magnifyingGlass;
        [SerializeField] private GameObject fingerIcon;
        [Space]
        [SerializeField] private float speedMultiplier;
        [SerializeField] private AnimationData animationData;

        private bool isZoomedIn;
        private Coroutine makeSmallCor;

        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }

            animationData.SetAnimatorSpeed(speedMultiplier);
            SetActiveIcons(false);
            SetAnimationSpeed(0, 0);
        }

        private void Update() {
            if (!isZoomedIn && magnifyingGlass.activeSelf) {
                if (TouchLogic(out HandControllerLink hand)) {
                    MakeBig(hand);
                }
            }
        }

        private void OnSetScenario(Scenario _) {
            MakeSmall();
        }

        public void MakeSmall() {
            if(isZoomedIn && makeSmallCor == null) {
                makeSmallCor = StartCoroutine(MakeSmallCor());
            }
        }

        private IEnumerator MakeSmallCor() {
            SetAnimationSpeed(-1, 1);

            yield return new WaitForSeconds(1f * speedMultiplier);
            isZoomedIn = false;
            makeSmallCor = null;
        }

        private void MakeBig(HandControllerLink hand) {
            if (!isZoomedIn) {
                isZoomedIn = true;
                SetActiveIcons(false);

                if (hand) {
                    float duration = .1f;
                    float strength = 1f;

                    hand.TryHapticImpulse(duration, strength);
                }

                SetAnimationSpeed(1, 0);
            }
        }

        private bool TouchLogic(out HandControllerLink hand) {
            hand = null;

            if (!Player.Instance) {
                return false;
            }

            bool isTouching = false;

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
            }

            return isTouching;
        }

        private bool CheckRange(Transform index) {
            if (!index) {
                return false;
            }

            float range = Vector3.Distance(index.position, magnifyingGlass.transform.position);
            return range < interactionRange;
        }

        public void SetActiveIcons(bool on) {
            magnifyingGlass.SetActive(on);
            fingerIcon.SetActive(on);
        }

        private void SetAnimationSpeed(float speed, float normalizedStartPosition) {
            speed = Sign0(speed);
            normalizedStartPosition = Mathf.Clamp01(normalizedStartPosition);

            animationData.ApplyValues(speed, normalizedStartPosition);
        }

        private float Sign0(float value) {
            return value == 0 ? 0 : value > 0 ? 1f : -1f;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(magnifyingGlass.transform.position, interactionRange);
        }
    }
}