using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HeartFloat : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField, Tooltip("本地坐标系上升距离（像素）")]
    private float riseDistance = 40f;
    [SerializeField, Tooltip("动画时长（秒）")]
    private float duration = 1.0f;
    [SerializeField, Tooltip("缓动曲线（0-1）")]
    private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional: 初始缩放弹一下")]
    [SerializeField] private bool popScale = true;
    [SerializeField] private float popScaleMul = 1.2f;
    [SerializeField] private float popDuration = 0.12f;

    private RectTransform rect;
    private CanvasGroup cg;
    private Vector3 startLocalPos;
    private Vector3 startLocalScale;
    private Coroutine running;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        startLocalPos = rect.localPosition;
        startLocalScale = rect.localScale;

        // 初始隐藏
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>从起始位置播放一次浮现动画</summary>
    public void Play()
    {
        if (running != null)
            StopCoroutine(running);
        gameObject.SetActive(true);

        // 重置到起始状态
        rect.localPosition = startLocalPos;
        rect.localScale = startLocalScale;
        cg.alpha = 0f;

        running = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        // 小弹一下
        if (popScale)
        {
            float t0 = 0f;
            while (t0 < popDuration)
            {
                t0 += Time.unscaledDeltaTime; // UI推荐用unscaled
                float k = Mathf.Clamp01(t0 / popDuration);
                float s = Mathf.Lerp(1f, popScaleMul, k);
                rect.localScale = startLocalScale * s;
                cg.alpha = Mathf.Lerp(0f, 1f, k);
                yield return null;
            }
            rect.localScale = startLocalScale * popScaleMul;
        }
        else
        {
            cg.alpha = 1f;
        }

        // 上升+淡出
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = ease.Evaluate(k);

            rect.localPosition = startLocalPos + new Vector3(0f, riseDistance * e, 0f);
            cg.alpha = 1f - k; // 线性淡出（简单好看）

            yield return null;
        }

        // 还原并隐藏
        rect.localPosition = startLocalPos;
        rect.localScale = startLocalScale;
        cg.alpha = 0f;
        gameObject.SetActive(false);
        running = null;
    }
}
