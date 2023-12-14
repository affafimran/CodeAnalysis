using UnityEngine;

namespace _360Fabriek.Audio {
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private CustomAudioSource audioPrefab;

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        public bool CreateSfxObject(AudioClip clip, bool playImmediately, out CustomAudioSource customSource, bool loop = false, float volume = 1f, float pitchOverride = 1f, float customDuration = 0f) {
            customSource = null;

            if (!clip) {
                return false;
            }

            customSource = Instantiate(audioPrefab, transform);
            customSource.Init(clip, loop, volume, pitchOverride);

            if (playImmediately) {
                customSource.Play(customDuration);
            }
            return customSource;
        }
    }

    public enum AudioGroup {
        UseDefault,
        Master,
        Music,
        Sfx
    }
}