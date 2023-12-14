using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _360Fabriek.Scenarios
{
    public class AnimationControls : MonoBehaviour
    {
        [SerializeField] private Animator animatorDeksel;
        [SerializeField] private Animator animatorPuthaak;


        public void SetTriggerDeksel(string check)
        {
            animatorDeksel.SetInteger(check, 1);
        }

        public void ResetTriggerDeksel(string check)
        {
            animatorDeksel.SetInteger(check, 0);
        }


        public void SetTriggerPuthaak(string check)
        {
            animatorPuthaak.SetInteger(check, 1);
        }

        public void ResetTriggerPuthaak(string check)
        {
            animatorPuthaak.SetInteger(check, 0);
        }
    }

}