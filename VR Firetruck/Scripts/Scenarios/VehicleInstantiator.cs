using UnityEngine;

namespace _360Fabriek {
    public class VehicleInstantiator : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private GameObject instantiateThisObject;
        [SerializeField] private Transform withParent;

        void Start() {
            Instantiate(instantiateThisObject, withParent);
        }
    }
}

