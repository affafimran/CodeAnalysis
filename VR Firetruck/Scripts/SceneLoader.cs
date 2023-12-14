using _360Fabriek.Scenarios;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using _360Fabriek.Controllers;

namespace _360Fabriek.SceneLoading {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] private Slider loadSlider;

        private const string ScenarioScene = "Scene_Scenario";
        private const string MainMenuScene = "Scene_MainMenu";

        private bool vehicleLogicFinished;
        private AsyncOperation loadSceneOperation;

        private IEnumerator Start() {
            bool useAddressables = false;

            if (ApplicationController.Instance && ApplicationController.Instance.LoadDirection == SceneLoadDirection.ToScenario) {
                while (!VehicleManager.Instance) {
                    yield return null;
                }

                useAddressables = VehicleManager.Instance.UseAddressables;

                if(useAddressables) {
                    while (VehicleManager.Instance.LoadSelectedVehicleHandle.PercentComplete < 1f) {
                        if(VehicleManager.Instance.LoadSelectedVehicleHandle.Status == AsyncOperationStatus.None) {
                            yield return null;
                        }

                        loadSlider.value = VehicleManager.Instance.LoadSelectedVehicleHandle.PercentComplete / 2f;
                        yield return new WaitForSeconds(.1f);
                    }
                }
            }

            if (!vehicleLogicFinished) {
                if (useAddressables) {
                    loadSlider.value = .5f;
                }

                string scene = MainMenuScene;

                if (ApplicationController.Instance && ApplicationController.Instance.LoadDirection == SceneLoadDirection.ToScenario) {
                    scene = useAddressables ? ScenarioScene : VehicleManager.Instance.SelectedVehicle.SceneName;
                }

                vehicleLogicFinished = true;
                loadSceneOperation = SceneManager.LoadSceneAsync(scene);
            }

            while (loadSceneOperation.progress < 1f) {
                loadSlider.value = CalculateSceneloadProgress(useAddressables);
                yield return new WaitForSeconds(.1f);
            }

            loadSlider.value = 1f;
        }

        private float CalculateSceneloadProgress(bool useAddressables) {
            float progress = loadSceneOperation.progress;
            float value = progress;

            if (useAddressables) {
                value = .5f + progress / 2f;
            }

            return value;
        }
    }
}