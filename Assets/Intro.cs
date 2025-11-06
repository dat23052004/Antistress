using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [Header("Intro UI")]
    [SerializeField] private CanvasGroup logoCanvas;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private string nextSceneName = "MainScene";

    [Header("Audio")]
    [SerializeField] private AudioSource introAudioSource;
    [SerializeField] private AudioClip startSound;     // âm khi fade in
    [SerializeField] private AudioClip clickSound;

    private bool canSkip = false;
    private bool isSkipping = false;

    private IEnumerator Start()
    {
       if(logoCanvas == null)
        {
            Debug.LogError("Logo CanvasGroup is not assigned in the Inspector.");
            yield break;
        }

#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
        if(startSound != null && introAudioSource != null)
        {
            introAudioSource.PlayOneShot(startSound);
        }
        canSkip = true; 

        float timer = 0f;
        while(timer < displayDuration && !isSkipping)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(FadeLogo(0f, fadeOutDuration));

        yield return StartCoroutine(LoadMainSceneAsync());

    }

    private IEnumerator FadeLogo(float targetAlpha, float duration)
    {
        float startAlpha = logoCanvas.alpha;
        float timer = 0f;

        while (timer < duration)
        {   
            timer += Time.deltaTime;
            logoCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        logoCanvas.alpha = targetAlpha;
    }


    private void Update()
    {
        if (canSkip && !isSkipping && Input.anyKeyDown)
        {
            isSkipping = true;

            if (clickSound != null && introAudioSource != null)
                introAudioSource.PlayOneShot(clickSound);

            StartCoroutine(SkipToMain());
        }
    }
    private IEnumerator SkipToMain()
    {
        yield return StartCoroutine(FadeLogo(0f, 0.5f));
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator LoadMainSceneAsync()
    {
        Debug.Log("loadiing main scene");

        TransitionManager.Ins?.StartLoading();
        yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;

        float timer = 0f;
        float minDisplayTime = 1f; // thời gian hiển thị tối thiểu (dù load nhanh)

        while (async.progress < 0.9f)
        {
            timer += Time.deltaTime;
            float fakeProgress = Mathf.Lerp(0f, async.progress, timer / minDisplayTime);
            TransitionManager.Ins?.UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        // Nếu load quá nhanh thì vẫn cho chạy giả đến 100%
        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            float fakeProgress = Mathf.Lerp(0.9f, 1f, (timer - minDisplayTime / 2f) / (minDisplayTime / 2f));
            TransitionManager.Ins?.UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        TransitionManager.Ins?.UpdateLoadingProgress(1f);
        yield return new WaitForSeconds(0.2f);
        async.allowSceneActivation = true;
    }

}
