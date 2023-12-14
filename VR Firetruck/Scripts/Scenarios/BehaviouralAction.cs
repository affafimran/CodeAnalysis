using System.Collections.Generic;
using UnityEngine;

namespace _360Fabriek {
    public abstract class BehaviouralAction : AbstractAction {
        [Header("Behavioural Action:")]
        [SerializeField] private bool deactivateActionAfterCompletion = true;
        [SerializeField] private List<AbstractAction> actions = null;

        protected List<AbstractAction> Actions => actions;

        protected override void InitAditional() {
            actions.ForEach(action => action.Init());
        }

        protected virtual void OnSubActionFinish(ActionArg arg) {
            if (deactivateActionAfterCompletion) {
                arg.TriggeredAction.Deactivate();
            }
        }

        protected override void OnActivate(ActionArg arg) {
            if (actions == null || actions.Count == 0) {
                Finish(State.Invalid);
            }
        }

        protected int FindState(State status) {
            int foundAmount = 0;

            actions.ForEach(action => foundAmount += (action.Status == status) ? 1 : 0);

            return foundAmount;
        }
    }
}
