using _360Fabriek.Audio;
using _360Fabriek.Utility;
using System.Collections;
using UnityEngine;

namespace _360Fabriek.Scenarios.Actions {
    public class SirenAction : AbstractAction {
        [SerializeField] private Animator animator;
        [SerializeField] private AudioClip sirenClip;
        [SerializeField] private float maxAudioPlaytime;
        [SerializeField] private float audioFadeOutTime;

        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0f, 10f)] private float pauseBeforeSiren = 5f;

        [SerializeField] private GameObject[] lamps;

        private bool sirensOn;
        private float audioPlayTime;
        private CustomAudioSource customAudio;

        private const string SirenKey = "Sirene";

        private void Start() {
            EnableSiren(false);
        }

        protected override void OnActivate(ActionArg arg) {
            StartCoroutine(SirenLogic());
        }

        private IEnumerator SirenLogic() {

            yield return new WaitForSeconds(this.pauseBeforeSiren);

            customAudio = null;

            if (AudioManager.Instance) {
                AudioManager.Instance.CreateSfxObject(sirenClip, true, out customAudio, true, this.volume);
            }

            EnableSiren(true);

            while (Status == State.Active) {


                if (audioPlayTime < maxAudioPlaytime) {
                    audioPlayTime += Time.deltaTime;
                } else {
                    FadeOutCustomAudioSource();
                }

                yield return new WaitForEndOfFrame();
            }

            EnableSiren(false);
            FadeOutCustomAudioSource();
        }

        private void FadeOutCustomAudioSource() {
            if (customAudio != null) {
                customAudio.FadeOut(audioFadeOutTime);
            }
        }

        private void EnableSiren(bool on) {

            if (sirensOn == on) {
                return;
            }

            sirensOn = on;
            animator.SetBool(SirenKey, on);

            if (!on) {
                for (int i = 0; i < lamps.Length; i++) {
                    StaticUtilities.TrySetActive(lamps[i], false);
                }
            }
        }
    }
}