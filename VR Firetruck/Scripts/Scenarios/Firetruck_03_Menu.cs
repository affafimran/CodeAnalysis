using UnityEngine;
using _360Fabriek.Menu;
using _360Fabriek.Controllers;
namespace _360Fabriek.Scenarios {
    public class Firetruck_03_Menu : MonoBehaviour {
        [SerializeField] private MenuInfo defaultMenu;
        [SerializeField] private GameObject defaultBanner;

        [SerializeField] private MenuInfo[] menus;
        [SerializeField] private GameObject[] sideButtons;
        [SerializeField] private GameObject[] underBanners;

        private void Start() {
            if (ScenarioManager.Instance) {
                ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            }

            OnSetScenario(null);
        }

        private void OnSetScenario(Scenario scenario) {
            OpenMenu(defaultMenu);
            ShowUnderBanner(defaultBanner);
        }

        public void OpenMenu(MenuInfo selectedMenu) {
            foreach(MenuInfo info in menus) {
                bool isSelectedMenu= info == selectedMenu;
                info.gameObject.SetActive(isSelectedMenu);
            }

            foreach(GameObject go in sideButtons) {
                go.SetActive(false);
            }

            selectedMenu.EnableSideButtons(true);
        }

        public void ShowUnderBanner(GameObject selectedBanner) {
            foreach(GameObject go in underBanners) {
                bool isSelectedBanner = go == selectedBanner;
                go.SetActive(isSelectedBanner);
            }
        }
    }
}