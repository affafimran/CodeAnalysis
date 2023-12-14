using UnityEngine;

namespace _360Fabriek.Scenarios.Listeners {
    public class ArboHatch : MonoBehaviour {
        [SerializeField] private AbstractAction action;

        [Header("Rotation")]
        [SerializeField] private Vector3 finishRotation;
        [SerializeField] private Transform target;

        private void Start() {
            if (!action) {
                action = GetComponent<AbstractAction>();
            }

            if (action) {
                action.DeactivateEvent.AddListener(OnActionDeactivate);
            }
        }

        private void OnActionDeactivate(AbstractAction.ActionArg arg) {
            target.transform.localRotation = Quaternion.Euler(finishRotation);
        }
    }
}