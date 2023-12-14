using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class EditorHider : MonoBehaviour {
        [SerializeField] private new Renderer renderer;

        private void Start() {
            if (renderer) {
                renderer.enabled = !Application.isEditor;
            }
        }
    }
}