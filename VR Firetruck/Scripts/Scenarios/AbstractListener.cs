using _360Fabriek.Controllers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static _360Fabriek.AbstractAction;

namespace _360Fabriek.Scenarios.Listeners {
    public abstract class AbstractListener : MonoBehaviour {
        [Header("Abstract Action Listener:")]
        [SerializeField] protected bool finishActionWithDestination = false;
        [SerializeField] protected bool startOnInitialise = false;
        [SerializeField] protected float wait = 0.05f;
        [Space]
        [SerializeField] protected float minValue = 0f;
        [SerializeField] protected float maxValue = 25f;
        [SerializeField] protected float startValue = 0f;
        [SerializeField, ReadOnly] protected float currentValue = 0f;
        [Space]
        [ShowIf(nameof(finishActionWithDestination))]
        [SerializeField] protected float destinationValue = 15f;
        [ShowIf(nameof(finishActionWithDestination))]
        [SerializeField] protected float destinationRange = 0.1f; 
        [Header("Abstract Action Listener Events:")]
        [SerializeField] private bool showEvents = false;
        [Space]
        [ShowIf(nameof(showEvents))] public UnityEvent InitialiseEvent = new UnityEvent();
        [ShowIf(nameof(showEvents))] public UnityEvent WithinDestinationEvent = new UnityEvent();
        [ShowIf(nameof(showEvents))] public UnityEvent<float> ValueChangeEvent = new UnityEvent<float>();

        public float Value => currentValue;

        public void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }

            if (minValue >= maxValue) {
                return;
            }

            InitAditional();

            InitialiseEvent?.Invoke();

            if (startOnInitialise) {
                OnActionActivate(null);
            }

            ScenarioManager.Instance.OnScenarioSet.AddListener((Scenario scenario) => { OnActionValueChange(0f); });
        }

        protected virtual void InitAditional() { }
        protected virtual void ResetListener() {
            currentValue = startValue;
        }

        public virtual void OnActionActivate(ActionArg arg) { }
        public virtual void OnActionDeactivate(ActionArg arg) { }

        public virtual void OnActionValueChange(float value) {
            ValueChangeEvent?.Invoke(value);
        }

        protected void CheckValueToFinish() {
            if (finishActionWithDestination) {
                if (Mathf.Abs(destinationValue - currentValue) < destinationRange) {
                    WithinDestinationEvent?.Invoke();
                }
            }
        }

        public void SetWait(float wait) {
            this.wait = wait;
        }
    }
}