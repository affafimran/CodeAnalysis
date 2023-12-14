using Autohand;
using UnityEngine;

namespace _360Fabriek {
    public class Player : MonoBehaviour {
        public static Player Instance { get; private set; }
        [SerializeField] private Transform head;
        [field: SerializeField] public Transform LeftIndex { get; private set; }
        [field: SerializeField] public Transform RightIndex { get; private set; }
        [field: SerializeField] public Transform EditorIndex { get; private set; }
        [field: SerializeField] public AutoHandPlayer PlayerBody { get; private set; }

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start() {
            EditorIndex.gameObject.SetActive(Application.isEditor);

            if (!head) {
                return;
            }

            Vector3 headPos = head.localPosition;
            headPos.y = 0;
            transform.position = transform.position - headPos;
        }
    }
}