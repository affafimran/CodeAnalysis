using UnityEngine;

namespace _360Fabriek.UI {
    public class UIPointerHelper : MonoBehaviour {
        [SerializeField] private Transform uiPointer;

        [SerializeField] private LocationData controllerLocalData;
        [SerializeField] private LocationData handTrackingLocalData;

        private bool updatePointer;
        private bool handTrackingActive;
        private bool handTrackingActiveLastFrame;

        private void Start() {
            updatePointer = true;
        }

        private void FixedUpdate() {
            handTrackingActive = OVRInput.IsControllerConnected(OVRInput.Controller.Hands);

            if (handTrackingActive != handTrackingActiveLastFrame) {
                updatePointer = true;
            }

            if (updatePointer) {
                uiPointer.localPosition = GetCurrentLocationData().position;
                uiPointer.localRotation = Quaternion.Euler(GetCurrentLocationData().rotation);
            }

            handTrackingActiveLastFrame = handTrackingActive;
        }

        private LocationData GetCurrentLocationData() {
            LocationData data;

            if (handTrackingActive) {
                data = handTrackingLocalData;
            } else {
                data = controllerLocalData;
            }

            return data;
        }

        [System.Serializable]
        private class LocationData {
            public Vector3 position;
            public Vector3 rotation;
        }
    }
}