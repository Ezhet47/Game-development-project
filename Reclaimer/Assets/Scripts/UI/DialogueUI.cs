using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas targetCanvas;              // UI Canvas（Screen Space - Camera / Overlay / World Space）
    public RectTransform bubbleRoot;         // 气泡面板根节点
    public TextMeshProUGUI textLabel;        // 文本
    public CanvasGroup canvasGroup;          // 淡入淡出、拦截点击

    [Header("Typing")]
    [Range(1, 120)] public float charsPerSecond = 25f;  // 打字机速度
    public bool clickToCompleteLine = true;             // 打字中按E是否立刻补完
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); // 头顶偏移（世界单位）

    [Header("E-Hint（像 InteractionDetect 一样控制）")]
    public GameObject eHintRoot;             // 小图标 + 文案 容器
    public Image eHintIcon;                  // E 键美术
    public TextMeshProUGUI eHintLabel;       // 提示文案：“按 E 补完 / 下一页 / 结束”
    public bool canPress = false;            // 控制显隐
    public bool followPlayerFlipX = false;   // 跟随玩家左右翻转（可选）

    [Header("Input Guard")]
    [SerializeField] private float inputIgnoreDuration = 0.1f; // 开始后忽略输入的时长
    private float inputAllowedAt = 0f;

    // === 说话 blip（Undertale 风） ===
    [Header("Voice / Blip Settings")]
    public AudioSource voiceSource;          // 建议挂在 DialogueUI 节点上
    public AudioClip[] blipClips;            // 一组极短的“嘟”音（可 1~4 个）
    [Tooltip("blip 触发间隔（秒），防止过密；留空则按速度自动算")]
    public float blipInterval = -1f;         // <=0 时自动：0.6 * (1/charsPerSecond)
    [Range(0f, 1f)] public float blipVolume = 0.9f;
    [Tooltip("避免对空格与常见标点触发 blip")]
    public string muteChars = " \n\r\t.,;:!?，。；：！？…";
    [Tooltip("音高随机范围")]
    public Vector2 pitchRandom = new Vector2(0.96f, 1.04f);
    [Tooltip("每 N 个字符才触发一次 blip（1 = 每个合格字符都触发）")]
    public int blipEveryNChars = 1;
    [Header("Blip Audio Pool")]
    [SerializeField] private int blipPoolSize = 6;   // 同时最多重叠6个blip
    private AudioSource[] blipPool;
    private int blipPoolCursor = 0;

    // 运行时
    private List<string> lines = new List<string>();
    private int index = -1;
    private Coroutine typingRoutine;
    private bool isShowing = false;
    private bool isTyping = false;
    private float nextBlipTime = 0f;
    private int typedCountOnLine = 0;

    private Transform followTarget;          // 气泡跟随谁（玩家或NPC）
    private Transform playerTransForFlip;    // 用于左右翻转
    private Player cachedPlayer;             // 锁/解锁移动
    private Camera cam;

    // 单例
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

        // 如果没挂 AudioSource，自动加一个（静音 3D 选项都可默认）
        if (!voiceSource) voiceSource = GetComponent<AudioSource>();
        if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f; // UI 声音一般用 2D

        HideImmediate();
        blipPool = new AudioSource[blipPoolSize];
        for (int i = 0; i < blipPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;   // 2D
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.volume = 1f;         // 实际音量用 Play 前设置
            blipPool[i] = src;
        }
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

            if (bubbleRoot.parent != canvasRect)
                bubbleRoot.SetParent(canvasRect, worldPositionStays: false);

            bubbleRoot.anchoredPosition = local;
            bubbleRoot.localRotation = Quaternion.identity;
        }

        // —— 输入（E）：先过输入门槛时间，再处理补完/翻页 ——
        if (Time.time >= inputAllowedAt && Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping && clickToCompleteLine)
            {
                CompleteTypingInstant();        // 打字中 → 立刻补完
            }
            else
            {
                Next();                         // 已打完 → 下一页/结束
            }
        }

        // —— E键提示显隐/翻转 ——
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

        // E 提示
        canPress = true;
        UpdateEHintTypingState(true);
        ApplyEHintActive();

        // 启动第一行
        Next();

        // 输入防抖：避免“同帧 E”直接补完
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

    // —— 打字机 + blip —— 
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        UpdateEHintTypingState(true);      // 正在打字：提示“按E补完”
        textLabel.text = "";
        typedCountOnLine = 0;

        float perCharDelay = 1f / Mathf.Max(1f, charsPerSecond);
        float interval = (blipInterval > 0f) ? blipInterval : (perCharDelay * 0.6f); // 自动限频
        nextBlipTime = 0f; // 立刻允许首个字符发声

        foreach (char c in line)
        {
            textLabel.text += c;
            typedCountOnLine++;

            TryPlayBlip(c, interval);

            yield return new WaitForSeconds(perCharDelay);
        }

        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     // 打完：提示“按E下一页/结束”
    }

    private void TryPlayBlip(char c, float interval)
    {
        if (blipClips == null || blipClips.Length == 0) return;

        // 跳过空白/标点
        if (muteChars.Contains(c.ToString())) return;

        // 触发频率限制 & 每N字符触发一次
        if (Time.time < nextBlipTime) return;
        if (blipEveryNChars > 1 && (typedCountOnLine % blipEveryNChars) != 0) return;

        // 取一个可用池音源（不 Stop 正在播的）
        AudioSource src = null;
        for (int k = 0; k < blipPoolSize; k++)
        {
            int idxPool = (blipPoolCursor + k) % blipPoolSize;
            if (!blipPool[idxPool].isPlaying) { src = blipPool[idxPool]; blipPoolCursor = (idxPool + 1) % blipPoolSize; break; }
        }
        // 如果全在播，强行复用下一个（非常少见）：仍然不会“咔”，但会截断其中一个最旧的
        if (src == null)
        {
            src = blipPool[blipPoolCursor];
            blipPoolCursor = (blipPoolCursor + 1) % blipPoolSize;
        }

        // 随机选择片段 & 音高
        int idx = Random.Range(0, blipClips.Length);
        var clip = blipClips[idx];

        src.clip = clip;
        src.pitch = Random.Range(pitchRandom.x, pitchRandom.y);
        src.volume = blipVolume;

        // 播放（不 Stop，避免点击）
        src.Play();

        // 下次最早触发时间
        nextBlipTime = Time.time + (interval > 0f ? interval : 0.04f);
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
