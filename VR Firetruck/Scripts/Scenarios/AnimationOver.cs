using UnityEngine;
using _360Fabriek.Scenarios.Actions;
using _360Fabriek.Controllers;

namespace _360Fabriek.Scenarios.Listeners
{
    public class AnimationOver : MonoBehaviour
    {
       public static AnimationOver Instance { get; set; }

        [SerializeField] private GameObject gameObject;
       


        Animator objectAnimator;

        private void Start()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            objectAnimator = gameObject.GetComponent<Animator>();
            //if (ScenarioManager.Instance)
            //{
            //    ScenarioManager.Instance.OnScenarioSet.AddListener(SetGameObject);
            //}
        }


        public void SetGameObject(Scenario _)
        {
            objectAnimator.SetFloat("Drawer_Open_Value", 0f);
            objectAnimator.Play(0, 21, 0f);
        }
    }

}