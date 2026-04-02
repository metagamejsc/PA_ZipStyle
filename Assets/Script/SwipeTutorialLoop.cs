using System.Collections;
using UnityEngine;

public class SwipeTutorialLoop : MonoBehaviour
{
    [Header("Root")]
    public RectTransform tutorialRoot;
    public CanvasGroup tutorialCanvasGroup;

    [Header("Refs")]
    public RectTransform hand;
    public RectTransform arrowLeft;
    public RectTransform arrowRight;
    public RectTransform crossIcon;
    public RectTransform heartIcon;
    public CanvasGroup handCanvasGroup;

    [Header("Placement")]
    public float sideOffset = 130f;
    public float handYOffset = -12f;
    public float arrowOffset = 65f;

    [Header("Timing")]
    public float initialDelay = 0.6f;
    public float moveDuration = 0.45f;
    public float pauseAtLeft = 0.2f;
    public float pauseAtRight = 0.2f;
    public float betweenLoopsDelay = 0.3f;
    public float delayAfterNewCard = 0.8f;

    [Header("PingPong Scale")]
    public float pingPongDuration = 0.12f;
    public float pingPongScale = 1.18f;
    public int pingPongLoops = 2;

    [Header("Hand FX")]
    public float handMinAlpha = 0.35f;

    private Coroutine loopCoroutine;
    private Coroutine resumeCoroutine;

    private Vector3 handBaseScale;
    private Vector3 crossBaseScale;
    private Vector3 heartBaseScale;
    private Vector3 arrowLeftBaseScale;
    private Vector3 arrowRightBaseScale;

    private Vector2 handStartPos;
    private Vector2 leftTargetPos;
    private Vector2 rightTargetPos;

    private void Awake()
    {
        if (tutorialRoot == null)
            tutorialRoot = transform as RectTransform;

        if (tutorialCanvasGroup == null)
            tutorialCanvasGroup = GetComponent<CanvasGroup>();

        if (handCanvasGroup == null && hand != null)
            handCanvasGroup = hand.GetComponent<CanvasGroup>();

        if (hand != null) handBaseScale = hand.localScale;
        if (crossIcon != null) crossBaseScale = crossIcon.localScale;
        if (heartIcon != null) heartBaseScale = heartIcon.localScale;
        if (arrowLeft != null) arrowLeftBaseScale = arrowLeft.localScale;
        if (arrowRight != null) arrowRightBaseScale = arrowRight.localScale;

        CachePositions();
        SetTutorialVisible(false);
    }

    private void OnDisable()
    {
        StopTutorialLoop();
    }

    private void CachePositions()
    {
        if (crossIcon != null)
            crossIcon.anchoredPosition = new Vector2(-sideOffset, 0f);

        if (heartIcon != null)
            heartIcon.anchoredPosition = new Vector2(sideOffset, 0f);

        if (hand != null)
            hand.anchoredPosition = new Vector2(0f, handYOffset);

        if (arrowLeft != null)
            arrowLeft.anchoredPosition = new Vector2(-arrowOffset, 0f);

        if (arrowRight != null)
            arrowRight.anchoredPosition = new Vector2(arrowOffset, 0f);

        handStartPos = hand != null ? hand.anchoredPosition : Vector2.zero;
        leftTargetPos = new Vector2(-sideOffset + 10f, handYOffset);
        rightTargetPos = new Vector2(sideOffset - 10f, handYOffset);
    }

    public void StartTutorialLoop()
    {
        StopTutorialLoop();

        if (tutorialRoot == null)
            return;

        CachePositions();
        ResetVisuals();
        SetTutorialVisible(true);

        loopCoroutine = StartCoroutine(TutorialLoop());
    }

    public void StopTutorialLoop()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        if (resumeCoroutine != null)
        {
            StopCoroutine(resumeCoroutine);
            resumeCoroutine = null;
        }
    }

    public void HideTutorialTemporarily()
    {
        StopTutorialLoop();
        SetTutorialVisible(false);

        resumeCoroutine = StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSeconds(delayAfterNewCard);

        if (tutorialRoot == null)
            yield break;

        CachePositions();
        ResetVisuals();
        SetTutorialVisible(true);

        loopCoroutine = StartCoroutine(TutorialLoop());
    }

    private void SetTutorialVisible(bool visible)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = visible ? 1f : 0f;
            tutorialCanvasGroup.interactable = false;
            tutorialCanvasGroup.blocksRaycasts = false;
        }
        else if (tutorialRoot != null)
        {
            for (int i = 0; i < tutorialRoot.childCount; i++)
                tutorialRoot.GetChild(i).gameObject.SetActive(visible);
        }
    }

    private IEnumerator TutorialLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            ResetLoopStateOnly();

            SetArrowState(true, false);
            yield return MoveHand(handStartPos, leftTargetPos);
            yield return PingPongScale(crossIcon, crossBaseScale);
            yield return new WaitForSeconds(pauseAtLeft);

            SetArrowState(false, true);
            yield return MoveHand(leftTargetPos, rightTargetPos);
            yield return PingPongScale(heartIcon, heartBaseScale);
            yield return new WaitForSeconds(pauseAtRight);

            if (hand != null)
            {
                hand.anchoredPosition = handStartPos;
                hand.localScale = handBaseScale;
            }

            if (handCanvasGroup != null)
                handCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(betweenLoopsDelay);
        }
    }

    private void SetArrowState(bool showLeft, bool showRight)
    {
        if (arrowLeft != null)
            arrowLeft.gameObject.SetActive(showLeft);

        if (arrowRight != null)
            arrowRight.gameObject.SetActive(showRight);
    }

    private IEnumerator MoveHand(Vector2 from, Vector2 to)
    {
        if (hand == null)
            yield break;

        hand.anchoredPosition = from;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float p = EaseInOutCubic(t / moveDuration);

            hand.anchoredPosition = Vector2.Lerp(from, to, p);

            if (handCanvasGroup != null)
            {
                float wave = Mathf.PingPong(p, 0.5f) / 0.5f;
                handCanvasGroup.alpha = Mathf.Lerp(1f, handMinAlpha, wave * 0.6f);
            }

            yield return null;
        }

        hand.anchoredPosition = to;

        if (handCanvasGroup != null)
            handCanvasGroup.alpha = 1f;
    }

    private IEnumerator PingPongScale(RectTransform target, Vector3 baseScale)
    {
        if (target == null)
            yield break;

        Vector3 maxScale = baseScale * pingPongScale;

        for (int i = 0; i < pingPongLoops; i++)
        {
            float t = 0f;
            while (t < pingPongDuration)
            {
                t += Time.deltaTime;
                float p = EaseOutCubic(t / pingPongDuration);
                target.localScale = Vector3.Lerp(baseScale, maxScale, p);
                yield return null;
            }

            t = 0f;
            while (t < pingPongDuration)
            {
                t += Time.deltaTime;
                float p = EaseOutCubic(t / pingPongDuration);
                target.localScale = Vector3.Lerp(maxScale, baseScale, p);
                yield return null;
            }
        }

        target.localScale = baseScale;
    }

    private void ResetVisuals()
    {
        if (hand != null)
        {
            hand.anchoredPosition = handStartPos;
            hand.localScale = handBaseScale;
        }

        if (handCanvasGroup != null)
            handCanvasGroup.alpha = 1f;

        if (crossIcon != null)
        {
            crossIcon.gameObject.SetActive(true);
            crossIcon.localScale = crossBaseScale;
        }

        if (heartIcon != null)
        {
            heartIcon.gameObject.SetActive(true);
            heartIcon.localScale = heartBaseScale;
        }

        if (arrowLeft != null)
        {
            arrowLeft.gameObject.SetActive(true);
            arrowLeft.localScale = arrowLeftBaseScale;
        }

        if (arrowRight != null)
        {
            arrowRight.gameObject.SetActive(false);
            arrowRight.localScale = arrowRightBaseScale;
        }
    }

    private void ResetLoopStateOnly()
    {
        if (hand != null)
        {
            hand.anchoredPosition = handStartPos;
            hand.localScale = handBaseScale;
        }

        if (handCanvasGroup != null)
            handCanvasGroup.alpha = 1f;

        if (crossIcon != null)
            crossIcon.localScale = crossBaseScale;

        if (heartIcon != null)
            heartIcon.localScale = heartBaseScale;

        if (arrowLeft != null)
            arrowLeft.localScale = arrowLeftBaseScale;

        if (arrowRight != null)
            arrowRight.localScale = arrowRightBaseScale;
    }

    private float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private float EaseInOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }
}