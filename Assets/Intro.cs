using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Intro : MonoBehaviour
{
    [Header("Intro UI")]
    [SerializeField] private CanvasGroup logoCanvas;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private string nextSceneName = "MainScene";

    [Header("Audio")]
    [SerializeField] private AudioSource introAudioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip clickSound;

    private bool canSkip;
    private bool isSkipping;

    private IEnumerator Start()
    {
        if (logoCanvas == null)
        {
            Debug.LogError("Logo CanvasGroup is not assigned in the Inspector.");
            yield break;
        }

#if UNITY_EDITOR
        Selection.activeObject = null;
#endif
        if (startSound != null && introAudioSource != null)
            introAudioSource.PlayOneShot(startSound);

        canSkip = true;

        float timer = 0f;
        while (timer < displayDuration && !isSkipping)
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
        if (canSkip && !isSkipping && InputManager.Ins.AnyInputStartedThisFrame())
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
        TransitionManager.Ins?.StartLoading();
        yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;

        float timer = 0f;
        float minDisplayTime = 1f;

        while (async.progress < 0.9f)
        {
            timer += Time.deltaTime;
            float fakeProgress = Mathf.Lerp(0f, async.progress, timer / minDisplayTime);
            TransitionManager.Ins?.UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            float fakeProgress = Mathf.Lerp(0.9f, 1f, (timer - minDisplayTime / 2f) / (minDisplayTime / 2f));
            TransitionManager.Ins?.UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        TransitionManager.Ins?.UpdateLoadingProgress(1f);
        yield return new WaitForSeconds(0.2f);
        TransitionManager.Ins.EndLoading();
        yield return null;
        async.allowSceneActivation = true;
    }
}
