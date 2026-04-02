using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class MultiCardStackManager : MonoBehaviour
{
    [Header("Setup")]
    public SwipeCard cardPrefab;
    public RectTransform stackRoot;
    public Canvas rootCanvas;
    public SwipeTutorialLoop tutorialLoop;

    [Header("Data")]
    public List<CardData> allCards = new List<CardData>();

    [Header("Stack")]
    public int visibleCardCount = 3;
    public Vector2 frontCardPos = Vector2.zero;
    public float yOffsetPerCard = -18f;
    public float scaleStep = 0.05f;

    [Header("Effects")]
    public DOTweenAnimation leftEffect;
    public DOTweenAnimation rightEffect;
    public float effectDisableDelay = 0.5f;

    [Header("Loop Data")]
    public bool loopCards = true;

    [Header("Swipe Rule")]
    public int leftSwipeTarget = 3;
    private int leftSwipeCount = 0;

    [Header("Events")]
    public UnityEvent onReachedThreeLeftSwipes;
    public UnityEvent onRightSwipeOnce;

    private readonly List<SwipeCard> activeCards = new List<SwipeCard>();
    private int nextDataIndex = 0;

    private Coroutine leftEffectCoroutine;
    private Coroutine rightEffectCoroutine;

    private void Awake()
    {
        SetEffectActive(leftEffect, false);
        SetEffectActive(rightEffect, false);
    }

    private void Start()
    {
        InitStack();
    }

    public void InitStack()
    {
        ClearStack();

        if (cardPrefab == null || stackRoot == null || rootCanvas == null || allCards.Count == 0)
            return;

        nextDataIndex = 0;
        leftSwipeCount = 0;

        SetEffectActive(leftEffect, false);
        SetEffectActive(rightEffect, false);

        int spawnCount = Mathf.Min(visibleCardCount, allCards.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnNextCardToBack();
        }

        RefreshStackVisual(true);
    }

    private void ClearStack()
    {
        for (int i = activeCards.Count - 1; i >= 0; i--)
        {
            if (activeCards[i] != null)
                Destroy(activeCards[i].gameObject);
        }

        activeCards.Clear();
    }

    private bool HasMoreData()
    {
        if (allCards == null || allCards.Count == 0)
            return false;

        return loopCards || nextDataIndex < allCards.Count;
    }

    private CardData GetNextData()
    {
        if (allCards == null || allCards.Count == 0)
            return null;

        if (!loopCards && nextDataIndex >= allCards.Count)
            return null;

        CardData data = allCards[nextDataIndex];
        nextDataIndex++;

        if (loopCards && nextDataIndex >= allCards.Count)
            nextDataIndex = 0;

        return data;
    }

    private void SpawnNextCardToBack()
    {
        if (!HasMoreData())
            return;

        CardData data = GetNextData();
        if (data == null)
            return;

        SwipeCard card = Instantiate(cardPrefab, stackRoot);
        card.rootCanvas = rootCanvas;
        card.OnSwipeCompleted += HandleSwipeCompleted;

        if (card.cardView != null)
            card.cardView.Bind(data);

        activeCards.Add(card);
    }

    private void HandleSwipeCompleted(SwipeCard swipedCard, SwipeCard.SwipeDirection dir)
    {
        if (dir == SwipeCard.SwipeDirection.Left)
        {
            leftSwipeCount++;
            PlayEffect(leftEffect, ref leftEffectCoroutine);

            if (AudioManager.ins != null)
            {
                AudioManager.ins.PlayswipeLeftSound();
            }

            if (leftSwipeCount >= leftSwipeTarget)
            {
                OnThreeLeftSwipes();
                leftSwipeCount = 0;
            }
        }
        else if (dir == SwipeCard.SwipeDirection.Right)
        {
            PlayEffect(rightEffect, ref rightEffectCoroutine);

            if (AudioManager.ins != null)
            {
                AudioManager.ins.PlayswipeRightSound();
            }

            OnRightSwipe();
        }

        StartCoroutine(HandleAfterSwipe(swipedCard));
    }

    private void PlayEffect(DOTweenAnimation effect, ref Coroutine effectCoroutine)
    {
        if (effect == null)
            return;

        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        SetEffectActive(effect, true);
        effect.DORestart();

        effectCoroutine = StartCoroutine(HideEffectAfterDelay(effect, effectDisableDelay));
    }

    private IEnumerator HideEffectAfterDelay(DOTweenAnimation effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetEffectActive(effect, false);
    }

    private void SetEffectActive(DOTweenAnimation effect, bool isActive)
    {
        if (effect == null)
            return;

        effect.gameObject.SetActive(isActive);
    }

    private void OnThreeLeftSwipes()
    {
        Debug.Log("Đã vuốt trái đủ 3 lần");
        LunaManager.ins.ShowEndCard();
        onReachedThreeLeftSwipes?.Invoke();
    }

    private void OnRightSwipe()
    {
        Debug.Log("Đã vuốt phải 1 lần");
        LunaManager.ins.ShowEndCard();
        onRightSwipeOnce?.Invoke();
    }

    private IEnumerator HandleAfterSwipe(SwipeCard swipedCard)
    {
        yield return new WaitForSeconds(0.05f);

        if (swipedCard != null)
        {
            swipedCard.OnSwipeCompleted -= HandleSwipeCompleted;
            activeCards.Remove(swipedCard);
            Destroy(swipedCard.gameObject);
        }

        if (HasMoreData())
            SpawnNextCardToBack();

        RefreshStackVisual(false);
    }

    private void RefreshStackVisual(bool instant)
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            SwipeCard card = activeCards[i];
            if (card == null) continue;

            bool isFront = i == 0;
            card.SetInteractable(isFront);

            Vector2 targetPos = frontCardPos + new Vector2(0f, yOffsetPerCard * i);
            float scale = 1f - scaleStep * i;
            int sortingOrder = 100 - i;

            card.transform.SetSiblingIndex(activeCards.Count - 1 - i);

            if (instant)
                card.ResetCard(targetPos, Vector3.one * scale, sortingOrder);
            else
                StartCoroutine(AnimateCardToStackSlot(card, targetPos, Vector3.one * scale));

            if (isFront)
            {
                if (tutorialLoop != null)
                {
                    card.tutorialLoop = tutorialLoop;
                }
            }
            else
            {
                card.tutorialLoop = null;
            }
        }

        if (tutorialLoop != null)
        {
            if (activeCards.Count > 0)
            {
                tutorialLoop.StartTutorialLoop();
            }
            else
            {
                tutorialLoop.StopTutorialLoop();
                if (tutorialLoop.tutorialCanvasGroup != null)
                    tutorialLoop.tutorialCanvasGroup.alpha = 0f;
            }
        }
    }

    private IEnumerator AnimateCardToStackSlot(SwipeCard card, Vector2 targetPos, Vector3 targetScale)
    {
        if (card == null || card.cardRect == null)
            yield break;

        RectTransform rect = card.cardRect;
        Vector2 fromPos = rect.anchoredPosition;
        Vector3 fromScale = rect.localScale;
        Quaternion fromRot = rect.localRotation;

        float duration = 0.18f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = EaseOutCubic(t / duration);

            rect.anchoredPosition = Vector2.Lerp(fromPos, targetPos, p);
            rect.localScale = Vector3.Lerp(fromScale, targetScale, p);
            rect.localRotation = Quaternion.Lerp(fromRot, Quaternion.identity, p);

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        rect.localScale = targetScale;
        rect.localRotation = Quaternion.identity;
    }

    private float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}