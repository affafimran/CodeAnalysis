using _360Fabriek.Controllers;
using Autohand;
using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class ScenarioObject : MonoBehaviour {
        [SerializeField] private ActivateObjects activateObjects;
        [Tooltip("The locations of these objects will be reset OnScenarioSet\ntarget position and rotation are only used when ScenarioObject.ApplyTargetLocation is called")]
        [SerializeField] private LocationObject[] locationObjects;

        [Header("[OBSOLETE] use activateObjects instead")]
        [System.Obsolete, SerializeField] private bool setActiveState;

        [Tooltip("These objects will set active to the value of setActivateState OnScenarioSet")]
        [System.Obsolete, SerializeField] private GameObject[] gameObjects;

        

        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }

            activateObjects.SetActiveValues();
            SetVisibility(setActiveState);

            foreach (LocationObject locationObject in locationObjects) {
                locationObject.Init();
            }
        }

        private void OnSetScenario(Scenario _) {
            activateObjects.SetActiveValues();
            ResetLocation();

            SetVisibility(setActiveState);
        }

        public void SetVisibility(bool on) {

            foreach (GameObject gameObject in gameObjects) {

                if (gameObject) {

                    gameObject.SetActive(on);
                }
            }
        }

        public void ApplyTargetLocation() {
            foreach (LocationObject locationObject in locationObjects) {
                locationObject.ApplyTargetLocation();
            }
        }

        public void ResetLocation() {
            foreach (LocationObject locationObject in locationObjects) {
                locationObject.Reset();
            }
        }

        [System.Serializable]
        private class ActivateObjects {
            [Header("These objects will be setactive accordingly OnScenarioSet")]
            [SerializeField] private GameObject[] enableObjects; 
             [SerializeField] private GameObject[] disableObjects;

            public void SetActiveValues() {
                SetActiveValueArray(enableObjects, true);
                SetActiveValueArray(disableObjects, false);
            }

            private void SetActiveValueArray(GameObject[] gameObjects, bool on) {
                foreach (GameObject go in gameObjects) {
                    go.SetActive(on);
                    //if (go.GetComponent<Grabbable>() != null)
                    //{
                    //    go.GetComponent<Grabbable>().OnReleaseEvent?.Invoke(null, null);
                        
                    //}
                }
            }
        }

        [System.Serializable]
        private class LocationObject {
            [SerializeField] private GameObject gameObject;

            [SerializeField, Tooltip("This gets called with:\nScenarioObject.ApplyTargetLocation")]
            private Vector3 targetPosition, targetRotation;

            private Transform startParent;
            private Vector3 startPosition;
            private Quaternion startRotation;

            public void Init() {
                startParent = gameObject.transform.parent;
                startPosition = gameObject.transform.localPosition;
                startRotation = gameObject.transform.localRotation;
            }

            public void ApplyTargetLocation() {
                gameObject.transform.localPosition = targetPosition;
                gameObject.transform.localRotation = Quaternion.Euler(targetRotation);
            }

            public void Reset() {
                gameObject.transform.SetParent(startParent);
                gameObject.transform.localPosition = startPosition;
                gameObject.transform.localRotation = startRotation;
                
            }
        }
    }
}