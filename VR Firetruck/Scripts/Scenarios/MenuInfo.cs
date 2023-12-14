using UnityEngine;

namespace _360Fabriek.Menu {
    public class MenuInfo : MonoBehaviour {
        [SerializeField] private GameObject[] sideButtons;

        public void EnableSideButtons(bool on) {
            foreach(GameObject go in sideButtons) {
                go.SetActive(on);
            }
        }
    }
}