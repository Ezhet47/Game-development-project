using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_QTE : MonoBehaviour
{
    public static UI_QTE Instance;

    /*=====================【UI（World Space）】=====================*/
    [Header("UI（World Space）")]
    public CanvasGroup panel;
    public Image barBG;
    public Image fill; // Image Type=Filled / Horizontal

    /*=====================【命中窗口设置】=====================*/
    [Header("命中窗口（无额外美术）")]
    [Range(0.05f, 0.6f)] public float zoneWidth = 0.22f;   // 命中区宽度(0-1)
    [Range(0f, 1f)] public float zoneCenter = 0.72f;       // 命中区中心(0-1)
    public bool randomizeCenter = true;                    // 是否随机命中区中心
    [Range(0f, 1f)] public float centerMin = 0.40f;        // 随机中心最小值
    [Range(0f, 1f)] public float centerMax = 0.85f;        // 随机中心最大值

    /*=====================【视觉反馈】=====================*/
    [Header("视觉反馈")]
    public Color fillNormalColor = Color.white;                          // 普通颜色
    public Color fillHotColor = new Color(0.95f, 0.95f, 0.95f, 1);       // 命中区颜色
    public bool pulseWhenHot = true;                                     // 命中区脉冲
    public float pulseScale = 1.06f;                                     // 脉冲缩放
    public float pulseSpeed = 7f;                                        // 脉冲速度

    /*=====================【输入与指针】=====================*/
    [Header("输入/指针")]
    public KeyCode key = KeyCode.E;          // 触发按键
    public float pointerSpeed = 1.4f;        // 指针往返速度（0-1/秒）

    /*=====================【UI 跟随定位】=====================*/
    [Header("UI 跟随定位")]
    public Vector3 focusOffset = new Vector3(0f, 0.25f, 0f); // UI 在“头顶点”基础上再抬一点
    public float uiFollowLerp = 20f;                         // UI 跟随插值速度

    /*=====================【弹出动画】=====================*/
    [Header("弹出动画")]
    public float popRise = 0.5f;            // 弹出上升高度
    public float popTime = 0.18f;           // 弹出时长
    public float popScaleFrom = 0.6f;       // 弹出起始缩放
    public float popOvershoot = 1.12f;      // 弹出超调缩放
    public float popSettleTime = 0.08f;     // 弹出回落时长

    /*=====================【运行时状态】=====================*/
    Transform focus;            // 跟随的目标（玩家/交互体等）
    Action onSuccess, onFail;   // 回调
    bool active, canInput;      // QTE 是否进行中 / 是否可以输入
    float t01;                  // 指针位置(0-1)
    int dir = 1;                // 指针方向（1→, -1←）

    /*=====================【生命周期】=====================*/
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetUIVisible(false, 0f);
        if (fill)
        {
            fill.color = fillNormalColor;
            fill.fillAmount = 0f;
        }
    }

    /*========================================================
     * QTE 开始：设置目标与UI，显示并弹出
     *========================================================*/
    public void StartSkillCheck(Transform focusTarget, Action success, Action fail)
    {
        if (active) return;

        active = true;
        canInput = false;
        focus = focusTarget;
        onSuccess = success;
        onFail = fail;

        if (randomizeCenter)
            zoneCenter = Mathf.Clamp01(UnityEngine.Random.Range(centerMin, centerMax));

        // UI 初始化
        t01 = 0f; dir = 1;
        if (fill)
        {
            fill.fillAmount = 0f;
            fill.color = fillNormalColor;
            fill.transform.localScale = Vector3.one;
        }

        // 先把 UI 放到起始位置，再做弹出
        Vector3 uiHead = GetTopOfBounds(focus);
        PreparePopIn(uiHead);
        SetUIVisible(true, 0.12f);
        StartCoroutine(CoPopIn());

        // 延迟开启输入，避免刚显示就误触
        Invoke(nameof(EnableInput), 0.08f);
    }

    /*========================================================
     * QTE 结束：隐藏UI，回调
     *========================================================*/
    public void StopSkillCheck(bool success)
    {
        if (!active) return;

        active = false;
        canInput = false;
        CancelInvoke(nameof(EnableInput));

        SetUIVisible(false, 0.12f);
        StartCoroutine(CoFinish(success));
    }

    /*========================================================
     * 主循环：UI 跟随 + 指针/命中/输入
     *========================================================*/
    void Update()
    {
        if (!active) return;

        // 指针往返（0↔1）
        t01 += dir * pointerSpeed * Time.unscaledDeltaTime;
        if (t01 >= 1f) { t01 = 1f; dir = -1; }
        else if (t01 <= 0f) { t01 = 0f; dir = 1; }

        if (fill) fill.fillAmount = t01;

        // UI 跟随“头顶点”
        if (panel && focus)
        {
            Vector3 uiHead = GetTopOfBounds(focus);
            Vector3 target = uiHead + focusOffset;
            panel.transform.position = Vector3.Lerp(
                panel.transform.position,
                target,
                Time.unscaledDeltaTime * uiFollowLerp
            );
            panel.transform.rotation = Quaternion.identity; // 防止被父节点或其他旋转影响
        }

        // 命中窗口视觉反馈
        bool inHot = IsInsideWindow(t01);
        if (fill)
        {
            fill.color = inHot ? fillHotColor : fillNormalColor;
            if (pulseWhenHot)
            {
                float s = inHot
                    ? (1f + (pulseScale - 1f) * 0.5f * (1f + Mathf.Sin(Time.unscaledTime * pulseSpeed)))
                    : 1f;
                fill.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        // 输入判定
        if (canInput && Input.GetKeyDown(key))
            StopSkillCheck(inHot);
    }

    /*========================================================
     * 判定与结束回调
     *========================================================*/
    void EnableInput() => canInput = true;

    bool IsInsideWindow(float v01)
    {
        float min = Mathf.Clamp01(zoneCenter - zoneWidth * 0.5f);
        float max = Mathf.Clamp01(zoneCenter + zoneWidth * 0.5f);
        return v01 >= min && v01 <= max;
    }

    IEnumerator CoFinish(bool success)
    {
        // 给一个最小延时以保证淡出可见
        yield return new WaitForSecondsRealtime(0.06f);
        if (success) onSuccess?.Invoke(); else onFail?.Invoke();
        onSuccess = null; onFail = null; focus = null;
    }

    /*========================================================
     * UI 显隐 & 弹出动画
     *========================================================*/
    void SetUIVisible(bool show, float fadeTime)
    {
        StopAllCoroutines(); // 只保留 UI 协程，当前脚本已无相机协程
        StartCoroutine(CoFade(panel, show ? 1f : 0f, fadeTime));
    }

    IEnumerator CoFade(CanvasGroup cg, float to, float time)
    {
        if (!cg) yield break;
        float from = cg.alpha, x = 0f;
        cg.blocksRaycasts = true; cg.interactable = true;
        while (x < 1f)
        {
            x += Time.unscaledDeltaTime / Mathf.Max(0.0001f, time);
            cg.alpha = Mathf.Lerp(from, to, x);
            yield return null;
        }
        cg.alpha = to;
        if (Mathf.Approximately(to, 0f))
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    void PreparePopIn(Vector3 headWorld)
    {
        if (!panel) return;
        Vector3 spawnPos = (headWorld + focusOffset) - new Vector3(0, popRise, 0);
        panel.transform.position = spawnPos;
        panel.transform.localScale = Vector3.one * popScaleFrom;
    }

    IEnumerator CoPopIn()
    {
        if (!panel) yield break;
        Vector3 p0 = panel.transform.position;
        Vector3 p1 = p0 + new Vector3(0, popRise, 0);

        float x = 0f;
        while (x < 1f)
        {
            x += Time.unscaledDeltaTime / Mathf.Max(0.0001f, popTime);
            float e = 1f - (1f - x) * (1f - x);
            panel.transform.position = Vector3.Lerp(p0, p1, e);
            float s = Mathf.Lerp(popScaleFrom, popOvershoot, e);
            panel.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        float y = 0f;
        Vector3 overs = panel.transform.localScale;
        while (y < 1f)
        {
            y += Time.unscaledDeltaTime / Mathf.Max(0.0001f, popSettleTime);
            float e2 = 1f - Mathf.Cos(y * Mathf.PI * 0.5f);
            float s2 = Mathf.Lerp(overs.x, 1f, e2);
            panel.transform.localScale = new Vector3(s2, s2, 1f);
            yield return null;
        }
        panel.transform.localScale = Vector3.one;
    }

    /*========================================================
     * 辅助：从 Renderer/Collider2D 估算“头顶点/中心点”
     *========================================================*/
    Vector3 GetTopOfBounds(Transform t)
    {
        if (!t) return Vector3.zero;
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr) { var b = sr.bounds; return new Vector3(b.center.x, b.max.y, t.position.z); }
        var col = t.GetComponentInChildren<Collider2D>();
        if (col) { var b = col.bounds; return new Vector3(b.center.x, b.max.y, t.position.z); }
        return t.position + new Vector3(0f, 0.5f, 0f); // 没有渲染器时的兜底
    }

    Vector3 GetCenterOfBounds(Transform t)
    {
        if (!t) return Vector3.zero;
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr) { var b = sr.bounds; return new Vector3(b.center.x, b.center.y, t.position.z); }
        var col = t.GetComponentInChildren<Collider2D>();
        if (col) { var b = col.bounds; return new Vector3(b.center.x, b.center.y, t.position.z); }
        return t.position;
    }
}
