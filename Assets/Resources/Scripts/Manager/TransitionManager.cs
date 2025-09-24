using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class TransitionManager : Singleton<TransitionManager>
{
    [Header("Transition Effects")]
    public GameObject transitionPanel;
    public Animator transitionAnimator;

    protected override void Initialize()
    {
        if (transitionPanel != null)
            transitionPanel.SetActive(false);
    }

    public void StartTransition()
    {
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
            if (transitionAnimator != null)
                transitionAnimator.SetTrigger("FadeIn");
        }

        AudioManager.Ins?.PlayTransitionSound();
    }

    public void EndTransition()
    {
        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("FadeOut");

        StartCoroutine(HideTransitionPanel());
    }

    private IEnumerator HideTransitionPanel()
    {
        yield return new WaitForSeconds(0.5f);
        if (transitionPanel != null)
            transitionPanel.SetActive(false);
    }
}

