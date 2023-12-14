using NaughtyAttributes;
using _360Fabriek.Scenarios;
using _360Fabriek.Introduction;
using _360Fabriek.Vehicles.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using _360Fabriek.Controllers;

namespace _360Fabriek.Menu {
    public class VehicleSelector : MonoBehaviour {
        [SerializeField] private Image icon;

        [SerializeField] private TextMeshProUGUI nameField;
        [SerializeField] private TextMeshProUGUI descriptionField;
        
        [SerializeField] private Button select;
        [SerializeField] private Button goLeft;
        [SerializeField] private Button goRight;
        [SerializeField] private GameObject vehicleSelectMenu;

        [Header("Sprite Preview")]
        [SerializeField] private AssetLabelReference spriteLabel;

        [SerializeField, ReadOnly] private List<Sprite> vehicleSprites = new List<Sprite>();

        [Header("Model Preview")]
        [SerializeField] private AssetLabelReference vehicleLabel;

        [SerializeField] private Transform centerPedestal;
        [SerializeField] private Transform leftPedestal;
        [SerializeField] private Transform rightPedestal;

        [SerializeField, ReadOnly] private List<GameObject> vehicleGameObjects = new List<GameObject>();

        public static VehicleSelector Instance { get; private set; }

        private int displayedVehicle = 0;
        private List<VehicleDataObject> vehicles = new List<VehicleDataObject>();

        private GameObject centerVehicleModel;
        private GameObject rightVehicleModel;
        private GameObject leftVehicleMode;

        private AsyncOperationHandle loadSpritesHandle;
        private AsyncOperationHandle loadGameObjectsHandle;

        private bool spritesLoaded;
        private bool gameObjectsLoaded;

        private bool AddressablesAreLoaded =>
            VehicleManager.Instance && VehicleManager.Instance.Vehicles.Count > 0 &&
            (!VehicleManager.Instance.UseAddressables || (gameObjectsLoaded && spritesLoaded));

        private bool CanSelect => IntroductionManager.Instance && !IntroductionManager.Instance.IsIntroducing && AddressablesAreLoaded;

        private void Awake() {
            if (Instance) {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start() {
            StartCoroutine(Initialize());

            LoadPreviewGameObjects();

            if (VehicleManager.Instance.UseAddressables) {
                LoadPreviewSprites();
            }
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        private void Update() {
            if (!CanSelect) {
                return;
            }

            int scroll = (int)(Input.GetAxisRaw("Mouse ScrollWheel") * 10);
            if (scroll != 0) {
                DisplayNextVehicle(scroll);
            }

            if (Input.GetButtonDown("Jump")) {
                SelectDisplayedVehicle();
            }
        }

        public void SetActiveSelectionMenu(bool on) {
            vehicleSelectMenu.gameObject.SetActive(on);
        }

        private IEnumerator Initialize() {
            while (!AddressablesAreLoaded) {
                print(AddressablesAreLoaded);
                yield return null;
            }

            vehicles = VehicleManager.Instance.Vehicles;

            goLeft.onClick.AddListener(() => DisplayNextVehicle(-1));
            goRight.onClick.AddListener(() => DisplayNextVehicle(+1));

            select.onClick.AddListener(SelectDisplayedVehicle);

            goLeft.interactable = vehicles.Count > 1;
            goRight.interactable = vehicles.Count > 1;

            DisplaySelectedVehicle();
        }

        private void DisplayNextVehicle(int direction) {
            if (!CanSelect) {
                return;
            }

            displayedVehicle += direction;

            if (displayedVehicle < 0) {

                displayedVehicle = vehicles.Count - 1;
            } else if (displayedVehicle >= vehicles.Count) {
                displayedVehicle = 0;
            }

            DisplaySelectedVehicle();
        }

        private void DisplaySelectedVehicle() {
            if (VehicleManager.Instance.Vehicles.Count == 0) {
                return;
            }

            VehicleDataObject vehicle = vehicles[displayedVehicle];

            DisplayVehicleModels();

            nameField.text = vehicle.VehicleName;
            descriptionField.text = vehicle.Description;
        }

        private void DisplayVehicleModels() {
            centerVehicleModel = DisplaySpecificVehicleModel(centerPedestal, displayedVehicle, centerVehicleModel);
            //leftVehicleMode = DisplaySpecificVehicleModel(leftPedestal, displayedVehicle - 1, leftVehicleMode);
            //rightVehicleModel = DisplaySpecificVehicleModel(rightPedestal, displayedVehicle + 1, rightVehicleModel);
        }

        private GameObject DisplaySpecificVehicleModel(Transform holder, int index, GameObject previousModel) {
            if (previousModel) {
                Destroy(previousModel);
            }

            if (index < 0) {
                index = vehicles.Count - 1;
            }

            if (index >= vehicles.Count) {
                index = 0;
            }


            GameObject prefab = VehicleManager.Instance.UseAddressables ? TryGetPreviewGameObject(vehicles[index].PreviewGameObject) : vehicles[index].PreviewGameObject;
            GameObject model = Instantiate(prefab, holder);

            return model;
        }

        private void SelectDisplayedVehicle() {
            vehicleSelectMenu.SetActive(false);
            VehicleManager.Instance.SelectVehicle(vehicles[displayedVehicle]);
        }

        private void LoadPreviewSprites() {
            loadSpritesHandle = Addressables.LoadAssetsAsync<Sprite>(
                spriteLabel,
                addressable => {
                    vehicleSprites.Add(addressable);
                });

            loadSpritesHandle.Completed += OnSpritesLoaded;
        }

        private void LoadPreviewGameObjects() {
            if (VehicleManager.Instance.UseAddressables) {
                loadGameObjectsHandle = Addressables.LoadAssetsAsync<GameObject>(
                    vehicleLabel,
                    addressable => {
                        vehicleGameObjects.Add(addressable);
                    });

                loadGameObjectsHandle.Completed += OnVehiclesLoaded;
                return;
            }

            VehicleManager.Instance.Vehicles.ForEach(o => vehicleGameObjects.Add(o.PreviewGameObject));
        }

        private GameObject TryGetPreviewGameObject(GameObject gameObject) {
            if (gameObjectsLoaded && gameObject) {
                for (int i = 0; i < vehicleGameObjects.Count; i++) {
                    if (vehicleGameObjects[i] == gameObject) {
                        return vehicleGameObjects[i];
                    }
                }
            }
            return null;
        }

        private void OnSpritesLoaded(AsyncOperationHandle obj) {
            switch (obj.Status) {
                case AsyncOperationStatus.Succeeded:
                print("Successfully loaded: " + spriteLabel.labelString);
                spritesLoaded = true;
                break;

                case AsyncOperationStatus.Failed:
                print("Failed to load: " + spriteLabel.labelString);
                break;
            }
        }

        private void OnVehiclesLoaded(AsyncOperationHandle obj) {
            switch (obj.Status) {
                case AsyncOperationStatus.Succeeded:
                print("Successfully loaded: " + vehicleLabel.labelString);
                gameObjectsLoaded = true;
                break;

                case AsyncOperationStatus.Failed:
                print("Failed to load: " + vehicleLabel.labelString);
                break;
            }
        }
    }
}