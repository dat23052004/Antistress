using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : Singleton<TransitionManager>
{
    [Header("Transition UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI loadingText;

    private float currentProgress = 0f;
    private Coroutine progressRoutine;

    protected override void Initialize()
    {
        DontDestroyOnLoad(this.gameObject);
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    public void StartLoading()
    {
        Debug.Log("load");
        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        currentProgress = 0f;

        if (fillImage != null) fillImage.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0%";
    }

    public void UpdateLoadingProgress(float progress)
    {
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
    }

    public IEnumerator LoadWithMinimumTime(float targetProgress, float minDisplayTime = 1.2f)
    {
        // targetProgress = tiến trình thực (vd async.progress)
        // minDisplayTime = thời gian tối thiểu hiển thị loading

        float timer = 0f;
        float startProgress = currentProgress;

        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            float fakeProgress = Mathf.Lerp(startProgress, targetProgress, timer / minDisplayTime);

            UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        // Đảm bảo hiển thị 100%
        UpdateLoadingProgress(1f);
        yield return new WaitForSeconds(0.2f);
    }
}
