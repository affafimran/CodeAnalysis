using UnityEngine;

namespace _360Fabriek.Utility {
    public class LookAtPlayer : MonoBehaviour {
        [SerializeField] protected GameObject rotator;
        [SerializeField] private TargetType targetType = TargetType.player;
        protected Transform target;
        Vector3 newDir;
        private void OnDisable() {
            TryEnable(rotator, false);
        }

        protected virtual void Update() {
            if (!TryGetTarget()) {
                return;
            }

            if (rotator) 
            {
               rotator.transform.LookAt(target.transform);
            }
        }

        protected bool TryGetTarget() {
            if (!target) {
                switch (targetType) {
                    case TargetType.player:
                    target = Player.Instance.PlayerBody.gameObject.transform;
                    break;
                    case TargetType.camera:
                    target = Camera.main.transform;
                    break;
                }
            }

            return target;
        }

        protected void TryEnable(GameObject go, bool on) {
            if (go && go.activeSelf != on) {
                go.SetActive(on);
            }
        }


       
        private enum TargetType {
            player, camera
        }
    }
}