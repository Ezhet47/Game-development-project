using UnityEngine;
using TMPro;
using System.Collections;

public class Teleport : MonoBehaviour
{
    [Header("Interaction")]
    public InteractionDetect otherScript;
    private bool collide = true;
    private bool playerInRange = false;

    [Header("Requirement")]
    public int requiredTotal = 7;                 // 需要的零件数
    [Tooltip("优先使用 ComponentCount.instance.totalComponents；如果你不想用它，可以在这里手动覆盖当前数量（>=0 生效）。")]
    public int currentCountOverride = -1;         // 可选覆盖（-1 表示不用）

    [Header("UI Hint")]
    public TextMeshProUGUI notEnoughText;         // “需要捡到7个零件” 文本（可选）
    public CanvasGroup notEnoughGroup;            // 可选：用来淡入淡出
    public string notEnoughMessage = "需要捡到7个零件";
    public float hintFade = 0.15f;                // 淡入/淡出时间
    public float hintShowTime = 1.25f;            // 停留时间

    // 与其它系统的兼容（比如对话时禁用交互）
    public bool IsInteractable
    {
        get
        {
            bool dialogueBusy = DialogueUI.Instance != null && DialogueUI.Instance.IsShowing;
            return playerInRange && collide && !dialogueBusy;
        }
    }

    private void Awake()
    {
        // 若有提示 UI，初始化为隐藏
        if (notEnoughGroup == null && notEnoughText != null)
            notEnoughGroup = notEnoughText.GetComponent<CanvasGroup>();

        if (notEnoughGroup != null)
        {
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = true;
        // 是否点亮 E 交互提示由你的 InteractionDetect 控制；这里不强制干预
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = false;
    }

    private void Update()
    {
        if (!(playerInRange && collide)) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int cur = GetCurrentCollected();

            if (cur >= requiredTotal)
            {
                // ? 达标：执行传送
                collide = false;
                if (otherScript) otherScript.canpress = false; // 关掉 E 提示（可选）

                GameManager.Instance.HasPlayedPuzzle = false;
                GameManager.Instance.GoToMainSceneBefore();
            }
            else
            {
                // ? 未达标：提示“需要捡到7个零件”
                ShowNotEnoughHint();
            }
        }
    }

    private int GetCurrentCollected()
    {
        // 1) 若手动覆盖有效，优先用
        if (currentCountOverride >= 0) return currentCountOverride;

        // 2) 默认从 ComponentCount 读取（你项目里有它）
        if (ComponentCount.instance != null)
            return ComponentCount.instance.totalComponents;

        // 3) 找不到来源时，返回 0（避免报错）
        return 0;
    }

    private void ShowNotEnoughHint()
    {
        if (notEnoughText == null && notEnoughGroup == null) return; // 没连 UI 就静默返回

        // 确保文本内容
        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;

        StopAllCoroutines();
        StartCoroutine(CoFlashHint());
    }

    private IEnumerator CoFlashHint()
    {
        if (notEnoughGroup != null)
        {
            notEnoughGroup.blocksRaycasts = true;
            notEnoughGroup.interactable = true;

            // 淡入
            float t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(0f, 1f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 1f;

            // 停留
            yield return new WaitForSeconds(hintShowTime);

            // 淡出
            t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(1f, 0f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        else if (notEnoughText != null)
        {
            // 没有 CanvasGroup 就直接闪现一小会儿
            notEnoughText.gameObject.SetActive(true);
            yield return new WaitForSeconds(hintShowTime);
            notEnoughText.gameObject.SetActive(false);
        }
    }
}
