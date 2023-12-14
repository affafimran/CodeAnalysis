using UnityEngine;

namespace _360Fabriek {
    public class CombinedAction : BehaviouralAction {
        [Header("Combined Actions:")]
        [SerializeField] private bool completeAllActions = true;
        [SerializeField] private bool ignoreSkippedActions = true;

        private bool isLoading = false;
        private int skippedActions = 0;

        protected override void OnSubActionFinish(ActionArg arg) {
            print("Combined => Completed " + arg.TriggeredAction.name + " (" + GetType().Name + ") => " + arg.Status);

            arg.TriggeredAction.FinishEvent.RemoveListener(OnSubActionFinish);

            base.OnSubActionFinish(arg);

            if (isLoading && ignoreSkippedActions && arg.Status == State.Skipped) {
                skippedActions++;
            }

            bool isComplete = completeAllActions ? (FindState(State.Active) == 0) : true;

            if (!isLoading && isComplete) {
                Finish();
            }
        }

        protected override void OnActivate(ActionArg arg) {
            base.OnActivate(arg);

            skippedActions = 0;
            isLoading = true;

            foreach (AbstractAction action in Actions) {
                action.FinishEvent.AddListener(OnSubActionFinish);
                action.Activate();
            }

            isLoading = false;

            if (skippedActions == Actions.Count) {
                Finish(State.Skipped);
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            foreach (AbstractAction action in Actions) {
                action.FinishEvent.RemoveListener(OnSubActionFinish);
                action.Deactivate();
            }
        }
    }
}
