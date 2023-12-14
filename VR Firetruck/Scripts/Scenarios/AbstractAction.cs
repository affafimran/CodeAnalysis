using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace _360Fabriek {
    public abstract class AbstractAction : MonoBehaviour {

        [Header("Abstract Action:")]
        [SerializeField] [ReadOnly] private State status = State.Uninitialised;

        [Space]
        [SerializeField] protected bool debug = true;
        [SerializeField] protected bool shouldSkip = false;
        [SerializeField] protected bool showInClipboard = true;
        [SerializeField] protected bool repeatOnFinish = false;
        [SerializeField] protected bool animateOnFinish = false;

        [Space]
        [SerializeField, ShowIf(nameof(animateOnFinish))] private Animator finishAnimator = null;
        [SerializeField, ShowIf(nameof(animateOnFinish))] private int animatorLayer = 0;
        [SerializeField, ShowIf(nameof(animateOnFinish))] private string animatorSpeedKey = "Speed";
        [SerializeField, ShowIf(nameof(animateOnFinish)), Range(-1, 1)] private int animatorSpeed = 1;

        [Space]
        [SerializeField, Range(0f, 60f)] private float secondsWaitBeforeFinish = 0.1f;
        [ShowIf(nameof(repeatOnFinish))]
        [SerializeField, Range(0f, 60f)] private float secondsWaitBeforeRepetitions = 0.1f;

        [field: SerializeField, Range(0, 20), ShowIf(nameof(repeatOnFinish))] public int RepetitionsNeeded { get; private set; }

        [Header("[OBSOLETE] for in editor usage, use DeactivateEvent instead")]
        [Tooltip("Obsolete")]
        public UnityEvent<ActionArg> FinishEvent = new UnityEvent<ActionArg>();
        [SerializeField, System.Obsolete] private Outline outline;

        [Header("Abstract Action Events:")]
        [SerializeField] protected bool showEvents = false;

        [Space]
        [ShowIf(nameof(showEvents))] public UnityEvent<ActionArg> InitEvent = new UnityEvent<ActionArg>();
        [ShowIf(nameof(showEvents))] public UnityEvent<ActionArg> ActivateEvent = new UnityEvent<ActionArg>();
        [ShowIf(nameof(showEvents))] public UnityEvent<ActionArg> DeactivateEvent = new UnityEvent<ActionArg>();
        [ShowIf(nameof(showEvents))] public UnityEvent<float> ValueChangeEvent = new UnityEvent<float>();
        [ShowIf(nameof(showEvents))] public UnityEvent OnGrabEvent = new UnityEvent();

        [HideInInspector] public UnityEvent<RepetitionArg> OnStepRepetitionEvent = new UnityEvent<RepetitionArg>();

        private Coroutine finishCoroutine = null;
        public bool ShowInClipboard => showInClipboard;
        public State Status => status;
        protected int TimesRepeated { get; private set; }

        public void Init() {
            if (status == State.Uninitialised) {
                FinishEvent.AddListener(OnFinishInternal);
                FinishEvent.AddListener(OnFinish);

                ActivateEvent.AddListener(OnActivateInternal);
                ActivateEvent.AddListener(OnActivate);

                DeactivateEvent.AddListener(OnDeactivateInternal);
                DeactivateEvent.AddListener(OnDeactivate);

                OnStepRepetitionEvent.AddListener(OnStepRepetition);

                InitAditional();

                status = State.Inactive;

                TryEnableOutline(false);

                print("Initialized => " + name + " (" + GetType().Name + ")");

                InitEvent?.Invoke(new ActionArg(this, State.Inactive));
            }
        }

        public void Disable() {
            animateOnFinish = false;
            shouldSkip = true;
            showInClipboard = false;
        }

        public void Finish(State status = State.Finished) {
            if (finishCoroutine == null) {
                finishCoroutine = StartCoroutine(FinishCoroutine(status));
            }
        }

        private IEnumerator FinishCoroutine(State status) {

            if (animateOnFinish && finishAnimator && animatorSpeed != 0) {

                int startPoint = (animatorSpeed < 0) ? 1 : 0;

                finishAnimator.Play(0, animatorLayer, startPoint);
                finishAnimator.SetFloat(animatorSpeedKey, animatorSpeed);
            }

            if (gameObject.activeSelf && (status == State.Skipped || !repeatOnFinish || TimesRepeated >= RepetitionsNeeded)) {

                yield return new WaitForSeconds(secondsWaitBeforeFinish);

                FinishEvent?.Invoke(new ActionArg(this, status));
                TimesRepeated = 0;
            } else {

                yield return new WaitForSeconds(secondsWaitBeforeRepetitions);

                OnStepRepetitionEvent?.Invoke(new RepetitionArg(RepetitionsNeeded - TimesRepeated));
                TimesRepeated++;
            }

            StopCoroutine(this.finishCoroutine);

            this.finishCoroutine = null;
        }

        public void Activate() {

            TimesRepeated = 0;

            this.ActivateEvent?.Invoke(new ActionArg(this, State.Active));
        }

        public void Deactivate() {

            this.DeactivateEvent?.Invoke(new ActionArg(this, State.Halted));
        }

        protected virtual void InitAditional() { }
        protected virtual void OnFinish(ActionArg arg) { }
        protected virtual void OnActivate(ActionArg arg) { }
        protected virtual void OnDeactivate(ActionArg arg) { }
        protected virtual void OnStepRepetition(RepetitionArg arg) { }

        private void OnFinishInternal(ActionArg arg) {

            this.status = arg.Status;

            print("Finish => " + name + " (" + this.GetType().Name + ") => " + this.status);
        }

        private void OnActivateInternal(ActionArg arg) {

            this.status = arg.Status;

            TryEnableOutline(true);

            print("Activate => " + name + " (" + this.GetType().Name + ") => " + this.status);

            if (this.shouldSkip && this.finishCoroutine == null) {

                Skip();
            }
        }

        private void OnDeactivateInternal(ActionArg arg) {

            TryEnableOutline(false);

            if (status == State.Active) {
                status = arg.Status;
            }

            print("Deactivate: => " + name + " (" + GetType().Name + ") => " + status);
        }

        public void SetAnimation()
        {
            if (finishAnimator)
            {
                finishAnimator.Play(0, animatorLayer, 0);
                finishAnimator.SetFloat(animatorSpeedKey, 0);
            }
        }

        public void Skip() {
            if (Status != State.Active) {
                return;
            }

            Finish(State.Skipped);

            print("Skipped => " + name + " (" + GetType().Name + ") => " + status);
        }

        [System.Obsolete]
        protected void TryEnableOutline(bool on) {

            if (outline) {

                outline.enabled = on;
            }
        }

        [System.Serializable]
        public class ActionArg : UnityEvent {

            public AbstractAction TriggeredAction;
            public State Status;

            public float Value;

            public ActionArg(AbstractAction triggeredAction, State state, float value = 0f) {

                this.TriggeredAction = triggeredAction;
                this.Status = state;
                this.Value = value;
            }
        }

        [System.Serializable]
        public class RepetitionArg : UnityEvent {

            public int RepetitionsLeft { get; private set; }

            public RepetitionArg(int repetionsLeft) {
                RepetitionsLeft = repetionsLeft;
            }
        }
    }
}