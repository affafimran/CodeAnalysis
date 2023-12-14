using _360Fabriek.Controllers;
using _360Fabriek.Utility;
using Autohand;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _360Fabriek.Scenarios
{
    public class ClipboardTouchHolder : MonoBehaviour
    {

        public static ClipboardTouchHolder Instance { get; private set; }

        [SerializeField] private ClipboardTouchNavigation touchNavigator;
        // Start is called before the first frame update
        void Start()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Finger"))
            {
                //touchNavigator.SetButtonPress(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Finger"))
            {
                //touchNavigator.SetButtonPress(false);
                //if (touchNavigator.GetButtonHolder())
                //{
                //    touchNavigator.EnableColliderScenario();
                //}
                //else
                //{
                //    touchNavigator.EnableColliderSteps();
                //}
            }
        }
    }
}