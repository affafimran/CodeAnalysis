using _360Fabriek.Controllers;
using UnityEngine;

namespace _360Fabriek.Scenarios.Listeners {
    public class ColorActionListener : MonoBehaviour {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color targetColor;

        private Color defaultColor;
        private Material material;

        private void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }
            
            material = meshRenderer.material;
            defaultColor = material.color;

            ScenarioManager.Instance.OnScenarioSet.AddListener((Scenario _) => ResetColor());
        }
        
        public void ApplyTargetColor() {
            material.color = targetColor;
        }

        public void ResetColor() {
            material.color = defaultColor;
        }
    }
}