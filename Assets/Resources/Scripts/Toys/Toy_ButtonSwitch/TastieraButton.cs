using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TastieraButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite[] frames;
    public float frameInterval = 0.05f;

    private SpriteRenderer sr;
    private bool isPressed = false;
    private Coroutine playRoutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = frames[0];
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed) return;
        isPressed = true;
        
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlayFramesForward());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlayFramesBackward());
    }

    IEnumerator PlayFramesForward()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameInterval);
        }
    }

    IEnumerator PlayFramesBackward()
    {
        for (int i = frames.Length - 1; i >= 0; i--)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameInterval);
        }
    }

}
