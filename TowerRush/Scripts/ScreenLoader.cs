using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenLoader : MonoBehaviour
{
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            StartCoroutine(LoadNewScene("MainMenu"));
        }
    }

    IEnumerator LoadNewScene(string sceneName)
    {
        yield return new WaitForSeconds(4);
        float fadetime = Fading.BeginFade(1);
        yield return new WaitForSeconds(fadetime + 1);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
