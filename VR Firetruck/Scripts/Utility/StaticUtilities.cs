using System;
using UnityEngine;

namespace _360Fabriek.Utility {
    public static class StaticUtilities {
        /// <summary>
        /// returns true if <paramref name="gameObject"/> != null
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="on"></param>
        public static bool TrySetActive(GameObject gameObject, bool on) {
            if (gameObject && gameObject.activeSelf != on) {
                gameObject.SetActive(on);
            }

            return gameObject;
        }

        /// <summary>
        /// returns true if <paramref name="rigidbody"/> != null
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="constraints"></param>
        public static bool TrySetRigidbodyConstraints(Rigidbody rigidbody, RigidbodyConstraints constraints) {
            if (rigidbody) {
                rigidbody.constraints = constraints;
            } else {
                Debug.LogWarning("No Rigidbody assigned");
            }

            return rigidbody;
        }

        internal static void TryDestroy(GameObject gameObject) {
            if (gameObject) {
                GameObject.Destroy(gameObject);
            }
        }

        public static float Round(this float value, int digits) {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }
    }

    [System.Serializable]
    public class AnimationData {
        [SerializeField] private Animator animator;
        [SerializeField] private string key;
        [SerializeField] private int layer;

        /// <summary>
        /// this function is used to change the speed of the animator (min 0), not to change speed key
        /// </summary>
        /// <param name="value"></param>
        public void SetAnimatorSpeed(float value) {
            value = Mathf.Abs(value);
            animator.speed = value;
        }

        public void ApplyValues(float speed, float normalizedStartPosition) {
            if (animator) {
                animator.SetFloat(key, speed);
                animator.Play(0, layer, normalizedStartPosition);
            }
        }
    }
}