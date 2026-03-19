using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

public class ZipperDragController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Camera cam;
    [SerializeField] private Collider2D zipperHeadCollider;

    [Header("Mask Setup")]
    [SerializeField] private Transform revealMask;
    [SerializeField] private float maskStartY = 2.0f;
    [SerializeField] private float maskEndY = -2.0f;

    [Header("Animation Names")]
    [SpineAnimation] [SerializeField] private string idleAnim = "idle";
    [SpineAnimation] [SerializeField] private string pullAnim = "action";

    [Header("Drag Range In World Space")]
    [SerializeField] private float startY = 2.0f;
    [SerializeField] private float endY = -2.0f;

    [Header("Complete Settings")]
    [SerializeField] private float completeThreshold = 0.95f;
    [SerializeField] private float returnSpeed = 2.5f;

    [Header("Auto Open Settings")]
    [SerializeField] private float autoOpenThreshold = 0.6f;
    [SerializeField] private float autoOpenSpeed = 2.5f;
    [SerializeField] private GameObject tut;

    private TrackEntry pullEntry;
    private Spine.Animation pullAnimation;
    private float animDuration;
    private float progress;
    private bool isDragging;
    private bool isReturning;
    private bool isAutoOpening;
    private bool completed;

    private int activeFingerId = -1;
    private bool usingMouse;

    private Vector3 maskInitialScale;
    private bool hasStartedDragMusic;

    private void Awake()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        if (cam == null)
            cam = Camera.main;

        if (zipperHeadCollider == null)
            zipperHeadCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (skeletonAnimation == null || cam == null || zipperHeadCollider == null || revealMask == null)
        {
            Debug.LogError("Thiếu reference trong Inspector.");
            enabled = false;
            return;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);

        pullAnimation = skeletonAnimation.Skeleton.Data.FindAnimation(pullAnim);
        if (pullAnimation == null)
        {
            Debug.LogError($"Không tìm thấy animation: {pullAnim}");
            enabled = false;
            return;
        }

        animDuration = pullAnimation.Duration;
        maskInitialScale = revealMask.localScale;

        UpdateVisuals();
    }

    private void Update()
    {
        if (completed) return;

        HandleTouchInput();
        HandleMouseInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0 || usingMouse)
            return;

        if (!isDragging)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began && IsPointerOnZipperHead(touch.position))
                {
                    tut.SetActive(false);
                    AudioManager.ins.PlaySoundClick();
                    LunaManager.ins.CheckClickShowEndCard();
                    BeginDrag();
                    activeFingerId = touch.fingerId;
                    UpdateDragFromScreenPosition(touch.position);
                    break;
                }
            }
        }
        else
        {
            bool foundActiveFinger = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId != activeFingerId) continue;

                foundActiveFinger = true;

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    UpdateDragFromScreenPosition(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndDrag();
                    activeFingerId = -1;
                }

                break;
            }

            if (!foundActiveFinger && activeFingerId != -1)
            {
                EndDrag();
                activeFingerId = -1;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (activeFingerId != -1)
            return;

        if (Input.GetMouseButtonDown(0) && IsPointerOnZipperHead(Input.mousePosition))
        {
            tut.SetActive(false);
            AudioManager.ins.PlaySoundClick();
            LunaManager.ins.CheckClickShowEndCard();
            usingMouse = true;
            BeginDrag();
            UpdateDragFromScreenPosition(Input.mousePosition);
        }

        if (isDragging && usingMouse)
        {
            if (Input.GetMouseButton(0))
            {
                UpdateDragFromScreenPosition(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
                usingMouse = false;
            }
        }
    }

    private bool IsPointerOnZipperHead(Vector2 screenPos)
    {
        Vector3 worldPos = ScreenToWorld(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null && hit == zipperHeadCollider;
    }

    private void BeginDrag()
    {
        if (isReturning || isAutoOpening)
            StopAllCoroutines();

        isReturning = false;
        isAutoOpening = false;
        isDragging = true;
        hasStartedDragMusic = false;

        pullEntry = skeletonAnimation.AnimationState.SetAnimation(0, pullAnim, false);
        pullEntry.TimeScale = 0f;
        UpdateVisuals();
    }

    private void UpdateDragFromScreenPosition(Vector2 screenPos)
    {
        Vector3 worldPos = ScreenToWorld(screenPos);
        float raw = Mathf.InverseLerp(startY, endY, worldPos.y);
        progress = Mathf.Clamp01(raw);

        if (!hasStartedDragMusic && progress >= 0.05f)
        {
            AudioManager.ins.PlayMusic();
            hasStartedDragMusic = true;
        }

        UpdateVisuals();
    }

    private void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;
        AudioManager.ins.StopMusic();

        if (progress >= completeThreshold)
        {
            progress = 1f;
            UpdateVisuals();
            completed = true;

            AudioManager.ins.PlayMusic();
            LunaManager.ins.ShowEndCard();
            LunaManager.ins.CheckClickShowEndCard();
        }
        else if (progress >= autoOpenThreshold)
        {
            StartCoroutine(AutoOpenToEnd());
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
    }

    private IEnumerator ReturnToStart()
    {
        isReturning = true;

        while (progress > 0f)
        {
            progress -= Time.deltaTime * returnSpeed;
            progress = Mathf.Clamp01(progress);
            UpdateVisuals();
            yield return null;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
        isReturning = false;
        usingMouse = false;
        activeFingerId = -1;
    }

    private IEnumerator AutoOpenToEnd()
    {
        isAutoOpening = true;

        while (progress < 1f)
        {
            progress += Time.deltaTime * autoOpenSpeed;
            progress = Mathf.Clamp01(progress);
            UpdateVisuals();
            yield return null;
        }

        completed = true;
        isAutoOpening = false;
        usingMouse = false;
        activeFingerId = -1;

        AudioManager.ins.PlayMusic();
        LunaManager.ins.ShowEndCard();
        LunaManager.ins.CheckClickShowEndCard();
    }

    private void UpdateVisuals()
    {
        SetAnimationProgress(progress);
        UpdateMask(progress);
    }

    private void SetAnimationProgress(float normalized)
    {
        if (pullEntry == null) return;

        float time = normalized * animDuration;
        pullEntry.TrackTime = time;

        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.Skeleton.UpdateWorldTransform();
    }

    private void UpdateMask(float normalized)
    {
        /*
        if (revealMask == null) return;

        Vector3 pos = revealMask.position;
        pos.y = Mathf.Lerp(maskStartY, maskEndY, normalized);
        revealMask.position = pos;
        */

        /*Vector3 scale = maskInitialScale;
        revealMask.localScale = scale;*/
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 screen = new Vector3(screenPos.x, screenPos.y, z);
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = 0f;
        return world;
    }
}