using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _360Fabriek
{

    public class DialLightListener : AbstractListener
    {

        // Serializable
        [Header("Dial Light Action Listener:")]
        [SerializeField] private List<Renderer> lights = new List<Renderer>();

        [Space(15)]
        [SerializeField] private Color lightOffColor = Color.gray;
        [SerializeField] private Color lightOnColor = Color.white;

        private int activatedLights = 0;
        private Coroutine coroutine = null;

        // Init
        protected override void InitAditional()
        {

            lights.ForEach((light) => { light.material.color = this.lightOffColor; });

            this.currentValue = this.startValue;
        }

        // Overrides
        public override void OnActionActivate(AbstractAction.ActionArg arg)
        {

            this.coroutine = StartCoroutine(this.LightDialCoroutine());
        }

        public override void OnActionDeactivate(AbstractAction.ActionArg arg)
        {

            StopCoroutine(this.coroutine);

            this.coroutine = null;
        }

        // Coroutine
        private IEnumerator LightDialCoroutine()
        {

            while (true)
            {

                while (this.currentValue != this.activatedLights)
                {

                    bool direction = (this.currentValue > this.activatedLights);
                    int  index     = (this.activatedLights > 0) ? this.activatedLights % this.lights.Count : 0;

                    if (direction)
                        this.LightOn(this.lights[index]);

                    else
                        this.LightOff(this.lights[index - 1]);

                    yield return new WaitForSeconds(this.wait);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void LightOn(Renderer light)
        {

            this.activatedLights += 1;

            light.material.color = this.lightOnColor;
        }
        
        private void LightOff(Renderer light)
        {

            this.activatedLights -= 1;

            light.material.color = this.lightOffColor;
        }

        // Value change
        public override void OnActionValueChange(float value)
        {

            this.currentValue = value;

            base.OnActionValueChange(value);
        }
    }
}
