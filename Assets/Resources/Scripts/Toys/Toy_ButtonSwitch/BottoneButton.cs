using UnityEngine;
using UnityEngine.EventSystems;

public class BottoneButton : MonoBehaviour, IPointerDownHandler
{
    public Sprite bottoneOff;
    public Sprite bottoneOn;
    public float frameInterval = 0.05f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = bottoneOff;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(sr.sprite == bottoneOn) sr.sprite = bottoneOff;
        else sr.sprite = bottoneOn;
    }
}
