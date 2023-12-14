using UnityEngine;

namespace _360Fabriek.Scenarios.Listeners {
    public class LocationListener : MonoBehaviour {
        [SerializeField] private Transform targetTranform;

        [SerializeField] private OverrideSetting overridePosition;
        [SerializeField] private Vector3 position;

        [SerializeField] private OverrideSetting overrideRotation;
        [SerializeField] private Vector3 rotation;

        public void Trigger() {
            if (!targetTranform) {
                return;
            }

            TryApplyPosition();
            TryApplyRotation();
        }

        private void TryApplyPosition() {
            switch (overridePosition) {
                case OverrideSetting.OverrideGlobal:
                targetTranform.position = position;
                break;
                case OverrideSetting.OverrideLocal:
                targetTranform.localPosition = position;
                break;
            }
        }

        private void TryApplyRotation() {
            switch (overrideRotation) {
                case OverrideSetting.OverrideGlobal:
                targetTranform.rotation = Quaternion.Euler(rotation);
                break;
                case OverrideSetting.OverrideLocal:
                targetTranform.localRotation = Quaternion.Euler(rotation);
                break;
            }
        }

        private enum OverrideSetting {
            DontOverride,
            OverrideGlobal,
            OverrideLocal
        }
    }
}