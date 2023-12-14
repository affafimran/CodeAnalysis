using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class ViewManager : MonoBehaviour
{
    GameManager gameManager;
    public GameObject pauseBtn;
    public GameObject MessagePopupGO;
    public GameObject ResumePanel;
    bool isPaused;
    public TextMeshProUGUI ResourcesCountText;



    public static ViewManager Instance;
    
    private void OnEnable()
    {
        GameManager.OnResourcesAdded += UpdateUI;
    }

    private void OnDisable()
    {
        GameManager.OnResourcesAdded -= UpdateUI;
    }


    private void Awake()
    {
        Instance = this;
        gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        UpdateUI();
    }

    public void PauseResume()
    {
        isPaused = !isPaused;
        UpdateUI();
        Time.timeScale = isPaused ? 0 : 1;
    }


    void UpdateUI() {

        ResumePanel.SetActive(isPaused);
        pauseBtn.SetActive(!isPaused);
        ResourcesCountText.text = GameManager.GetResourcesCount().ToString();

    }

    public void ShowMessage(string message)
    {
        MessagePopupGO.SetActive(true);
        MessagePopupGO.GetComponent<MessagePopup>().DisplayMessage(message);
    }

    public void CloseMessage()
    {
        MessagePopupGO.GetComponent<MessagePopup>().ClearMessage();
        MessagePopupGO.SetActive(false);
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu() {
        SceneManager.LoadScene("MainMenu");
    }


    public void ModifyData() {

        LoadPlayerData.Instance.ModifyData();
    }

}