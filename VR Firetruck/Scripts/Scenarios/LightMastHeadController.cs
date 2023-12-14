using _360Fabriek.Scenarios.Actions;
using UnityEngine;
using UnityEngine.Events;

namespace _360Fabriek.Scenarios {
    public class LightMastHeadController : AbstractAction {
        [Header("Head")]
        [SerializeField] private float rotationSpeed;
        [SerializeField] private Transform horizontalRotater;
        [SerializeField] private DirectionInputs horizontalInput;
        [Space]
        [SerializeField] private int maxVerticalRotation;
        [SerializeField] private Transform verticalRotater;
        [SerializeField] private DirectionInputs verticalInput;

        private int verticalRotationValue;
        private int verticalRotationInput;
        private int horizontalRotationInput;

        protected override void InitAditional() {
            horizontalInput.Init(
                positiveAction: () => RotateDirection(Direction.Horizontal, 1), 
                negativeAction: () => RotateDirection(Direction.Horizontal, -1),
                stopAction: () => RotateDirection(Direction.Horizontal, 0)
            );

            verticalInput.Init(
                positiveAction: () => RotateDirection(Direction.Vertical, 1), 
                negativeAction: () => RotateDirection(Direction.Vertical, -1),
                stopAction: () => RotateDirection(Direction.Vertical, 0)
            );
        }

        protected override void OnActivate(ActionArg _) {
            horizontalInput.Activate();
            verticalInput.Activate();
        }

        protected override void OnDeactivate(ActionArg arg) {
            horizontalInput.Deactivate();
            verticalInput.Deactivate();
        }

        private void LateUpdate() {
            if (Status == State.Active) {
                EditorInputs();

                ClampVerticalValue();

                if (horizontalRotationInput + verticalRotationInput != 0) {
                    RotateHead();
                }
            }
        }

        private void EditorInputs() {
            if (Application.isEditor) {
                GetEditorInput(1, KeyCode.W, Direction.Vertical);
                GetEditorInput(-1, KeyCode.S, Direction.Vertical);
                GetEditorInput(-1, KeyCode.A, Direction.Horizontal);
                GetEditorInput(1, KeyCode.D, Direction.Horizontal);
            }
        }

        private void GetEditorInput(int inputValue, KeyCode key, Direction direction) {
            if (Input.GetKeyDown(key)) {
                RotateDirection(direction, inputValue);
            } else if (Input.GetKeyUp(key)){
                RotateDirection(direction, 0);
            }
        }

        private void ClampVerticalValue() {
            int vertical = verticalRotationValue + verticalRotationInput;

            if (vertical > maxVerticalRotation || vertical < -maxVerticalRotation) {
                verticalRotationInput = 0;
            } else {
                verticalRotationValue = vertical;
            }
        }

        private void RotateDirection(Direction direction, int amount) {
            switch (direction) {
                case Direction.Horizontal:
                horizontalRotationInput = amount;
                TryFinish(horizontalRotater, amount);
                break;

                case Direction.Vertical:
                verticalRotationInput = amount;
                TryFinish(verticalRotater, amount); 
                break;
            }
        }

        private void TryFinish(Transform rotator, int amount) {
            if (rotator && amount != 0) {
                Finish();
            }
        }

        private void RotateHead() {
            float horizontalSpeed = horizontalRotationInput * rotationSpeed * Time.deltaTime;
            float verticalSpeed = verticalRotationInput * rotationSpeed * Time.deltaTime;

            if (horizontalRotater) {
                horizontalRotater.Rotate(new Vector3(0, horizontalSpeed), Space.World);
            }

            if (verticalRotater) {
                verticalRotater.Rotate(new Vector3(0, 0, verticalSpeed), Space.Self);
            }
        }

        public enum Direction {
            Horizontal, Vertical
        }

        [System.Serializable]
        private class DirectionInputs {
            [SerializeField] private VRTouchAction positive;
            [SerializeField] private VRTouchAction negative;

            public void Init(UnityAction positiveAction, UnityAction negativeAction, UnityAction stopAction) {
                TryInit(positive, positiveAction, stopAction);
                TryInit(negative, negativeAction, stopAction);
            }

            public void Activate() {
                TrySetActive(positive);
                TrySetActive(negative);
            }

            public void Deactivate() {
                TryDeactivate(positive);
                TryDeactivate(negative);
            }

            private void TryInit(VRTouchAction action, UnityAction onTouchStart, UnityAction onTouchStop) {
                if (action) {
                    action.Init();

                    action.OnTouchStart.AddListener(onTouchStart);
                    action.OnTouchStop.AddListener(onTouchStop);
                }
            }

            private void TrySetActive(VRTouchAction action) {
                if (action) {
                    action.Activate();
                }
            }

            private void TryDeactivate(VRTouchAction action) {
                if (action) {
                    action.Deactivate();
                }
            }
        }
    }
}