using Autohand;
using UnityEngine;
using TMPro;
using NaughtyAttributes;
using UnityEngine.Events;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios {
    public class PercentageDialAction: GrabbableAction {
        [SerializeField] private UnityEvent<float> OnValueChanged = new UnityEvent<float>();
        [Space]
        [Header("Dial Action:")]
        [SerializeField] private HingeJoint dialHingeJoint = null;

        public float startingPercentage;
        private float StartingPercentage {
            get { return startingPercentage; }
            set { startingPercentage = Mathf.Clamp(value, 0f, 100f); }
        }

        [Range(0f, 100f)]
        [SerializeField] private float finishPercentage = 20f;
        [SerializeField] private float finishThreshold = 2.5f;

        [Range(-180f, 180f)]
        [SerializeField] private float dialMinValue = -75f;

        [Space]
        [SerializeField] private TMP_Text text0;
        [SerializeField, ReadOnly] private float currentPercentage;

        private bool releasedThisFrame;
        private Vector3 originalDialRotation;

        [Range(-180f, 180f)]
        [SerializeField] private float dialMaxValue = 75f;

        private float DialMinValue {
            get { return dialMinValue; }
            set { dialMinValue = Mathf.Clamp(value, -180f, DialMaxValue); }
        }

        
        private float DialMaxValue {
            get { return dialMaxValue; }
            set { dialMaxValue = Mathf.Clamp(value, DialMinValue, 180f); }
        }

        

        protected override void Start() {
            if (dialHingeJoint) {
                originalDialRotation = dialHingeJoint.transform.localEulerAngles + Mathf.Lerp(DialMinValue, DialMaxValue, StartingPercentage/100f) * dialHingeJoint.axis;
                SetDialLimits(DialMinValue, DialMaxValue);
                dialHingeJoint.transform.localEulerAngles = originalDialRotation;
            }
            ScenarioManager.Instance.OnScenarioSet.AddListener(_ => { OnSetScenario(); });
            grabbable.OnReleaseEvent += TryRelease;

            base.Start();
        }

        private void Update() {
            if (Status != State.Active) { return; }

            currentPercentage = (dialHingeJoint.angle - dialHingeJoint.limits.min)/Mathf.Abs(dialHingeJoint.limits.min - dialHingeJoint.limits.max)*100;
            ScenarioManager.Instance.valueTest.text = currentPercentage.ToString();
            if (ReachedTarget() && releasedThisFrame) {
                Finish();
            }

            releasedThisFrame = false;

            if(currentPercentage != Mathf.Infinity) {
                float shownValue = (float)decimal.Round((decimal)currentPercentage, 2);
                if(text0) {
                    text0.text = shownValue.ToString();
                }

                OnValueChanged?.Invoke(shownValue);
            }
        }

        private void TryRelease(Hand hand, Grabbable grabbable) {
            releasedThisFrame = true;
        }

        private bool ReachedTarget() {
            bool hasReached = false;

            if(currentPercentage > finishPercentage - finishThreshold && currentPercentage < finishPercentage + finishThreshold) {
                hasReached = true;
            }

            return hasReached;
        }

        public void OnSetScenario() {
            dialHingeJoint.transform.localEulerAngles = originalDialRotation;
        }

        public void SetDialLimits(float newMinValue, float newMaxValue) {
            JointLimits limits = dialHingeJoint.limits;
            DialMinValue = newMinValue;
            DialMaxValue = newMaxValue;
            limits.min = DialMinValue;
            limits.max = DialMaxValue;
            dialHingeJoint.limits = limits;
        }


    }
}
