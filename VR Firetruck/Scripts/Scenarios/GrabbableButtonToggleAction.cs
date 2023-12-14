using UnityEngine;
using Autohand;

namespace _360Fabriek {
    public class GrabbableButtonToggleAction : GrabbableAction {
        [Header("Grabbable Button Toggle Action:")]
        [SerializeField] private PhysicsGadgetButton button;
        [SerializeField] private bool finishAfterButtonPressed = true;
        [SerializeField] private bool isToggle = true;
        [SerializeField] private bool expectedResult = true;

        private bool isPressed = false;

        protected override void Start() {
            button.OnPressed.AddListener(OnButtonPressed);
            button.OnUnpressed.AddListener(OnButtonUnpressed);

            Grabbable.OnReleaseEvent += OnGrabbableReleased;

            base.Start();
        }

        private void OnButtonPressed() {
            isPressed = true;
        }

        private void OnButtonUnpressed() {
            isPressed = false;
        }

        private void OnGrabbableReleased(Hand hand, Grabbable trigger) {
            if (Status == State.Active && isPressed == expectedResult && finishAfterButtonPressed) {
                if (isToggle) {
                    expectedResult = !expectedResult;
                }

                Finish();
            }
        }
    }
}