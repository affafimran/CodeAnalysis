using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _360Fabriek.Audio {

    [RequireComponent(typeof(AudioSource))]
    public class CustomAudioSource : MonoBehaviour {
        [SerializeField] private AudioSource source;

        private bool IsInitialized;
        private Coroutine fadeOutCor;

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Awake() {
            if (CheckSource()) {
                source.Stop();
            }
        }

        private void Start() {
            if (CheckSource() && source.playOnAwake && AudioManager.Instance) {
                if (source.clip) {
                    Init(source.clip, source.loop);
                    Play();
                }
            }
        }

        public void Init(AudioClip clip, bool loop, float volume = 1f, float pitchOverride = 1f) {
            if (CheckSource()) {
                source.loop = loop;
                source.clip = clip;
                source.pitch = pitchOverride;
                source.volume = volume;
                transform.name = clip.name;
                IsInitialized = true;
            }
        }

        public void Play(float customDuration = 0) {
            if (!CheckSource() || source.isPlaying) {
                return;
            }

            if (!source.clip) {
                Destroy(gameObject);
                return;
            }

            source.Play();

            if (!source.loop || customDuration > 0) {
                float duration = customDuration > 0 ? customDuration : source.clip.length;
                Stop(duration);
            }
        }

        public void Stop(float timeToStop = 0f) {
            Destroy(gameObject, timeToStop);
        }

        public void SetPitch(float pitch) {
            source.pitch = pitch;
        }

        public void FadeOut(float fadeOutTime) {
            if (fadeOutCor == null) {
                fadeOutCor = StartCoroutine(FadeOutCor(fadeOutTime));
            }
        }

        private bool CheckSource() {
            if (!source) {
                source = GetComponent<AudioSource>();
            }

            return source;
        }

        private IEnumerator FadeOutCor(float fadeOutTime) {
            float timer = 0;
            float startingVolume = source.volume;

            while (timer <= 1f) {
                source.volume = startingVolume * Mathf.Clamp01(1f - timer);
                timer += Time.deltaTime / fadeOutTime;
                yield return new WaitForSecondsRealtime(0f);
            }

            Stop();
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
            if (IsInitialized) {
                Stop();
            }
        }
    }
}