using UnityEngine;
using UnityEngine.SceneManagement;

namespace _360Fabriek.Controllers {
    public class ApplicationController : MonoBehaviour {
        private const string LoadingScene = "Scene_Loading";

        [field: SerializeField] public bool DevMode { get; private set; }
        public static ApplicationController Instance { get; private set; }
        public SceneLoadDirection LoadDirection { get; private set; }

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadToScene(SceneLoadDirection direction) {
            LoadDirection = direction;
            print(direction);

            SceneManager.LoadScene(LoadingScene);
        }
    }

    public enum SceneLoadDirection {
        None,
        ToScenario,
        ToMenu
    }
}