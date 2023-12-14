using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios.Actions {

    public class ShutterOpenAction : CombinedAction {
        
        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnOpenShutters.AddListener(OnOpenShutters);
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            base.OnDeactivate(arg);

            ScenarioManager.Instance.OpenShutters();
        }

        private void OnOpenShutters() {
            foreach(AbstractAction action in Actions) {
                if(action.Status == State.Active) {
                    return;
                }
            }

            foreach(AbstractAction action in Actions) {
                action.Disable();
            }

            Disable();
        }
    }
}