using UnityEngine;

namespace _360Fabriek {
    public abstract class InteractableAction : AbstractAction {
        [Header("Interactable Action:")]
        [SerializeField] protected Collider interactableCollider = null;
        [SerializeField] private bool disableColliderWhenInactive = true;

        protected virtual void Start() {
            if (disableColliderWhenInactive) {
                interactableCollider.enabled = false;
            }
        }

        protected override void OnActivate(ActionArg arg) {
            interactableCollider.enabled = true;
        }

        protected override void OnDeactivate(ActionArg arg) {
            if (disableColliderWhenInactive) {
                interactableCollider.enabled = false;
            }
        }
    }
}