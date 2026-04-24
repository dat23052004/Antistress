using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
public class ScrollHandleController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public ScrollRect targetScrollView; // Tham chiếu đến ScrollRect cần điều khiển
    public RectTransform scrollHandle; // Tham chiếu đến thanh kéo (handle) của ScrollRect
    public RectTransform trackContainer; // Tham chiếu đến vùng chứa thanh kéo (handle)
    public CanvasGroup handleCanvasGroup; // Tham chiếu đến CanvasGroup của thanh kéo (handle) để điều khiển độ mờ

    [Header("Settings")]
    public float trackMarginTop = 30f; // Khoảng cách từ trên cùng của vùng chứa đến thanh kéo (handle)
    public float trackMarginBottom = 10f; // Khoảng cách từ dưới cùng của vùng chứa đến thanh kéo (handle)
    public float autoHideDelayScroll = 4f; // fade sau 2s nếu cuộn (OnScroll)
    public float autoHideDelayDrag = 1f;   // fade sau 1s nếu kéo
    public float slideOffset = 60f;        // khoảng cách trượt ngang
    public float animDuration = 0.3f;      // thời gian hiệu ứng
    public Ease showEase = Ease.OutCubic;
    public Ease hideEase = Ease.InCubic;

    private bool isDragging = false;
    private bool isVisible = false;
    private float  handleMinY, handleMaxY;
    private Coroutine hideCoroutine;

    private float dragOffsetY;


    void Start()
    {
        handleMinY = trackMarginBottom + scrollHandle.rect.height / 5;
        handleMaxY = trackContainer.rect.height - trackMarginTop - scrollHandle.rect.height / 1.2f;

        handleCanvasGroup.alpha = 0;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        StopHideTimer();
        ShowHandle();
        scrollHandle.localScale = Vector3.one * 1.2f; // Phóng to thanh kéo khi bắt đầu kéo

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackContainer, eventData.position, eventData.pressEventCamera, out localPoint);
        dragOffsetY = localPoint.y - scrollHandle.anchoredPosition.y;

        targetScrollView.StopMovement();
        targetScrollView.velocity = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging) return; 
        Vector2 localPoint;

        // Chuyển đổi tọa độ từ màn hình sang tọa độ cục bộ của trackContainer
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackContainer, eventData.position, eventData.pressEventCamera, out localPoint);

        float targetY = localPoint.y - dragOffsetY;
        float clampedY = Mathf.Clamp(targetY, handleMinY, handleMaxY);

        scrollHandle.anchoredPosition = new Vector2(scrollHandle.anchoredPosition.x, clampedY);

        float scrollPercent = Mathf.InverseLerp(handleMinY, handleMaxY, clampedY);
        targetScrollView.verticalNormalizedPosition = scrollPercent;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end");
        isDragging = false;
        scrollHandle.localScale = Vector3.one; // Thu nhỏ thanh kéo khi kết thúc kéo
        StartHideTimer(autoHideDelayDrag);
    }

    public void OnScrollValueChanged(Vector2 value)
    {
        if (isDragging) return; // nếu đang kéo thì bỏ qua
        UpdateHandlePosition();

        if (!isVisible) ShowHandle();
        StartHideTimer(autoHideDelayScroll); // 🕒 fade sau 2s không cuộn
    }

    private void UpdateHandlePosition()
    {
        float scrollPercent = targetScrollView.verticalNormalizedPosition;

        float newY = Mathf.Lerp(handleMinY, handleMaxY, scrollPercent);

        scrollHandle.anchoredPosition = new Vector2(scrollHandle.anchoredPosition.x, newY);
    }

    private void ShowHandle()
    {
        if (isVisible) return;
        StopHideTimer();
        handleCanvasGroup.DOKill();
        scrollHandle.DOKill();

        // Dịch handle ra ngoài trước khi trượt vào
        if (handleCanvasGroup.alpha <= 0.01f) 
            scrollHandle.anchoredPosition = new Vector2(slideOffset, scrollHandle.anchoredPosition.y);

        // Trượt từ phải → vị trí thật
        scrollHandle.DOAnchorPosX(0f, animDuration).SetEase(showEase);

        // Fade in
        handleCanvasGroup.DOFade(1f, animDuration).SetEase(Ease.Linear);
    }


    private void StartHideTimer(float delay)
    {
        StopHideTimer();
        hideCoroutine = StartCoroutine(HideAfterDelay(delay));
    }

    private void StopHideTimer()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        handleCanvasGroup.DOKill();
        scrollHandle.DOKill();

        // Trượt ra phải + fade out
        scrollHandle.DOAnchorPosX(slideOffset, animDuration)
            .SetEase(hideEase);
        handleCanvasGroup.DOFade(0f, animDuration)
            .SetEase(Ease.Linear);
    }
}
