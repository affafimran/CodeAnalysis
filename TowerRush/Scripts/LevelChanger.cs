using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChanger : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        //animator = GetComponent<Animator>;
        StartCoroutine(Delay());
        //yield WaitForSeconds(9f);
    }
    IEnumerator Delay()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {

        }
        else
        {
            yield return new WaitForSeconds(6f);
            animator.SetBool("FadeIn", true);
        }
        
    }

   /* public void FateToLevel(int levelIndex)
    {
        animator.SetTrigger("FadeIn");
    }*/
}
