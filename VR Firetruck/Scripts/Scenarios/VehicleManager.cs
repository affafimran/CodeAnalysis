using _360Fabriek.Vehicles.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using NaughtyAttributes;

namespace _360Fabriek.Controllers {

    public class VehicleManager : MonoBehaviour {
        [field: SerializeField] public bool UseAddressables { get; private set; }
        [SerializeField, ReadOnly] private VehicleDataObject selectedVehicle;
        [SerializeField] private AssetLabelReference vehicleDataLabel;

        [SerializeField] private List<VehicleDataObject> tempVehicles = new List<VehicleDataObject>();
        
        private List<VehicleDataObject> vehicles = new List<VehicleDataObject>();
        private AsyncOperationHandle loadVehicleDataObjectsHandle;

        public static VehicleManager Instance { get; private set; }
        public AsyncOperationHandle<GameObject> LoadSelectedVehicleHandle { get; private set; }
        public VehicleDataObject SelectedVehicle => selectedVehicle;
        public List<VehicleDataObject> Vehicles { get; private set; } = new List<VehicleDataObject>();

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            LoadVehicleDataObjects();
        }

        private void LoadVehicleDataObjects() {
            if (UseAddressables) {
                loadVehicleDataObjectsHandle = Addressables.LoadAssetsAsync<VehicleDataObject>(
                    vehicleDataLabel,
                    addressable => {
                        vehicles.Add(addressable);
                    });

                loadVehicleDataObjectsHandle.Completed += OnVehicleDataLoaded;

                return;
            }

            Vehicles = tempVehicles;
        }

        private void OnVehicleDataLoaded(AsyncOperationHandle obj) {
            switch (obj.Status) {
                case AsyncOperationStatus.Succeeded:
                print("Successfully loaded: " + vehicleDataLabel.labelString);
                Vehicles = vehicles;
                break;

                case AsyncOperationStatus.Failed:
                print("Failed to load: " + vehicleDataLabel.labelString);
                break;
            }
        }

        public void SelectVehicle(VehicleDataObject vehicle) {
            selectedVehicle = vehicle;

            if (UseAddressables) {
                LoadSelectedVehicleHandle = Addressables.LoadAssetAsync<GameObject>(SelectedVehicle.Prefab);
            }

            Destroy(Player.Instance.gameObject);
            ApplicationController.Instance.LoadToScene(SceneLoadDirection.ToScenario);
        }
    }
}