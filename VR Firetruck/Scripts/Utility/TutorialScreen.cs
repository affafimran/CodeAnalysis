using _360Fabriek.Controllers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _360Fabriek.Tutorial {
    public class TutorialScreen : MonoBehaviour {
        [SerializeField] private Screen handtrackingUI;
        [SerializeField] private Screen controllerUI;

        private bool updateUI;
        private bool isDisplaying;

        private bool handTrackingActive;
        private bool handTrackingActiveLastFrame;

        private void Start() {
            if (!ScenarioManager.Instance) {
                return;
            }

            ScenarioManager.Instance.OnScenarioSet.AddListener(_ => Display(false));

            handtrackingUI.Init();
            controllerUI.Init();
        }

        private void FixedUpdate() {
            handTrackingActive = OVRInput.IsControllerConnected(OVRInput.Controller.Hands);

            if(handTrackingActive != handTrackingActiveLastFrame) {
                updateUI = true;
            }

            if(isDisplaying) {
                if(updateUI) {
                    handtrackingUI.Enable(handTrackingActive);
                    controllerUI.Enable(!handTrackingActive);
                }

                SpriteLogic();
            } else {
                handtrackingUI.Enable(false);
                controllerUI.Enable(false);
            }
         
            handTrackingActiveLastFrame = handTrackingActive;
        }

        private void SpriteLogic() {
            handtrackingUI.SpriteLogic();
            controllerUI.SpriteLogic();
        }

        public void Display(bool on) {
            isDisplaying = on;
            updateUI = true;
        }

        [System.Serializable]
        private class Screen {
            [SerializeField] private GameObject parent;
            [Space]
            [SerializeField] private float spriteSwapTime = 1f;
            [SerializeField] private Image displayImage;
            [SerializeField] private Sprite[] displaySprites;

            [SerializeField] private float spriteSwapTimer;
            private Queue<Sprite>spriteQueue = new Queue<Sprite>();

            public void Init() {
                foreach (Sprite sprite in displaySprites) {
                    spriteQueue.Enqueue(sprite);
                }

                ShowNextSprite();
            }

            public void Enable(bool on) {
                if(parent.activeSelf != on) {
                    parent.SetActive(on);
                }
            }

            public void SpriteLogic() {
                if(displaySprites.Length > 1) {
                    spriteSwapTimer += Time.fixedDeltaTime;

                    if (spriteSwapTimer >= spriteSwapTime) {
                        spriteSwapTimer = 0f;
                        ShowNextSprite();
                    }
                }
            }

            private void ShowNextSprite() {
                if (displayImage && spriteQueue.Count >= 1) {
                    Sprite sprite = spriteQueue.Dequeue();
                    displayImage.sprite = sprite;
                    spriteQueue.Enqueue(sprite);
                }
            }
        }
    }
}