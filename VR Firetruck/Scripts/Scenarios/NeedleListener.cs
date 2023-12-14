using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

using static _360Fabriek.AbstractAction;

namespace _360Fabriek.Scenarios.Listeners {
    public class NeedleListener : AbstractListener {
        [Header("Dial Action Listener:")]
        [SerializeField] private Transform dial;
        [SerializeField] private float speed = 25f;
        [SerializeField] private bool invertDirection = false;
        [Space]
        [Range(0, 360)] [SerializeField] private float maxRotationalLimit = 270;
        [Range(-360, 0)] [SerializeField] private float minRotationalLimit = 0;
        [Range(-360, 360)] [SerializeField] private float startRotation = 0;
        [Header("Debug Values: ")]
        [Range(-360, 360)] [SerializeField] private float currentRotation = 0;
        [Range(-360, 360)] [SerializeField] [ReadOnly] private float actualRotation = 0;
        [Space]
        [SerializeField] [ReadOnly] private float minRangeValue = 0;
        [SerializeField] [ReadOnly] private float maxRangeValue = 0;
        [SerializeField] [ReadOnly] private float minRangeRotation = 0;
        [SerializeField] [ReadOnly] private float maxRangeRotation = 0;

        private float value = 0f;
        private IEnumerator coroutine = null;
        [SerializeField] public static float currentValuePoint;


        protected override void InitAditional() {
            maxRangeValue = maxValue - startValue;
            minRangeValue = startValue - minValue;
            currentValue = startValue;

            maxRangeRotation = maxRotationalLimit - startRotation;
            minRangeRotation = startRotation - minRotationalLimit;

            currentRotation = startRotation;
            actualRotation = startRotation;

            dial.localRotation = Quaternion.Euler(0, 0, -startRotation);
        }

        public override void OnActionActivate(ActionArg arg) {
            coroutine = RotationCoroutine();

            StartCoroutine(coroutine);
        }

        public override void OnActionDeactivate(ActionArg arg) {
            if (coroutine == null) {
                StopCoroutine(coroutine);
            }
        }

        public void SetSpeed(float speed) {
            this.speed = speed;
        }

        private IEnumerator RotationCoroutine() {
            while (true) {
                while (actualRotation != currentRotation) {
                    bool direction = (actualRotation < currentRotation);
                    actualRotation += (speed * Time.deltaTime / 1f) * ((direction) ? 1 : -1);

                    if (Math.Abs(actualRotation - currentRotation) < 1) {
                        actualRotation = currentRotation;
                    }

                    dial.localRotation = Quaternion.Euler(0, 0, -actualRotation);
                    CalculateValue();

                    yield return new WaitForSeconds(wait);
                }

                yield return new WaitForSeconds(0.25f);

                if (Mathf.Abs(actualRotation - currentRotation) > 1) {
                    CheckValueToFinish();
                }
            }
        }

        public override void OnActionValueChange(float value) {
            if (invertDirection) {
                value = -value;
            }

            float corrected = (value > 100) ? 100 : (value < -100) ? -100 : value;
            float percentage = (((corrected < 0) ? minRangeRotation : maxRangeRotation) / 100f) * corrected;

            currentRotation = percentage + startRotation;

            base.OnActionValueChange(value);
        }


        public void setCurrentValue(float value)
        {
            currentValuePoint = value;
        }
        public float getCurrentValue()
        {
            return currentValuePoint;
        }
        public void SetPercentage(float value) { OnActionValueChange(value); }

        private void CalculateValue() {
            if (actualRotation > startRotation) {
                currentValue = maxRangeValue / maxRangeRotation;
            } else if (actualRotation < startRotation) {
                currentValue = minRangeValue / minRangeRotation;
            } else {
                currentValue = 0;
            }

            currentValue *= actualRotation - startRotation;
            currentValue += startValue;

            currentValue = (float)Math.Round(currentValue, 1);

            if (value != currentValue) {
                value = currentValue;
                ValueChangeEvent?.Invoke(currentValue);
            }
        }

       
    }
}
