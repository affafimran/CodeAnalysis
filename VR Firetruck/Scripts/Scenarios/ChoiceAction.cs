using System.Collections.Generic;
using UnityEngine;

namespace _360Fabriek.Scenarios.Actions {
    public class ChoiceAction : AbstractAction {
        [Header("Choice Action:")]
        [SerializeField] private bool startSelected;
        [SerializeField] private AbstractAction executedAction;
        [SerializeField] private List<ChoiceAction> connectedActions = new List<ChoiceAction>();

        private bool isSelected;

        private void Awake() {
            isSelected = startSelected;
        }

        protected override void InitAditional() {
            if (executedAction) {
                if (connectedActions.Count > 0) {
                    connectedActions.ForEach((action) => { action.Init(); });
                }

                executedAction.Init();
            }
        }

        protected override void OnActivate(ActionArg arg) {
            if (!isSelected) {
                Finish(State.Skipped);
            } else {
                base.OnActivate(arg);

                if (executedAction) {
                    if (connectedActions.Count > 0) {
                        connectedActions.ForEach((action) => { executedAction.FinishEvent.AddListener(action.Select); });
                    }

                    executedAction.FinishEvent.AddListener(OnExecutedActionFinish);
                    executedAction.Activate();
                }
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            executedAction.Deactivate();
        }

        private void OnExecutedActionFinish(ActionArg arg) {
            arg.TriggeredAction.FinishEvent.RemoveListener(OnExecutedActionFinish);

            Finish(arg.Status);
        }

        private void Select(ActionArg arg) {
            arg.TriggeredAction.FinishEvent.RemoveListener(Select);

            isSelected = true;
        }
    }
}