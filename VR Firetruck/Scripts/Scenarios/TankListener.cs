using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios.Listeners {
    public class TankListener : MonoBehaviour {
        [SerializeField] private float speed;
        [SerializeField] private int maxCapacity;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI fillText;
        [SerializeField] private TankSegment[] tankSegments;

        [SerializeField] private UnityEvent TargetReached = new UnityEvent();

        private int targetValue;
        private float currentValue;
        private Coroutine moveToTargetCoroutine;

        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }

            OnSetScenario(null);
        }

        private void OnSetScenario(Scenario _) {
            SetStartValue(maxCapacity);
        }

        public void StartMovingToTarget() {
            if(moveToTargetCoroutine == null) {
                moveToTargetCoroutine = StartCoroutine(MoveToTarget());
            }
        }

        public void StopMovingToTarget() {
            StopAllCoroutines();
            moveToTargetCoroutine = null;
        }

        public void SetStartValue(int value) {
            currentValue = Mathf.Clamp(value, 0, maxCapacity);
            UpdateUI();
        }

        public void SetTargetValue(int value) {
            targetValue = Mathf.Clamp(value, 0, maxCapacity);
        }

        private void UpdateUI() {
            int roundedValue = Mathf.RoundToInt(currentValue);
            float normalizedAmountFilled = (float)roundedValue / (float)maxCapacity;
            int percentageFilled = (int)(normalizedAmountFilled * 100f);

            fillText.text = roundedValue.ToString();
            fillImage.fillAmount = normalizedAmountFilled;

            foreach(TankSegment segment in tankSegments) {
                segment.Logic(percentageFilled);
            }
        }

        private IEnumerator MoveToTarget() {
            float targetIsPositive = targetValue > currentValue ? 1f : -1f;
            float timer = 0f;

            while(currentValue != targetValue) {
                currentValue += speed * targetIsPositive * Time.deltaTime;
                currentValue = Mathf.Clamp(currentValue, 0, maxCapacity);

                timer += Time.deltaTime;

                if ((targetIsPositive > 0 && currentValue > targetValue) || (targetIsPositive < 0 && currentValue < targetValue)) {
                    currentValue = targetValue;
                }

                UpdateUI();

                yield return new WaitForEndOfFrame();
            }

            moveToTargetCoroutine = null;
            TargetReached?.Invoke();
        }

        [System.Serializable]
        private class TankSegment {
            [SerializeField, Tooltip("This is in percentage filled")] 
            private int minValue;
            [SerializeField] private MeshRenderer mesh;

            [Header("Extra settings")]
            [SerializeField] private bool showIfLessThanMin;
            [SerializeField] private Material overrideMaterial;

            private Material defaultMaterial;

            public void Logic(int percentageFilled) {
                if (!defaultMaterial) {
                    defaultMaterial = mesh.material;
                }

                bool isMoreThanMin = percentageFilled >= minValue;

                if (showIfLessThanMin) {
                    mesh.material = isMoreThanMin ? defaultMaterial : overrideMaterial;
                    return;
                }

                mesh.gameObject.SetActive(isMoreThanMin);
            }
        }
    }
}