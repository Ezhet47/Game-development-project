using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas targetCanvas;              // 你的 UI Canvas（Screen Space - Camera / Overlay / World Space 都支持）
    public RectTransform bubbleRoot;         // 气泡面板根节点（RectTransform）
    public TextMeshProUGUI textLabel;        // 文本
    public CanvasGroup canvasGroup;          // 淡入淡出、拦截点击（可选）

    [Header("Typing")]
    [Range(1, 120)] public float charsPerSecond = 25f;  // 打字机速度
    public bool clickToCompleteLine = true;             // 打字中按E是否立刻补完
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); // 头顶偏移（世界单位）

    [Header("Input Guard")]
    [SerializeField] private float inputIgnoreDuration = 0.1f; // 开始后忽略输入的时长（秒）
    private float inputAllowedAt = 0f;

    [Header("E-Hint（像 InteractionDetect 一样控制）")]
    public GameObject eHintRoot;             // 小图标 + 文案 的容器（挂在 bubbleRoot 下）
    public Image eHintIcon;                  // 你的按键美术（E 键图）
    public TextMeshProUGUI eHintLabel;       // 提示文案：“按 E 补完 / 下一页 / 结束”
    public bool canPress = false;            // 控制显隐（仿 InteractionDetect.canpress）
    public bool followPlayerFlipX = false;   // 跟随玩家左右翻转（可选）

    // 运行时
    private List<string> lines = new List<string>();
    private int index = -1;
    private Coroutine typingRoutine;
    private bool isShowing = false;
    private bool isTyping = false;

    private Transform followTarget;          // 气泡跟随谁（玩家或NPC）
    private Transform playerTransForFlip;    // 用于左右翻转
    private Player cachedPlayer;             // 锁/解锁移动
    private Camera cam;

    // 单例（方便触发器调用）
    public static DialogueUI Instance { get; private set; }
    public bool IsShowing => isShowing;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (!bubbleRoot) bubbleRoot = GetComponent<RectTransform>();
        cam = Camera.main;

        HideImmediate();
    }

    void LateUpdate()
    {
        if (!isShowing || followTarget == null || targetCanvas == null || bubbleRoot == null)
        {
            ApplyEHintActive();
            return;
        }

        // —— 把“世界位置(头顶偏移)”转换到 Canvas 坐标并定位气泡 ——
        var worldPos = followTarget.position + worldOffset;

        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            bubbleRoot.position = worldPos;
            bubbleRoot.rotation = Quaternion.identity;
        }
        else
        {
            // ScreenSpace-Overlay / ScreenSpace-Camera
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam ? cam : Camera.main, worldPos);
            RectTransform canvasRect = targetCanvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screen,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 local
            );

            // 设父并放置
            if (bubbleRoot.parent != canvasRect)
                bubbleRoot.SetParent(canvasRect, worldPositionStays: false);

            bubbleRoot.anchoredPosition = local;
            bubbleRoot.localRotation = Quaternion.identity;
        }

        // —— 输入（E键）：补完 或 翻页 ——
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.time >= inputAllowedAt && Input.GetKeyDown(KeyCode.E))
            {
                if (isTyping && clickToCompleteLine) { CompleteTypingInstant(); }
                else { Next(); }
            }
        }

        // —— E键提示的显隐与翻转（模仿 InteractionDetect 风格） ——
        ApplyEHintActive();

        if (followPlayerFlipX && playerTransForFlip && eHintRoot)
        {
            var rt = eHintRoot.transform as RectTransform;
            if (rt)
            {
                Vector3 ls = rt.localScale;
                ls.x = Mathf.Sign(-playerTransForFlip.localScale.x) * Mathf.Abs(ls.x);
                rt.localScale = ls;
            }
        }
    }

    // —— 开始对话 ——
    public void StartDialogue(IEnumerable<string> content, Transform follow, Player player)
    {
        lines.Clear();
        lines.AddRange(content);
        followTarget = follow;
        cachedPlayer = player ? player : FindObjectOfType<Player>();
        index = -1;

        // 锁移动
        if (cachedPlayer) cachedPlayer.canMove = false;
        if (cachedPlayer) playerTransForFlip = cachedPlayer.transform;

        // 显示面板
        isShowing = true;
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        bubbleRoot.gameObject.SetActive(true);

        // E 键提示：允许显示，并在第一行开始前显示“按E补完”
        canPress = true;
        UpdateEHintTypingState(true);
        ApplyEHintActive();

        Next(); // 播放第一句
        inputAllowedAt = Time.time + inputIgnoreDuration;
    }

    // —— 下一页 or 结束 ——
    public void Next()
    {
        if (!isShowing) return;

        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }

    // —— 打字机 ——
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        UpdateEHintTypingState(true);      // 正在打字：提示“按E补完”

        textLabel.text = "";
        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        foreach (char c in line)
        {
            textLabel.text += c;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     // 打完：提示“按E下一页/结束”
    }

    // —— 立刻补完当前页 ——
    private void CompleteTypingInstant()
    {
        if (!isTyping) return;
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        textLabel.text = lines[index];
        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     // 补完后：提示“按E下一页/结束”
    }

    // —— 结束对话（解锁移动） ——
    public void EndDialogue()
    {
        isShowing = false;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        bubbleRoot.gameObject.SetActive(false);

        if (cachedPlayer) cachedPlayer.canMove = true;

        // 收尾
        lines.Clear();
        index = -1;
        followTarget = null;
        playerTransForFlip = null;
        cachedPlayer = null;

        // 隐藏 E 提示
        canPress = false;
        ApplyEHintActive();
    }

    private void HideImmediate()
    {
        if (bubbleRoot) bubbleRoot.gameObject.SetActive(false);
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        canPress = false;
        ApplyEHintActive();
    }

    // —— E 提示显隐 & 文案 —— 
    private void ApplyEHintActive()
    {
        if (eHintRoot) eHintRoot.SetActive(canPress);
    }

    private void UpdateEHintTypingState(bool typing)
    {
        if (!eHintLabel) return;

        if (typing)
        {
            eHintLabel.text = "按 E 补完";
        }
        else
        {
            bool isLast = (index >= 0 && index == lines.Count - 1);
            eHintLabel.text = isLast ? "按 E 结束" : "按 E 下一页";
        }
    }
}
