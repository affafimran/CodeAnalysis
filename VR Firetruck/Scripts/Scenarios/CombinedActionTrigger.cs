using _360Fabriek.Controllers;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _360Fabriek {
    public class CombinedActionTrigger : MonoBehaviour {
        [SerializeField] private UnityEvent OnTrigger = new UnityEvent();
        [SerializeField] private UnityEvent OnSetScenario = new UnityEvent();
        [SerializeField, ReadOnly] public bool isTriggered = false;
        [Header("[OBSOLETE] everything below this line is depricated, don't use it")]
        [SerializeField, System.Obsolete] public List<ParticleSystem> particles;
        [SerializeField, System.Obsolete] public Audio.AudioListener listener;
        [SerializeField, System.Obsolete] public AudioClip clip;

        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnScenarioSet);
            }
        }

        private void OnScenarioSet(Scenario arg0) {
            isTriggered = false;

            if (listener) {
                listener.Stop();
            }

            OnSetScenario?.Invoke();

            particles.ForEach(o => o.Stop());
        }

        public void Trigger() {
            if (!isTriggered) {
                OnTrigger?.Invoke();

                isTriggered = true;

                if (listener) {
                    listener.SetAudio(clip);
                }

                particles.ForEach(o => o.Play());
            }
        }

        public void Untrigger() {
            if (isTriggered) {
                OnScenarioSet(null);
            }
        }
    }
}
