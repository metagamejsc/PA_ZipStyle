using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HandPointerController : MonoBehaviour
{
    public RectTransform handPointer;
    public float moveDuration = 0.5f;
    public float delayBetweenMoves = 1f;

    public RectTransform[] slots;

    public Color defaultColor = Color.black;      // Màu mặc định
    public Color highlightColor = Color.cyan;   // Màu khi được chọn

    private RectTransform currentSlot; // Slot hiện tại

    void Start()
    {
        handPointer.position = slots[0].position;
        HighlightSlot(slots[0]);
        currentSlot = slots[0];

        StartCoroutine(MoveHandToSlots());
    }

    private IEnumerator MoveHandToSlots()
    {
        handPointer.gameObject.SetActive(true);
        yield return new WaitForSeconds(delayBetweenMoves);

        foreach (var slot in slots)
        {
            ChangeSlotColor(slot);
            yield return MoveHandToPoint(slot);
            yield return new WaitForSeconds(delayBetweenMoves);
        }

        StartCoroutine(MoveHandToSlots());
    }

    IEnumerator MoveHandToPoint(RectTransform target)
    {
        Vector3 startPosition = handPointer.position;
        Vector3 endPosition = target.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            handPointer.position = Vector3.Lerp(startPosition, endPosition, elapsed / moveDuration);
            yield return null;
        }

        handPointer.position = endPosition;
    }

    void ChangeSlotColor(RectTransform newSlot)
    {
        // Reset slot cũ
        if (currentSlot != null)
        {
            currentSlot.localScale=Vector3.one;
            SetSlotColor(currentSlot, defaultColor);
        }

        // Highlight slot mới
        HighlightSlot(newSlot);
        currentSlot = newSlot;
    }

    void HighlightSlot(RectTransform slot)
    {
        slot.localScale=Vector3.one*1.2f;
        SetSlotColor(slot, highlightColor);
    }

    void SetSlotColor(RectTransform slot, Color color)
    {
        Image img = slot.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
    }
}
