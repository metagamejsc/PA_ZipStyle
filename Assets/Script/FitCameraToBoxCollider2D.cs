using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FitCameraToBoxCollider2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private BoxCollider2D targetBox;

    [Header("Fit Settings")]
    [SerializeField] private float padding = 0.5f;
    [SerializeField] private bool fitOnStart = true;
    [SerializeField] private bool fitEveryFrame = false;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (fitOnStart)
            FitNow();
    }

    private void LateUpdate()
    {
        if (fitEveryFrame)
            FitNow();
    }

    [ContextMenu("Fit Now")]
    public void FitNow()
    {
        if (targetBox == null)
        {
            Debug.LogWarning("Chưa gán BoxCollider2D.");
            return;
        }

        if (cam == null)
            cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("Script này dành cho Orthographic Camera.");
            return;
        }

        Bounds bounds = targetBox.bounds;

        // Đưa camera vào tâm của box, giữ nguyên Z hiện tại
        Vector3 camPos = cam.transform.position;
        camPos.x = bounds.center.x;
        camPos.y = bounds.center.y;
        cam.transform.position = camPos;

        // Kích thước box
        float targetHeight = bounds.size.y + padding * 2f;
        float targetWidth = bounds.size.x + padding * 2f;

        // Tính orthographic size theo cả chiều cao và chiều rộng
        float screenRatio = (float)Screen.width / Screen.height;
        float verticalSize = targetHeight * 0.5f;
        float horizontalSize = (targetWidth * 0.5f) / screenRatio;

        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void SetTarget(BoxCollider2D newTarget)
    {
        targetBox = newTarget;
        FitNow();
    }
}