using _360Fabriek.Controllers;
using _360Fabriek.Utility;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace _360Fabriek.Audio {
    public class AudioListener : MonoBehaviour {
        [Space, Header("Audio Listener:")]
        [SerializeField] private AudioClip audioClip = null;
        [SerializeField] [ReadOnly] private CustomAudioSource audioSource = null;

        [Space, Header("Audio Listener Settings:")]
        [Range(0f, 1f)]
        [SerializeField] public float volume = 1f;
        [SerializeField] public bool playImmidiatly = true;
        [SerializeField] public bool loop = false;
        [SerializeField, Tooltip("KeepPlayingOnScenarioSet")] private bool keepPlayingOnScenarioSet;
        [Tooltip("(OPTIONAL) else uses duration of clip if loop is false")]
        [SerializeField] private float duration;

        [Space, Header("Audio Listener Pitch:")]
        [Range(0.125f, 5f)]
        [SerializeField] private float pitchTarget = 1f;
        [Range(0.125f, 5f)]
        [SerializeField, ReadOnly] private float currentPitch = 1f;
        [Tooltip("In seconds")]
        [SerializeField] private float pitchShiftDuration = 1f;

        private float defaultPitch;
        private Coroutine shiftPitchCoroutine = null;

        private void OnDisable() {
            Stop();
        }

        private void OnDestroy() {
            Stop();
        }

        private void Awake() {
            GetDuration();
        }

        private void Start() {
            defaultPitch = pitchTarget;

            SetAudio(audioClip);

            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(_ => OnSetScenario());
            }
        }

        public float getPitchValue()
        {
            return currentPitch;
        }

        private void OnSetScenario() {
            if (!keepPlayingOnScenarioSet) {
                Stop();
            }
        }

        public void SetAudio(AudioClip audioClip) {
            this.audioClip = audioClip;

            currentPitch = pitchTarget;

            if (audioSource) {
                StaticUtilities.TryDestroy(audioSource.gameObject);
            }

            if (AudioManager.Instance && audioClip) {
                AudioManager.Instance.CreateSfxObject(audioClip, playImmidiatly, out audioSource, loop, volume, pitchTarget, GetDuration());
            }
        }

        private float GetDuration() {
            if (duration < 0) {
                duration = 0f;
            }

            return duration;
        }

        public void Stop() {
            pitchTarget = defaultPitch;

            if (audioSource) {
                audioSource.Stop();
            }
        }

        public void Play() {
            if (audioSource) {
                audioSource.Play();
            }
        }

        public void ShiftPitch(float newPitch) {
            pitchTarget = newPitch;

            if (shiftPitchCoroutine == null) {
                shiftPitchCoroutine = StartCoroutine(ShiftPitchCoroutine());
            }
        }

        public void SetShiftDuration(float duration) {
            pitchShiftDuration = duration;
        }

        private IEnumerator ShiftPitchCoroutine() {
            if (audioSource) {
                float timer = 0;
                
                float normalizedProgress = 0f;
                float startPitch = currentPitch;

                while (normalizedProgress < 1) {
                    timer += Time.deltaTime;

                    normalizedProgress = timer / pitchShiftDuration;
                    normalizedProgress.Round(2);

                    currentPitch = startPitch + (pitchTarget - startPitch) * normalizedProgress;

                    if (pitchTarget > startPitch) {
                        currentPitch = Mathf.Clamp(currentPitch, startPitch, pitchTarget);
                    } else {
                        currentPitch = Mathf.Clamp(currentPitch, pitchTarget, startPitch);
                    }

                    audioSource.SetPitch(currentPitch);

                    yield return null;
                }

                shiftPitchCoroutine = null;
            }
        }
    }
}

























