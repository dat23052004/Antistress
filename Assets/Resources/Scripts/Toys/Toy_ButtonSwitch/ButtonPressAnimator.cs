using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class ButtonPressAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    public Transform shadow; 
    public SpriteRenderer shadowSprite; 
    private Animator animator;

    [Header("Shadow Settings")]
    public float pressedScaleY = 0.5f;   // bóng co lại
    public float releasedScaleY = 0.8f;  // bóng dãn ra
    public float tweenDuration = 0.1f;

    private bool isPressed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed) return;
        isPressed = true;

        // Chạy anim nút bấm
        animator.Play("Press", 0, 0f);

        shadow.DOScaleY(pressedScaleY, tweenDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        // Chạy anim bật lại
        animator.Play("Release", 0, 0f);

        shadow.DOScaleY(releasedScaleY, tweenDuration);
    }
}
