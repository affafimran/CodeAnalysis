using UnityEngine;
using _360Fabriek.Scenarios.Actions;
using _360Fabriek.Controllers;
using UnityEngine.Events;
using UnityEngine.UI;
using _360Fabriek.Scenarios.Listeners;
using _360Fabriek.Audio;

namespace _360Fabriek.Scenarios
{
    public class VRButtonPressAction : AbstractAction
    {
        [SerializeField] private float value;
        [Space]
        [SerializeField] private float valueSecondary;
        [Space]
        [SerializeField] private float step;
        [Space]
        [SerializeField] private float stepSecondary;
        [Space]
        [SerializeField] private VRTouchAction pressTouchable;
        [Space]
        [SerializeField] private NeedleListener pressureGuage;
        [Space]
        [SerializeField] private NeedleListener pressureGuageSecondary;
        [Space]
        [SerializeField] private bool hasTwoPumps;
        [Space]
        [SerializeField] private bool decreaseValue;


        //public Text dummyText;
        private float currentValue = 0;
        private float currentValueSecondary = 0;
        private float pitchValueCurrent = 0;
        private bool isAdding = false;
        private bool isDecreasing = false;

        [Space(5), Header("Audio Settings")]
        [SerializeField] private float pitchStep;
        [SerializeField] private Audio.AudioListener pumpAudio;

        protected override void InitAditional()
        {
            if (!decreaseValue)
            {
                InitMoveTouchable(pressTouchable, () => Move(true), () => Move(false));
            }
            else 
            {
                InitMoveTouchable(pressTouchable, () => Decrease(true), () => Decrease(false));

            }

           //dummyText.text = "At start Gauge 1 Value = " + GaugeControls.Instance.GetHDGauge() + " and Gauge 2 value = " + GaugeControls.Instance.GetLDGauge();
            //if (ScenarioManager.Instance)
            //{
            //    ScenarioManager.Instance.OnScenarioSet.AddListener(OnSetScenario);
            //}
        }


        //private void OnSetScenario(Scenario _)
        //{
        //    currentValue = 0;
        //    currentValueSecondary = 0;
        //    pitchValueCurrent = 0;
        //    pressureGuage.setCurrentValue(0f);
        //    pressureGuageSecondary.setCurrentValue(0f);
            
        //}


        protected override void OnActivate(ActionArg arg)
        {
            pressTouchable.Activate();
            currentValue = GaugeControls.Instance.GetHDGauge();
            currentValueSecondary = GaugeControls.Instance.GetLDGauge();
            pitchValueCurrent=pumpAudio.getPitchValue();
           //dummyText.text = "Gauge 1 Value = " + currentValue + " and Gauge 2 value = "+ currentValueSecondary;
        }

        protected override void OnFinish(ActionArg arg)
        {
           //dummyText.text = "0";
            pressTouchable.Deactivate();
            
            isAdding = false;
        }

        private void InitMoveTouchable(VRTouchAction touch, UnityAction move, UnityAction stop)
        {
            touch.Init();
            touch.OnTouchStart.AddListener(move);
            touch.OnTouchStop.AddListener(stop);
        }

        private void Move(bool canAdd)
        {
            isAdding = canAdd;
        }


        private void Decrease(bool canDecrease)
        {
            isDecreasing = canDecrease;
        }
        // Update is called once per frame
        private void FixedUpdate()
        {
            if (Status == State.Active)
            {
                if (isAdding)
                {
                    TrySetRotationFloat();
                }
                else if(isDecreasing)
                {
                    TryDecreaseRotationFloat();
                }
            }
        }

        private void TrySetRotationFloat()
        {
            if (isAdding && hasTwoPumps)
            {
               
                if (currentValue >= value && currentValueSecondary >= valueSecondary)
                {
                    currentValue = value;
                    currentValueSecondary = valueSecondary;
                    pressureGuage.OnActionValueChange(currentValue);
                    pressureGuageSecondary.OnActionValueChange(currentValueSecondary);
                    pressTouchable.Finish();
                    GaugeControls.Instance.SetHDGauge(currentValue);
                    GaugeControls.Instance.SetLDGauge(currentValueSecondary);
                    Finish();
                }
                else
                {
                    if (currentValue < value)
                    {
                        currentValue += step;
                    }
                    else
                    {
                        currentValue = value;
                    }
                    if (currentValueSecondary < valueSecondary)
                    {
                        currentValueSecondary += stepSecondary;
                    }
                    else
                    {
                        currentValueSecondary = valueSecondary;
                    }

                    pitchValueCurrent += pitchStep;

                    pressureGuage.OnActionValueChange(currentValue);
                    pressureGuageSecondary.OnActionValueChange(currentValueSecondary);
                    pumpAudio.ShiftPitch(pitchValueCurrent);
                }
            }
            else if(isAdding && !hasTwoPumps)
            {
                if (currentValue < value)
                {
                    currentValue += step;
                    pressureGuage.OnActionValueChange(currentValue);
                    pitchValueCurrent += pitchStep;
                    pumpAudio.ShiftPitch(pitchValueCurrent);
                }
                
                else if (currentValue >= value)
                {
                    currentValue = value;
                    pressureGuage.OnActionValueChange(currentValue);
                    pressTouchable.Finish();
                    GaugeControls.Instance.SetHDGauge(currentValue);
                    Finish();
                }
            }
        }

        private void TryDecreaseRotationFloat()
        {
            if (isDecreasing && hasTwoPumps)
            {
                
                if (currentValue <= value && currentValueSecondary <= valueSecondary)
                {
                    currentValue = value;
                    currentValueSecondary = valueSecondary;
                    pressureGuage.OnActionValueChange(currentValue);
                    pressureGuageSecondary.OnActionValueChange(currentValueSecondary);
                    pressTouchable.Finish();
                    GaugeControls.Instance.SetHDGauge(currentValue);
                    GaugeControls.Instance.SetLDGauge(currentValueSecondary);
                    Finish();
                }
                else 
                {
                    if (currentValue > value)
                    {
                        currentValue -= step;
                    }
                    else
                    {
                        currentValue = value;
                    }
                    if (currentValueSecondary > valueSecondary)
                    {
                        currentValueSecondary -= stepSecondary;
                    }
                    else
                    {
                        currentValueSecondary = valueSecondary;
                    }
                    pitchValueCurrent -= pitchStep;

                    pressureGuage.OnActionValueChange(currentValue);
                    pressureGuageSecondary.OnActionValueChange(currentValueSecondary);
                    pumpAudio.ShiftPitch(pitchValueCurrent);
                }
            }
            else if (isDecreasing && !hasTwoPumps)
            {
                if (currentValue > value)
                {
                    currentValue -= step;
                    pressureGuage.OnActionValueChange(currentValue);
                    pitchValueCurrent -= pitchStep;
                    pumpAudio.ShiftPitch(pitchValueCurrent);
                }

                else if (currentValue <= value)
                {
                    currentValue = value;
                    pressureGuage.OnActionValueChange(currentValue);
                    pressTouchable.Finish();
                    GaugeControls.Instance.SetHDGauge(currentValue);
                    Finish();
                }
            }
        }
    }
}

