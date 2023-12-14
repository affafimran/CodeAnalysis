using UnityEngine;

namespace _360Fabriek.Scenarios.Actions {
    public class EditorPlaceholderAction : AbstractAction {
        [SerializeField] private bool throwErrorInit;

        protected override void InitAditional() {
            if (throwErrorInit) {
                Debug.LogError($"({nameof(EditorPlaceholderAction)}) in {name}");
            }
        }
    }
}