using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : Singleton<TransitionManager>
{
    [Header("Transition UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI loadingText;

    private float currentProgress = 0f;
    private Coroutine progressRoutine;

    private bool simpleMode = false;
    protected override void Initialize()
    {
        DontDestroyOnLoad(this.gameObject);
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    public void StartLoading(bool simple = false)
    {
        
        simpleMode = simple;
        Debug.Log("Start Loading. Simple mode: " + simpleMode);
        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }

        if (loadingPanel != null) loadingPanel.SetActive(true);

        if (progressBar) progressBar.SetActive(!simpleMode);

        currentProgress = 0f;

        if (!simpleMode)
        {
            if (fillImage) fillImage.fillAmount = 0f;
            if (loadingText) loadingText.text = "0%";
        }
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (simpleMode) return;

        progress = Mathf.Clamp01(progress);
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(UpdateProgressSmooth(progress));
    }

    private IEnumerator UpdateProgressSmooth(float progress)
    {

        while (currentProgress < progress)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, progress, Time.deltaTime * 0.8f);

            if (fillImage != null)
                fillImage.fillAmount = currentProgress;

            if (loadingText != null)
                loadingText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";

            yield return null;
        }
    }

    public void EndLoading()
    {
        StartCoroutine(HideLoadingRoutine());
    }

    private IEnumerator HideLoadingRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        if (fillImage != null) fillImage.fillAmount = 1f;
        if (loadingText != null) loadingText.text = "100%";
        yield return new WaitForSeconds(0.2f);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        simpleMode = false;
    }
}
