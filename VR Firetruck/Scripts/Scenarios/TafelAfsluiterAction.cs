using UnityEngine;
using TMPro;
using NaughtyAttributes;
using Autohand;
using _360Fabriek.Controllers;

namespace _360Fabriek {
    public class TafelAfsluiterAction : GrabbableAction {
        [Space]
        [Header("Crank Action:")]
        [SerializeField] private Rigidbody crankRotatingBaseRigidbody = null;
        [SerializeField] private HingeJoint rotatingBaseHingeJoint = null;
        [SerializeField] private float turnToFinishAction = 8;

        [Header("Constraints: ")]
        [SerializeField] private float maximumTurns = 8;
        public float startingTurns;
        private float StartingTurns {
            get { return startingTurns; }
            set { startingTurns = Mathf.Clamp(value, 0f, maximumTurns); }
        }

        [Space]
        [SerializeField] private TMP_Text text0;
        [SerializeField, ReadOnly] private float currentTurn;

        private float lastRotation;
        private Vector3 originalRotation;
        private float turnRestant;
        private bool needsToReachTargetClockwise;

        protected override void OnActivate(ActionArg arg) {
            base.OnActivate(arg);
            if (text0) {
                text0.enabled = true;
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            base.OnDeactivate(arg);
            if (text0) {
                text0.enabled = false;
            }
        }

        protected override void Start() {
            if (rotatingBaseHingeJoint) {
                originalRotation = rotatingBaseHingeJoint.transform.localEulerAngles + rotatingBaseHingeJoint.axis * 360f * (StartingTurns % 1f) - rotatingBaseHingeJoint.axis * 180f;
                Debug.Log(originalRotation);
                lastRotation = StartingTurns % 1f * 360f;
                currentTurn = StartingTurns;
                JointLimits limits = rotatingBaseHingeJoint.limits;
                //To test the starting rotation of Wheel at beginning.
                ScenarioManager.Instance.valueTest.text = ""+ rotatingBaseHingeJoint.transform.localEulerAngles.x +", " + rotatingBaseHingeJoint.transform.localEulerAngles.y+", "+ rotatingBaseHingeJoint.transform.localEulerAngles.z;
                limits.min = -180f;
                limits.max = 180f;
                rotatingBaseHingeJoint.limits = limits;
                rotatingBaseHingeJoint.transform.localEulerAngles = originalRotation;
            }

            text0.text = string.Empty;

            ScenarioManager.Instance.OnScenarioSet.AddListener(_ => { OnSetScenario(); });

            if (grabbable){
                grabbable.OnReleaseEvent += OnRelease;
                grabbable.OnGrabEvent += OnGrab;
            }

            needsToReachTargetClockwise = currentTurn < turnToFinishAction;

            base.Start();
        }

        private void FixedUpdate() {
            if (Status != State.Active)
            {
                //ScenarioManager.Instance.valueTest.text = "This step is not active";
                return;
            }
            else
            {
                //ScenarioManager.Instance.valueTest.text = "This step is active";
            }

            float currentDegrees = rotatingBaseHingeJoint.angle + 180f;
            bool firstTurn = false;
            if ((lastRotation - currentDegrees) > 300f && currentTurn < (maximumTurns)) {
                if (currentTurn <= 0f) {
                    firstTurn = true;
                } else {
                    currentTurn += 1f;
                }
            }


            if ((lastRotation - currentDegrees) < -300f && currentTurn > 0f) {
                currentTurn -= 1f;
            }

            if (currentTurn <= 0f && !firstTurn) {
                ScenarioManager.Instance.valueTest.text = "First check!!!";
                rotatingBaseHingeJoint.transform.localEulerAngles = originalRotation - rotatingBaseHingeJoint.axis * 180f;
                currentTurn = 0.0f;
                turnRestant = 0f;
            } else if (currentTurn >= maximumTurns && !firstTurn) {
                ScenarioManager.Instance.valueTest.text = "Second check!!!";
                rotatingBaseHingeJoint.transform.localEulerAngles = originalRotation + rotatingBaseHingeJoint.axis * ((maximumTurns % 1f) * 360f) - rotatingBaseHingeJoint.axis * 180f;
                currentTurn = maximumTurns - 0.0f;
                turnRestant = maximumTurns % 1f;
            } else {
                ScenarioManager.Instance.valueTest.text = "Third check!!!";
                currentTurn = Mathf.Clamp(currentTurn, 0, maximumTurns);
                currentTurn -= turnRestant;
                turnRestant = currentDegrees / 360f;
                currentTurn += turnRestant;
            }

            text0.text = currentTurn.Round(2).ToString();

            lastRotation = currentDegrees;

            if (((currentTurn > turnToFinishAction) && needsToReachTargetClockwise) || ((currentTurn <=  turnToFinishAction) && !needsToReachTargetClockwise)) {
                Finish();
                crankRotatingBaseRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            needsToReachTargetClockwise = currentTurn < turnToFinishAction;
        }

        private void OnRelease(Hand hand, Grabbable grabbable) {
            crankRotatingBaseRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void OnGrab(Hand hand, Grabbable grabbable){
            crankRotatingBaseRigidbody.constraints = RigidbodyConstraints.None;
        }

        public void OnSetScenario() {
            crankRotatingBaseRigidbody.constraints = RigidbodyConstraints.None;
            rotatingBaseHingeJoint.transform.localEulerAngles = originalRotation;
            currentTurn = StartingTurns;
            turnRestant = currentTurn % 1f;
            needsToReachTargetClockwise = currentTurn < turnToFinishAction;
            crankRotatingBaseRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
