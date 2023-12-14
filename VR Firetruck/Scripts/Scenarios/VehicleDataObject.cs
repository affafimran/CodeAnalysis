using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _360Fabriek.Vehicles.Data {
    [CreateAssetMenu(fileName = "New Vehicle", menuName = "360Fabriek/New Vehicle")]
    public class VehicleDataObject : ScriptableObject {
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject previewGameObject;
        [SerializeField] private string vehicleName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private AssetReferenceGameObject prefab;
        [SerializeField] private string sceneName;

        public Sprite Icon => icon;
        public GameObject PreviewGameObject => previewGameObject;
        public string VehicleName => vehicleName;
        public string Description => description;
        public AssetReferenceGameObject Prefab => prefab;
        public string SceneName => sceneName;
    }
}