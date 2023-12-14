using UnityEngine;
using NaughtyAttributes;

namespace _360Fabriek.Scenarios {
    public class IndexedActions : BehaviouralAction {
        [Header("Indexed Actions:")]
        [SerializeField] [ReadOnly] private int index = -1;

        protected override void OnSubActionFinish(ActionArg arg) {
            arg.TriggeredAction.FinishEvent.RemoveListener(OnSubActionFinish);

            base.OnSubActionFinish(arg);

            NextAction();

            if (index == Actions.Count) {
                Finish();
            }
        }

        protected override void OnActivate(ActionArg arg) {
            base.OnActivate(arg);

            index = -1;
            NextAction();
        }

        private void NextAction() {
            index++;

            if (index < Actions.Count) {
                AbstractAction action = Actions[index];

                action.FinishEvent.AddListener(OnSubActionFinish);
                action.Activate();
            }
        }
    }
}
