using System.Collections;
using UnityEngine;
using Autohand;
using NaughtyAttributes;

namespace _360Fabriek {
    public class RotatableAction : GrabbableAction {
        [Header("Rotatable Action:")]
        [SerializeField] private bool finishAfterRotation = true;
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private GameObject rotationTarget = null;
        [Space]
        [ShowIf(nameof(useBoundaries))] [SerializeField] private bool breakHandConnectionWhenOutOfBounds = false;
        [ShowIf(nameof(useBoundaries))] [SerializeField] private bool resetBoundaryWhenOutOfBounds = false;
        [ShowIf(nameof(useBoundaries))] [SerializeField] private bool resetBoundaryWhenOutOfBoundsAfterwards = false;
        [Header("Rotating:")]
        [ShowIf(nameof(useBoundaries))] [SerializeField] private Vector3 fullRotationsMaxBounds = new Vector3(0f, 0f, 0.5f);
        [ShowIf(nameof(useBoundaries))] [SerializeField] private Vector3 fullRotationsMinBounds = new Vector3(0f, 0f, -.5f);
        [ShowIf(nameof(finishAfterRotation))] [SerializeField] private Vector3 fullRotationsDestination = new Vector3(0f, 0f, 0f);
        [ShowIf(nameof(finishAfterRotation))] [SerializeField] private float destinationRadius = 0.05f;
        [Header("Rotating Debug Values:")]
        [SerializeField] [ReadOnly] private Vector3 fullRotationsRotated = new Vector3(0f, 0f, 0f);
        [SerializeField] [ReadOnly] private Vector3 startAngles;
        [SerializeField] [ReadOnly] private Vector3 originalAngles;
        [Space]
        [ShowIf(nameof(finishAfterRotation))] [SerializeField] [ReadOnly] private float destinationDistance = 0f;
        [ShowIf(nameof(useBoundaries))] [SerializeField] [ReadOnly] private float maxBoundDistance = 0f;
        [ShowIf(nameof(useBoundaries))] [SerializeField] [ReadOnly] private float minBoundDistance = 0f;
        [Space]
        [ShowIf(nameof(useBoundaries))] [SerializeField] [ReadOnly] private float totalBounds = 0f;
        [ShowIf(nameof(useBoundaries))] [SerializeField] [ReadOnly] private float maxBound = 0f;
        [ShowIf(nameof(useBoundaries))] [SerializeField] [ReadOnly] private float minBound = 0f;

        private IEnumerator coroutine;
        private Hand hand;
        private bool turning = false;

        protected override void Start() {
            totalBounds = Vector3.Distance(fullRotationsMaxBounds, fullRotationsMinBounds);
            maxBound = Vector3.Distance(fullRotationsMaxBounds, fullRotationsRotated);
            minBound = Vector3.Distance(fullRotationsMinBounds, fullRotationsRotated);

            if (rotationTarget) {
                startAngles = rotationTarget.transform.localRotation.eulerAngles;
            }

            CalculateBounds();

            Grabbable.OnGrabEvent += OnRotatableGrab;
            Grabbable.OnReleaseEvent += OnRotatableRelease;
            Grabbable.OnJointBreakEvent += OnRotatableRelease;

            base.Start();
        }

        protected override void OnDeactivate(ActionArg arg) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
            }

            if (hand) {
                Grabbable.ForceHandRelease(hand);
            }

            base.OnDeactivate(arg);
        }

        private void OnRotatableGrab(Hand hand, Grabbable trigger) {
            if (Status == State.Active) {
                coroutine = RotationCoroutine();
                this.hand = hand;

                StartCoroutine(coroutine);
            }
        }

        private void OnRotatableRelease(Hand hand, Grabbable trigger) {
            if (Status == State.Active) {
                StopCoroutine(coroutine);

                turning = false;

                if (resetBoundaryWhenOutOfBoundsAfterwards && !IsInBounds()) {
                    CalculateBounds();
                    ResetBounds();
                }

                if (finishAfterRotation && IsOnDestination()) {
                    Finish();
                }
            }
        }

        private IEnumerator RotationCoroutine() {
            turning = true;

            while (true) {
                CalculateBounds();
                fullRotationsRotated += Rotate();

                if (useBoundaries && !IsInBounds()) {
                    ResetBounds();
                }

                ValueChangeEvent?.Invoke(CalculatePercentage());

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void CalculateBounds() {
            if (useBoundaries) {
                maxBoundDistance = Vector3.Distance(fullRotationsMaxBounds, fullRotationsRotated);
                minBoundDistance = Vector3.Distance(fullRotationsMinBounds, fullRotationsRotated);
            }

            if (finishAfterRotation) {
                destinationDistance = Vector3.Distance(fullRotationsDestination, fullRotationsRotated);
            }
        }

        private float CalculatePercentage() {
            bool isNegative = minBound > minBoundDistance;

            return (minBoundDistance - minBound) / ((isNegative ? minBound : maxBound) / 100f);
        }

        private bool IsInBounds() {
            return !(maxBoundDistance > totalBounds || minBoundDistance > totalBounds);
        }

        private bool IsOnDestination() {
            return destinationDistance <= destinationRadius;
        }

        private void ResetBounds() {
            if (resetBoundaryWhenOutOfBounds || (!turning && resetBoundaryWhenOutOfBoundsAfterwards)) {

                if (maxBoundDistance > totalBounds) {
                    ResetBound(fullRotationsMinBounds);
                }

                if (minBoundDistance > totalBounds) {
                    ResetBound(fullRotationsMaxBounds);
                }
            }

            if (hand != null && breakHandConnectionWhenOutOfBounds) {
                Grabbable.ForceHandRelease(hand);
            }
        }

        private void ResetBound(Vector3 boundary) {
            fullRotationsRotated = boundary;
            rotationTarget.transform.localRotation = Quaternion.Euler(startAngles + (boundary * 360f));
        }

        private Vector3 Rotate() {
            Vector3 newAngle = rotationTarget.transform.localRotation.eulerAngles - startAngles;
            Vector3 difAngle = (newAngle - originalAngles) / 360f;

            originalAngles = newAngle;

            return new Vector3(CorrectAxis(difAngle.x), CorrectAxis(difAngle.y), CorrectAxis(difAngle.z));
        }

        private float CorrectAxis(float value) {
            float limit = 0.25f;

            if (value >= limit) {
                value -= 1;
            }

            if (value <= -limit) {
                value += 1;
            }

            return value;
        }
    }
}