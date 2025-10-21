using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Popup : MonoBehaviour
{
    [Header("Refs")]
    public Canvas targetCanvas;                 // 目标 Canvas
    public RectTransform root;                  // 自己（RectTransform）
    public CanvasGroup canvasGroup;             // 用于淡入淡出
    public Image icon;
    public TextMeshProUGUI label;

    [Header("Anim")]
    public float appearScale = 1f;
    public float appearTime = 0.15f;
    public float holdTime = 0.35f;
    public float floatUpDistance = 80f;
    public float fadeOutTime = 0.35f;

    [Header("Offset")]
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0); // ✅ 新增：世界空间上方偏移（1.5 米，可调）

    void Reset()
    {
        root = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// 在世界坐标处生成并播放动画
    /// </summary>
    public void ShowAtWorld(Vector3 worldPos, Sprite sprite, string text, Camera cam = null)
    {
        // 1) 选对 Canvas：优先用 QTE 的那个（与 QTE 一致就不会居中跑偏）
        if (!targetCanvas)
        {
            var qte = UI_QTE.Instance;
            if (qte && qte.panel)
                targetCanvas = qte.panel.GetComponentInParent<Canvas>();
            if (!targetCanvas)
                targetCanvas = FindObjectOfType<Canvas>();
        }

        if (!root) root = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (icon) icon.sprite = sprite;
        if (label) label.text = text;
        if (cam == null) cam = Camera.main;

        // 给一点上方偏移（可在 Inspector 调整）
        // 如果你类里还没有 worldOffset 字段，直接用一个常量也行：var worldOffset = new Vector3(0, 1.2f, 0);
        var worldOffset = new Vector3(0, 1.2f, 0);
        worldPos += worldOffset;

        var canvasRect = targetCanvas.transform as RectTransform;

        // 2) 不同 Canvas 模式分别处理
        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            // ✅ World Space：直接用世界坐标摆放
            root.SetParent(canvasRect, worldPositionStays: false);
            root.position = worldPos;
            root.rotation = Quaternion.identity;
            root.localScale = Vector3.one * 0.001f; // 视你Canvas缩放(常见1/1000)，保持合适视觉尺寸
            root.anchoredPosition3D = root.localPosition; // 保证不被锚点影响
        }
        else
        {
            // ✅ Screen Space（Camera/Overlay）：世界->屏幕->本地
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 localPoint
            );

            root.SetParent(canvasRect, worldPositionStays: false);
            root.anchoredPosition = localPoint;
            root.localScale = Vector3.one;
            root.rotation = Quaternion.identity;
        }
        if (icon != null)
        {
            if (sprite == null)
            {
                icon.enabled = false;                 // ✅ 没图标就不渲染
            }
            else
            {
                icon.enabled = true;
                icon.sprite = sprite;
                // 可选：自动匹配图标尺寸
                // icon.SetNativeSize();
                // icon.preserveAspect = true;
            }
        }

        if (label != null) label.text = text ?? string.Empty;
        // 开始出现/上浮/淡出动画（保持你原来的协程）
        StartCoroutine(PlayAnim());
    }


    private IEnumerator PlayAnim()
    {
        canvasGroup.alpha = 0f;

        // appear
        float t = 0f;
        while (t < appearTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / appearTime);
            root.localScale = Vector3.one * Mathf.Lerp(0f, appearScale, k);
            canvasGroup.alpha = k;
            yield return null;
        }
        root.localScale = Vector3.one * appearScale;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdTime);

        // float up & fade out
        t = 0f;
        Vector2 startPos = root.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, floatUpDistance);
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            root.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
            canvasGroup.alpha = 1f - k;
            yield return null;
        }

        Destroy(gameObject);
    }
}
