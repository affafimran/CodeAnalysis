using _360Fabriek.Controllers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace _360Fabriek {
    public class PercentageAction : AbstractAction {
        [Space(5), Header("Percentage Action:")]
        [SerializeField] private AbstractAction action;

        [Space(5), Header("Percentage Action Events:")]
        [ShowIf(nameof(showEvents))] [SerializeField] private UnityEvent<ActionArg> withinPercentageEvent = new UnityEvent<ActionArg>();

        protected override void InitAditional() {
            if (!ScenarioManager.Instance) {
                Debug.LogError("No the functionality doesn't works");
                return;
            }

            if (action) {
                Debug.LogError("Yes the functionality works");
                action.Init();
            }
        }

        private void OnPercentageActionFinish(ActionArg arg) {
            Finish(arg.Status);
        }

        protected override void OnActivate(ActionArg arg) {
            int randomPercentage = Random.Range(20, 100);
            ScenarioManager.Instance.valueTest.text = "Percentage for error occurance for this action is = " + randomPercentage;
            Debug.LogError("Percentage for error occurance for this action is = " + randomPercentage);

#if UNITY_EDITOR
            if (randomPercentage <= ScenarioManager.Instance.CurrentDifficulty.errorPercentage)
            {
                Debug.LogError("Can't trigger the error");
            }
#endif
                if (randomPercentage <= ScenarioManager.Instance.CurrentDifficulty.errorPercentage) {

                withinPercentageEvent?.Invoke(new ActionArg(this, Status));

                action.FinishEvent.AddListener(OnPercentageActionFinish);
                action.Activate();

            } else {
                Finish(State.Skipped);
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            action.FinishEvent.RemoveListener(OnPercentageActionFinish);
            action.Deactivate();
        }
    }
}
