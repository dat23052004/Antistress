using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TransitionManager : Singleton<TransitionManager>
{
    [Header("Transition UI")]
    public GameObject loadingPanel;
    public Image progressBar;
    public TMP_Text loadingText;

    protected override void Initialize()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    public void StartLoading()
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (progressBar) progressBar.fillAmount = 0;
        if (loadingText) loadingText.text = "Loading...";
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (progressBar) progressBar.fillAmount = progress;
        if (loadingText) loadingText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
    }

    public void EndLoading()
    {
        StartCoroutine(HideLoadingRoutine());
    }

    private IEnumerator HideLoadingRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        if (loadingPanel) loadingPanel.SetActive(false);
    }
}

