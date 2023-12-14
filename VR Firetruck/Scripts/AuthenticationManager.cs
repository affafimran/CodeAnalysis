using NaughtyAttributes;
using _360Fabriek.Scenarios;
using _360Fabriek.Introduction;
using _360Fabriek.Vehicles.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using _360Fabriek.Controllers;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace _360Fabriek.Menu
{
    public class AuthenticationManager : MonoBehaviour
    {
        [SerializeField] private GameObject loginMenu;
        [SerializeField] TMP_InputField email;
        [SerializeField] TMP_InputField password;
        [SerializeField] private Button back;
        [SerializeField] private Button login;
        [SerializeField] private string loginURI;
        [SerializeField] private TMP_Text invalidText;


        private bool isChecking = false;
        public static AuthenticationManager Instance { get; private set; }

        public bool onLoginPage { get; private set; }
        // Start is called before the first frame update
        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            onLoginPage = true;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                login.interactable = false;
                invalidText.text = "Kan geen verbinding maken, controleer of de VR headset verbonden is met een WiFi netwerk.";
            }
            else
            {
                invalidText.text = "";
                login.interactable = true;
            }
        }

        private void Start()
        {
            back.onClick.AddListener(StartIntroduction);
            login.onClick.AddListener(LoginUser);
            
        }

        private void StartIntroduction()
        {
            onLoginPage = false;
            loginMenu.SetActive(onLoginPage);

            if (IntroductionManager.Instance)
            {
                IntroductionManager.Instance.SetActiveSelectionMenu(!onLoginPage);
            }
        }
        private void FixedUpdate()
        {
            if (!isChecking)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    login.interactable = false;
                    invalidText.text = "Kan geen verbinding maken, controleer of de VR headset verbonden is met een WiFi netwerk.";
                }
                else
                {
                    invalidText.text = "";
                    isChecking = true;
                    login.interactable = true;
                }
            }
        }
        public void SetActiveSelectionMenu(bool on)
        {
            loginMenu.gameObject.SetActive(on);
        }

        private void LoginUser()
        {
            string json = $"{{ \"email\": \"{email.text}\", \"password\": \"{password.text}\"}}";
         
            StartCoroutine(POSTLoginRequest(loginURI, json));

            //onLoginPage = false;
            //EnableDisableIntroductionEffects();
        }

        IEnumerator POSTLoginRequest(string url, string json)
        {
            login.interactable = false;
            invalidText.text = "";


            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
                req.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                //Send the request then wait here until it returns
                yield return req.SendWebRequest();

                //print(req.result);
                //print(req.responseCode);
                if (req.responseCode == 401)
                {
                    invalidText.text = "Onjuiste inloggegevens, probeer het opnieuw.";
                    //Invoke("clearIndicator", 1.5f);
                }
                else
                {
                    switch (req.result)
                    {



                        case UnityWebRequest.Result.Success:
                            invalidText.text = "Logged in successfully.";
                            onLoginPage = false;
                            PlayerPrefs.SetInt("LoggedIn", 1);
                            EnableDisableIntroductionEffects();
                            login.gameObject.SetActive(false);

                            break;


                        case UnityWebRequest.Result.ProtocolError:
                            ServerResponse response = JsonConvert.DeserializeObject<ServerResponse>(req.downloadHandler.text);
                            //invalidText.text = response.msg;
                            break;

                        case UnityWebRequest.Result.DataProcessingError:
                            break;

                        case UnityWebRequest.Result.ConnectionError:
                        default:

                            invalidText.text = "Kan geen verbinding maken, controleer of de VR headset verbonden is met een WiFi netwerk.";
                            print(req.result);
                            break;

                    }
                }
                login.interactable = true;


            }

        }

        private void clearIndicator()
        {
            invalidText.text = "";
        }
        private void EnableDisableIntroductionEffects()
        {
            loginMenu.SetActive(onLoginPage);

            if (VehicleSelector.Instance)
            {
                VehicleSelector.Instance.SetActiveSelectionMenu(!onLoginPage);
            }

          
        }

        [System.Serializable]
        class ServerResponse
        {
            public string msg { get; set; }
        }
    }
}