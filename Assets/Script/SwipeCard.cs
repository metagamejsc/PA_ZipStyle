using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SwipeDirection
    {
        Left,
        Right
    }

    [Header("Refs")]
    public RectTransform cardRect;
    public Canvas rootCanvas;
    public CanvasGroup infoGroup;
    public CardView cardView;
    public SwipeTutorialLoop tutorialLoop;

    [Header("Swipe")]
    public float maxRotation = 18f;
    public float returnDuration = 0.18f;
    public float swipeOutDuration = 0.28f;
    public bool useHalfCardWidthAsThreshold = true;
    public float manualSwipeThreshold = 220f;

    [Header("Tutorial Hide")]
    public float hideTutorialDragThreshold = 15f;

    public Action<SwipeCard, SwipeDirection> OnSwipeCompleted;

    private Vector2 startAnchoredPos;
    private Quaternion startRotation;

    private bool isDragging;
    private bool isAnimating;
    private bool hasTriggeredSwipeOut;
    private bool hasHiddenTutorialByDrag;

    private Vector2 dragStartScreenPos;
    private float currentDragDeltaX;

    private void Awake()
    {
        if (cardRect == null) cardRect = GetComponent<RectTransform>();
        if (cardView == null) cardView = GetComponent<CardView>();
        if (infoGroup == null && cardView != null) infoGroup = cardView.infoGroup;

        cardRect.pivot = new Vector2(0.5f, 0f);

        startAnchoredPos = cardRect.anchoredPosition;
        startRotation = Quaternion.identity;
    }

    public void ResetCard(Vector2 anchoredPos, Vector3 scale, int sortingOrder = 0)
    {
        StopAllCoroutines();

        startAnchoredPos = anchoredPos;
        startRotation = Quaternion.identity;

        cardRect.anchoredPosition = anchoredPos;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = scale;

        if (infoGroup != null)
            infoGroup.alpha = 1f;

        currentDragDeltaX = 0f;
        isDragging = false;
        isAnimating = false;
        hasTriggeredSwipeOut = false;
        hasHiddenTutorialByDrag = false;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = sortingOrder;
    }

    public void SetInteractable(bool value)
    {
        enabled = value;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating || hasTriggeredSwipeOut) return;

        isDragging = true;
        dragStartScreenPos = eventData.position;
        currentDragDeltaX = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isAnimating || hasTriggeredSwipeOut) return;

        currentDragDeltaX = eventData.position.x - dragStartScreenPos.x;

        if (!hasHiddenTutorialByDrag && Mathf.Abs(currentDragDeltaX) >= hideTutorialDragThreshold)
        {
            hasHiddenTutorialByDrag = true;
            if (tutorialLoop != null)
                tutorialLoop.HideTutorialTemporarily();
        }

        float threshold = GetSwipeThreshold();
        float normalizedX = Mathf.Clamp(currentDragDeltaX / threshold, -1f, 1f);

        float rotZ = -normalizedX * maxRotation;
        cardRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);

        if (infoGroup != null)
            infoGroup.alpha = 1f - Mathf.Abs(normalizedX) * 0.65f;

        if (Mathf.Abs(currentDragDeltaX) >= threshold)
        {
            hasTriggeredSwipeOut = true;
            isDragging = false;

            SwipeDirection dir = currentDragDeltaX > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            StartCoroutine(AnimateSwipeOut(dir));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (hasTriggeredSwipeOut || isAnimating) return;
        if (!isDragging) return;

        isDragging = false;
        StartCoroutine(AnimateReturn());
    }

    private float GetSwipeThreshold()
    {
        if (!useHalfCardWidthAsThreshold)
            return manualSwipeThreshold;

        return cardRect.rect.width * 0.5f;
    }

    private IEnumerator AnimateReturn()
    {
        isAnimating = true;

        Quaternion fromRot = cardRect.localRotation;
        float fromAlpha = infoGroup != null ? infoGroup.alpha : 1f;

        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float p = EaseOutCubic(t / returnDuration);

            cardRect.localRotation = Quaternion.Lerp(fromRot, startRotation, p);

            if (infoGroup != null)
                infoGroup.alpha = Mathf.Lerp(fromAlpha, 1f, p);

            yield return null;
        }

        cardRect.localRotation = startRotation;
        if (infoGroup != null)
            infoGroup.alpha = 1f;

        currentDragDeltaX = 0f;
        isAnimating = false;
    }

    private IEnumerator AnimateSwipeOut(SwipeDirection dir)
    {
        isAnimating = true;

        Vector2 fromPos = cardRect.anchoredPosition;
        Quaternion fromRot = cardRect.localRotation;
        float fromAlpha = infoGroup != null ? infoGroup.alpha : 1f;

        float canvasWidth = ((RectTransform)rootCanvas.transform).rect.width;
        float targetX = dir == SwipeDirection.Right ? canvasWidth * 1.35f : -canvasWidth * 1.35f;
        Vector2 targetPos = new Vector2(targetX, fromPos.y + 60f);

        float extraRot = dir == SwipeDirection.Right ? -35f : 35f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, extraRot);

        float t = 0f;
        while (t < swipeOutDuration)
        {
            t += Time.deltaTime;
            float p = EaseInCubic(t / swipeOutDuration);

            cardRect.anchoredPosition = Vector2.Lerp(fromPos, targetPos, p);
            cardRect.localRotation = Quaternion.Lerp(fromRot, targetRot, p);

            if (infoGroup != null)
                infoGroup.alpha = Mathf.Lerp(fromAlpha, 0f, p);

            yield return null;
        }

        OnSwipeCompleted?.Invoke(this, dir);

        currentDragDeltaX = 0f;
        isAnimating = false;
    }

    private float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private float EaseInCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * x;
    }
}