using UnityEngine;
using TMPro;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI text;            // 可在 Inspector 指定；若为空，脚本会自动查找子物体里的 TMP
    public float displayTime = 5f;
    public CanvasGroup canvasGroup;         // 可在 Inspector 指定；若为空会自动获取或创建

    [Header("Auto Start")]
    [TextArea] public string startMessage = "Welcome!";
    public bool showOnStart = true;

    private Coroutine currentRoutine;
    private bool isShowing = false;

    // 缓存玩家引用，用来控制 canMove
    private Player cachedPlayer;

    void Awake()
    {
        // 自动查找 TextMeshProUGUI（在子物体上）
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        // 自动获取或创建 CanvasGroup（通常在同一物体上）
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 初始隐藏
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        // 尝试缓存 Player（若 Start 时 Player 尚未 Awake，也会在 Show 时再次查找）
        cachedPlayer = FindObjectOfType<Player>();

        if (showOnStart)
            Show(startMessage);
    }

    public void Show(string msg)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // 如果 text 还是 null，再次尝试查找（以防你在运行时才创建子对象）
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        StopAllCoroutines();
        if (text != null) text.text = msg;
        currentRoutine = StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        isShowing = true;

        // 禁止玩家移动（若找到 player）
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = false;

        // 启用交互阻挡（防止玩家点击场景）
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // 淡入
        float t = 0f;
        float fadeIn = 0.25f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // ? 显示计时：只能按下 E 键 才关闭
        float timer = 0f;
        while (timer < displayTime && isShowing)
        {
            if (Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(0)) // ← 修改处
                break;
            timer += Time.deltaTime;
            yield return null;
        }

        // 淡出
        t = 0f;
        float fadeOut = 0.25f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        // 隐藏 UI
        gameObject.SetActive(false);
        isShowing = false;

        // 恢复玩家移动
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;

        // 恢复交互阻挡
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// 外部可以调用取消当前提示并恢复玩家控制
    /// </summary>
    public void AbortAndHide()
    {
        StopAllCoroutines();
        isShowing = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        gameObject.SetActive(false);

        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;
    }
}
