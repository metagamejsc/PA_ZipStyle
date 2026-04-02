using System.Collections;
using UnityEngine;

public class SimplePulseEffect : MonoBehaviour
{
    public RectTransform target;
    public float duration = 0.2f;
    public float scaleMultiplier = 1.2f;

    private Vector3 baseScale;

    private void Awake()
    {
        if (target == null) target = transform as RectTransform;
        baseScale = target.localScale;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float half = duration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = t / half;
            target.localScale = Vector3.Lerp(baseScale, baseScale * scaleMultiplier, p);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = t / half;
            target.localScale = Vector3.Lerp(baseScale * scaleMultiplier, baseScale, p);
            yield return null;
        }

        target.localScale = baseScale;
    }
}